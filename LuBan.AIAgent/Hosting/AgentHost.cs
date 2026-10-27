/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： AgentHost
*版本号： V1.0.0.0
*唯一标识：宿主默认实现
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：Agent 宿主默认实现：按会话路由到会话运行时并输出 UI 无关事件
*
*****************************************************************************/using LuBan.AIAgent.Attachments;

namespace LuBan.AIAgent.Hosting;

/// <summary>
/// Agent 宿主：无状态路由器，按 sessionId 路由到对应会话运行时执行，输出 UI 无关的 <see cref="AgentStreamEvent"/> 序列。
/// 所有 per-session 运行态均在 <see cref="SessionRuntime"/> 的会话 scope 内，宿主自身不持有会话状态。
/// </summary>
public sealed class AgentHost(
    IServiceProvider services,
    SessionRuntimeManager runtimes,
    IAgentWorkspaceResolver workspaceResolver,
    IAgentProfileResolver profileResolver,
    IAgentUserResolver userResolver,
    IAgentModelSelection modelSelection,
    IAgentAttachmentOrchestrator attachments) : IAgentHost
{
    /// <inheritdoc />
    public IServiceProvider Services => services;

    /// <inheritdoc />
    public string? CurrentSessionId => services.GetService<ISessionManager>()?.CurrentSession?.SessionId;

    /// <inheritdoc />
    public bool IsInitialized => CurrentSessionId is { Length: > 0 } id && runtimes.Get(id)?.Agent is not null;

    /// <inheritdoc />
    public async Task InitializeAsync(string sessionId, CancellationToken ct = default)
        => await EnsureRuntimeAsync(sessionId, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public void Reset(string? sessionId = null)
    {
        var id = sessionId ?? CurrentSessionId;
        if (id is not { Length: > 0 }) return;

        var runtime = runtimes.Get(id);
        if (runtime is null) return;

        // 回合运行中释放 Agent 会让正在执行的回合持有已释放/空实例：
        // 改为打延迟重载标记，由回合收尾时释放，下一次对话按新配置重建。
        if (runtime.IsRunning)
        {
            runtime.RequestReload();
            return;
        }

        DetachAgent(runtime);
    }

    /// <summary>释放会话运行时缓存的 Agent 并解绑工作区（下一次对话按当前配置与工作区重建）。</summary>
    /// <remarks>
    /// 调用方必须保证此刻没有回合正在使用该 Agent；运行中的释放走 <see cref="SessionRuntime.RequestReload"/> 延后路径。
    /// </remarks>
    private static void DetachAgent(SessionRuntime runtime)
    {
        if (runtime.Agent is IDisposable oldAgent)
        {
            try { oldAgent.Dispose(); }
            catch { /* 忽略释放异常，避免影响后续重建 */ }
        }
        runtime.Agent = null;
        runtime.Workspace = null;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AgentStreamEvent> RunStreamingAsync(AgentRunRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return RunStreamingCoreAsync(request, ct);
    }

    /// <summary>
    /// 取（或创建）指定会话的运行时，并确保其 Agent 已按该会话自身绑定的工作区构建。
    /// </summary>
    private async Task<SessionRuntime> EnsureRuntimeAsync(string sessionId, CancellationToken ct)
    {
        var runtime = runtimes.Get(sessionId);
        if (runtime?.Agent is not null)
            return runtime;

        var workspace = await workspaceResolver.ResolveAsync(sessionId, ct).ConfigureAwait(false);
        runtime = runtimes.GetOrCreate(sessionId, userResolver.ResolveUserId(services), workspace.Id, workspace.RootPath);
        await EnsureAgentAsync(runtime, workspace, ct).ConfigureAwait(false);
        return runtime;
    }

    /// <summary>运行一轮对话的主体（异步迭代器：参数/配置异常在首次 MoveNext 时抛出）。</summary>
    private async IAsyncEnumerable<AgentStreamEvent> RunStreamingCoreAsync(
        AgentRunRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var sid = request.SessionId ?? CurrentSessionId
            ?? throw new InvalidOperationException("未设置当前会话");

        var modelId = modelSelection.QualifiedModelName;
        if (string.IsNullOrWhiteSpace(modelId))
            throw new InvalidOperationException("未选择模型");

        // 附件路径归一：UI 传入路径（拖拽/选择器）+ 正文 @引用 合并去重，正文剥离引用。
        // 解析规则由产品实现（IAgentAttachmentOrchestrator），故所有宿主口径一致。
        var resolved = AgentAttachmentResolver.Resolve(request, attachments);
        request = request with { Input = resolved.Text, AttachmentPaths = resolved.Paths };

        IReadOnlyList<ProcessedAttachment> processed = Array.Empty<ProcessedAttachment>();
        if (request.AttachmentPaths.Count > 0)
        {
            string? failure = null;
            try
            {
                processed = await attachments.ProcessAsync(request.AttachmentPaths, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                failure = $"附件处理失败: {ex.Message}";
            }

            if (failure != null)
            {
                yield return new ErrorEvent(failure);
                yield break;
            }

            if (attachments.Validate(modelId, processed) is { } unsupported)
            {
                yield return new ErrorEvent(unsupported);
                yield break;
            }

            foreach (var a in processed)
                yield return new AttachmentAddedEvent(a.Info.FileName, a.Info.MediaType, a.Info.FileSize);
        }

        var runtime = await EnsureRuntimeAsync(sid, ct);

        var hasImage = processed.Any(a => a.Info.Kind == AttachmentKind.Image);
        var probe = attachments.CreateProbe(modelId, hasImage);

        if (probe is null || !probe.IsProbing)
        {
            await foreach (var evt in RunInRuntimeAsync(runtime, request, processed, ct))
                yield return evt;
            yield break;
        }

        // 首次带图调用：由 LLM 实际结果回填模型视觉能力（成功记 true；命中「图片不支持」错误记 false，仅记录不重发）
        Exception? streamError = null;
        var completed = false;

        var enumerator = RunInRuntimeAsync(runtime, request, processed, ct).GetAsyncEnumerator(ct);
        try
        {
            while (true)
            {
                AgentStreamEvent current;
                try
                {
                    if (!await enumerator.MoveNextAsync())
                    {
                        completed = true;
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception ex)
                {
                    streamError = ex;
                    throw;
                }

                if (current is ErrorEvent errorEvent)
                    probe.Observe(errorEvent.Message);

                yield return current;
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
            probe.Complete(streamError, completed);
        }
    }

    /// <summary>
    /// 把一轮对话入队到指定会话运行时串行执行，并以 Channel 把回合内产生的事件回传为异步流。
    /// </summary>
    private async IAsyncEnumerable<AgentStreamEvent> RunInRuntimeAsync(
        SessionRuntime runtime,
        AgentRunRequest request,
        IReadOnlyList<ProcessedAttachment> processed,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var channel = Channel.CreateUnbounded<AgentStreamEvent>();

        await runtime.EnqueueAsync(new RuntimeTurn
        {
            Text = request.Input,
            CancellationToken = ct,
            RunAsync = async (sessionContext, token) =>
            {
                try
                {
                    await foreach (var evt in RunCoreAsync(runtime, sessionContext, request, processed, token))
                        channel.Writer.TryWrite(evt);
                    channel.Writer.TryComplete();
                }
                catch (Exception ex)
                {
                    channel.Writer.TryComplete(ex);
                }
                finally
                {
                    // 回合收尾时本回合尚未退出运行循环（IsRunning 仍为 true），
                    // 故不能回调 Reset（会被再次延后而永久搁置），直接释放让下次对话重建。
                    if (runtime.ConsumePendingReload())
                        DetachAgent(runtime);
                }
            }
        }, ct);

        await foreach (var evt in channel.Reader.ReadAllAsync(ct))
            yield return evt;
    }

    /// <summary>确保指定会话运行时内已按工作区构建 Agent。</summary>
    private async Task EnsureAgentAsync(SessionRuntime runtime, WorkspaceDescriptor workspace, CancellationToken ct)
    {
        if (runtime.Agent is not null && runtime.Workspace?.Id == workspace.Id)
            return;

        var sp = runtime.Services;
        var agentFactory = sp.GetRequiredService<ILuBanAgentFactory>();
        var ruleEngine = sp.GetRequiredService<RuleEngine>();
        var pluginRegistry = sp.GetRequiredService<ToolPluginRegistry>();
        var skillRegistry = sp.GetRequiredService<SkillRegistry>();
        var mcpRegistry = sp.GetRequiredService<MCPRegistry>();

        // 模型从注入的 IAgentModelSelection（用户 scope）取，避免会话 scope 拿错用户配置
        var modelName = modelSelection.QualifiedModelName;
        if (string.IsNullOrWhiteSpace(modelName))
            throw new InvalidOperationException("未选择模型");

        // 会话级扩展回放：插件贡献、连接器启用等由产品实现承接；失败不阻断会话构建。
        foreach (var extension in sp.GetServices<IAgentSessionExtension>())
        {
            try
            {
                await extension.ApplyAsync(sp, workspace, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Warn($"会话扩展执行失败: {extension.GetType().Name}", ex);
            }
        }

        var profile = profileResolver.Resolve(workspace);

        var newAgent = await profile.CreateAgentAsync(
            agentFactory, modelName, workspace,
            ruleEngine, pluginRegistry, skillRegistry, mcpRegistry, ct).ConfigureAwait(false);

        // 释放旧 Agent 实例，避免重建时连接/句柄等资源泄漏
        if (runtime.Agent is IDisposable oldAgent)
        {
            try { oldAgent.Dispose(); }
            catch { /* 忽略释放异常，避免影响新 Agent 使用 */ }
        }
        runtime.Agent = newAgent;
        runtime.Workspace = workspace;
    }

    private async IAsyncEnumerable<AgentStreamEvent> RunCoreAsync(
        SessionRuntime runtime,
        ISessionContext sessionContext,
        AgentRunRequest request,
        IReadOnlyList<ProcessedAttachment> processed,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var sp = runtime.Services;

        // 运行时绑定即权威来源，不再按进程级“当前工作区”重建（避免跨工作区串扰）。
        var agent = runtime.Agent
            ?? throw new InvalidOperationException("Agent 初始化失败，请先调用 InitializeAsync");

        var context = sp.GetRequiredService<ToolConfirmationContext>();

        // 统一装配：模式策略由框架分发，回调仅在需要人工确认时被调用；
        // 授权记忆（本轮/本会话）由框架依据返回值处理。
        // 路径检查器基于本会话上下文（EffectiveRoot/AllowedRoots，含 worktree），实现会话级隔离。
        context.ConfigureForTurn(
            request.PermissionMode,
            ct,
            request.ConfirmHandler,
            sessionContext: sessionContext,
            onPlannedAction: request.OnPlannedAction,
            onUserDenied: request.OnUserDenied);

        try
        {
            var updates = processed.Count == 0
                ? agent.RunStreamingAsync(request.Input, ct)
                : agent.RunStreamingAsync(request.Input, processed, ct);
            await foreach (var update in updates)
            {
                if (update.Contents is null) continue;

                // 框架已在每个更新上标注 LLM 轮次号（工具结果边界 +1）：UI 据此按“同一轮回复”归并思考块
                var llmRound = update.AdditionalProperties is { } roundProps
                    && roundProps.TryGetValue(LuBanAgent.LlmRoundKey, out var roundValue)
                    && roundValue is int r
                        ? r
                        : 0;

                foreach (var content in update.Contents)
                {
                    // 编排进度：规划与节点执行均为长耗时非流式过程，
                    // 转为进度事件供 UI 实时反馈（否则此阶段内容区长时间空白）
                    if (content is OrchestrationProgressContent progress)
                    {
                        yield return OrchestrationProgressEvent.Create(
                            progress.EventType, progress.NodeId, progress.Message,
                            progress.ElapsedMs, progress.NodeResult, progress.Activity)
                            with { LlmRound = llmRound };
                        continue;
                    }

                    // 思考过程
                    if (content is TextReasoningContent reasoning && !string.IsNullOrEmpty(reasoning.Text))
                    {
                        yield return new ThinkingDeltaEvent(reasoning.Text) { LlmRound = llmRound };
                        continue;
                    }

                    // 工具调用
                    if (content is FunctionCallContent functionCall)
                    {
                        var args = functionCall.Arguments != null
                            ? new Dictionary<string, object?>(functionCall.Arguments)
                            : new Dictionary<string, object?>();
                        yield return new ToolCallStartedEvent(
                            functionCall.Name,
                            functionCall.CallId,
                            args) { LlmRound = llmRound };
                        continue;
                    }

                    // 工具结果
                    if (content is FunctionResultContent functionResult)
                    {
                        if (functionResult.Exception is not null)
                        {
                            yield return new ToolCallFailedEvent(
                                functionResult.CallId,
                                functionResult.Exception.Message) { LlmRound = llmRound };
                        }
                        else if (ToolResult.IsUserCancelled(functionResult.Result))
                        {
                            // 拒绝与取消在结果层不可区分：用中性文案，宿主聊天流已分别提示
                            yield return new ToolCallFailedEvent(functionResult.CallId, "工具调用被拒绝或已取消") { LlmRound = llmRound };
                        }
                        else
                        {
                            yield return new ToolCallCompletedEvent(
                                functionResult.CallId,
                                SummarizeResult(functionResult.Result)) { LlmRound = llmRound };
                        }
                        continue;
                    }

                    // 正文回复
                    if (content is TextContent text && !string.IsNullOrEmpty(text.Text))
                    {
                        yield return new TextDeltaEvent(text.Text) { LlmRound = llmRound };
                        continue;
                    }

                    // 错误
                    if (content is ErrorContent error)
                    {
                        yield return new ErrorEvent(error.Message) { LlmRound = llmRound };
                        continue;
                    }

                    // UsageContent 静默跳过
                }
            }
        }
        finally
        {
            context.Reset();
        }
    }

    /// <summary>
    /// 提取工具结果摘要：截断至 200 字符，超长末尾加 "…"。
    /// 处理 string / JsonElement / ToolResult / 其它对象（ToString 兜底）。
    /// </summary>
    /// <param name="result">工具返回值。</param>
    /// <returns>摘要文本；无内容返回 null。</returns>
    private static string? SummarizeResult(object? result)
    {
        if (result is null) return null;

        string? text;
        try
        {
            // 优先处理 JsonElement：ToString 输出紧凑 JSON
            if (result is System.Text.Json.JsonElement je)
            {
                text = je.ValueKind == System.Text.Json.JsonValueKind.String
                    ? je.GetString()
                    : je.ToString();
            }
            // 处理 ToolResult：优先取 Message，其次用 ToString 兜底
            else if (result is LuBan.AIAgent.Abstractions.ToolResult tr)
            {
                text = tr.Message ?? tr.ToString();
            }
            else
            {
                text = result as string ?? result.ToString();
            }
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(text)) return null;

        const int MaxLength = 200;
        return text.Length <= MaxLength
            ? text
            : text[..MaxLength] + "…";
    }
}