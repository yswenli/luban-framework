/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： AgentHost
*版本号： V1.0.0.0
*唯一标识：09dba94c-8c5c-45d1-9721-66b0a4f0e67e
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：Agent 宿主默认实现：按会话路由到会话运行时并输出 UI 无关事件
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.1
*描述：附件超 MaxCount 改为截断 + 跳过事件（原实现抛异常会导致整轮对话失败）
*
*****************************************************************************/
using LuBan.AIAgent.Attachments;
namespace LuBan.AIAgent.Hosting;

/// <summary>
/// Agent 宿主：无状态路由器，按 sessionId 路由到对应会话运行时执行，输出 UI 无关的 <see cref="AgentStreamEvent"/> 序列。
/// 所有 per-session 运行态均在 <see cref="SessionRuntime"/> 的会话 scope 内，宿主自身不持有会话状态。
/// </summary>
public sealed class AgentHost(
    DiContainer services,
    SessionRuntimeManager runtimes,
    IAgentWorkspaceResolver workspaceResolver,
    IAgentProfileResolver profileResolver,
    IAgentUserResolver userResolver,
    IAgentModelSelection modelSelection,
    IAgentAttachmentOrchestrator attachments,
    IAgentAttachmentSourceResolver attachmentSources,
    IToolConfirmationBroker confirmations,
    IOptions<LuBanAgentOptions> options) : IAgentHost
{
    /// <inheritdoc />
    public DiContainer Services => services;

    /// <inheritdoc />
    public string? CurrentSessionId => services.GetService<ISessionManager>()?.CurrentSession?.SessionId;

    /// <inheritdoc />
    public bool IsInitialized => CurrentSessionId is { Length: > 0 } id && runtimes.Get(id)?.Agent is not null;

    /// <inheritdoc />
    public async Task InitializeAsync(
        string sessionId, CancellationToken ct = default, AgentBuildOverrides? overrides = null)
    {
        var workspace = await workspaceResolver.ResolveAsync(sessionId, ct).ConfigureAwait(false);
        var runtime = runtimes.Get(sessionId)
            ?? runtimes.GetOrCreate(
                sessionId, userResolver.ResolveUserId(services), workspace.Id, workspace.RootPath);

        await EnsureAgentAsync(runtime, workspace, ct, NormalizeOverrides(overrides)).ConfigureAwait(false);
    }

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
        runtime.BuildOverrides = null;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AgentStreamEvent> RunStreamingAsync(AgentRunRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return RunStreamingCoreAsync(request, ct);
    }

    /// <summary>取（或创建）会话运行时；<b>不</b>建 Agent（建 Agent 下沉到回合串行区间）。</summary>
    /// <param name="sessionId">会话标识。</param>
    /// <param name="workspace">本次解析到的工作区（仅创建运行时时写入）。</param>
    /// <returns>会话运行时。</returns>
    private SessionRuntime GetRuntime(string sessionId, WorkspaceDescriptor workspace)
        => runtimes.Get(sessionId)
           ?? runtimes.GetOrCreate(
               sessionId, userResolver.ResolveUserId(services), workspace.Id, workspace.RootPath);

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

        // 请求线程只做「解析工作区 + 附件前置 + 取运行时」；建 Agent 下沉到回合串行区间（C12）。
        // 工作区必须先解析：附件相对路径以其根目录为第一基准。
        var workspace = await workspaceResolver.ResolveAsync(sid, ct).ConfigureAwait(false);

        AttachmentPreparation? preparation = null;
        string? preparationFailure = null;
        try
        {
            preparation = await PrepareAttachmentsAsync(request, workspace, sid, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            preparationFailure = $"附件处理失败: {ex.Message}";
        }

        if (preparationFailure is not null)
        {
            yield return new ErrorEvent(preparationFailure);
            yield break;
        }

        foreach (var evt in preparation!.Events)
            yield return evt;

        var processed = preparation.Attached;

        if (attachments.Validate(modelId, processed) is { } unsupported)
        {
            yield return new ErrorEvent(unsupported);
            yield break;
        }

        var hasImage = processed.Any(a => a.Info.Kind == AttachmentKind.Image);
        var probe = attachments.CreateProbe(modelId, hasImage);

        // 附件全部就绪后才创建会话运行时：附件失败不应留下空转的 scope
        var runtime = GetRuntime(sid, workspace);

        if (probe is null || !probe.IsProbing)
        {
            await foreach (var evt in RunInRuntimeAsync(runtime, workspace, request, processed, ct))
                yield return evt;
            yield break;
        }

        // 首次带图调用：由 LLM 实际结果回填模型视觉能力（成功记 true；命中「图片不支持」错误记 false，仅记录不重发）
        Exception? streamError = null;
        var completed = false;

        var enumerator = RunInRuntimeAsync(runtime, workspace, request, processed, ct).GetAsyncEnumerator(ct);
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
    /// 附件前置：来源解析 → 逐项处理 → 投影成事件。任何单文件失败都只跳过，不终止本轮。
    /// </summary>
    /// <param name="request">本轮请求。</param>
    /// <param name="workspace">当前工作区（相对路径基准）。</param>
    /// <param name="sessionId">会话标识（URL 落盘子目录）。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task<AttachmentPreparation> PrepareAttachmentsAsync(
        AgentRunRequest request, WorkspaceDescriptor workspace, string sessionId, CancellationToken ct)
    {
        // 显式来源优先，@引用/UI 路径一并走同一套解析（本地路径在这里是原样命中）
        var sources = request.Attachments
            .Concat(request.AttachmentPaths)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (sources.Count == 0)
            return AttachmentPreparation.Empty;

        var settings = options.Value.Attachments;
        if (!settings.Enabled)
            throw new InvalidOperationException("附件功能未启用");

        // 超出上限不整轮失败：保留前 N 个（UI 附件在前，与合并口径一致），
        // 其余折成跳过项下发。正文 @引用 与 UI 附件合并后很容易超限，
        // 早期实现此处直接抛异常，导致「附件满额 + 正文再写一个 @文件」整轮无回复。
        var overflow = new List<AttachmentSourceFailure>();
        if (settings.MaxCount > 0 && sources.Count > settings.MaxCount)
        {
            var reason = AttachmentEventProjector.MaxCountExceededReason(settings.MaxCount);
            overflow.AddRange(sources.Skip(settings.MaxCount)
                .Select(source => new AttachmentSourceFailure(source, reason)));
            sources = sources.Take(settings.MaxCount).ToList();
        }

        var context = new AttachmentSourceContext(workspace.RootPath, settings.ContentRoot, sessionId);
        var sourcesResult = await attachmentSources.ResolveAsync(sources, context, ct).ConfigureAwait(false);

        var processing = sourcesResult.Resolved.Count == 0
            ? new AttachmentProcessingResult(
                Array.Empty<ProcessedAttachment>(), Array.Empty<AttachmentSourceFailure>())
            : await attachments.ProcessAsync(
                sourcesResult.Resolved.Select(r => r.LocalPath).ToList(), ct).ConfigureAwait(false);

        // 把「本地路径」映射回「调用方给的原始来源」，事件里必须是原始来源而不是落盘名
        var sourceByLocal = sourcesResult.Resolved
            .GroupBy(r => Path.GetFullPath(r.LocalPath), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Source, StringComparer.OrdinalIgnoreCase);

        var processed = new List<ProcessedAttachment>(processing.Processed.Count);
        foreach (var item in processing.Processed)
        {
            var local = Path.GetFullPath(item.Info.SourcePath);
            processed.Add(sourceByLocal.TryGetValue(local, out var origin)
                ? item.WithSource(origin)
                : item.WithSource(item.Info.SourcePath));
        }

        var failures = sourcesResult.Failed.Concat(processing.Failed).Concat(overflow).ToList();
        var projection = AttachmentEventProjector.Project(processed, failures, request.EnableTools);

        var events = new List<AgentStreamEvent>(projection.Attached.Count + projection.Skipped.Count);
        foreach (var item in projection.Attached)
        {
            events.Add(new AttachmentAddedEvent(
                item.Info.FileName, item.Info.MediaType, item.Info.FileSize, item.Source));
        }
        events.AddRange(projection.Skipped);

        return new AttachmentPreparation(projection.Attached, events);
    }

    /// <summary>
    /// 把一轮对话入队到指定会话运行时串行执行，并以 Channel 把回合内产生的事件回传为异步流。
    /// </summary>
    private async IAsyncEnumerable<AgentStreamEvent> RunInRuntimeAsync(
        SessionRuntime runtime,
        WorkspaceDescriptor workspace,
        AgentRunRequest request,
        IReadOnlyList<ProcessedAttachment> processed,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var channel = Channel.CreateUnbounded<AgentStreamEvent>();
        var overrides = NormalizeOverrides(request);

        // 只有调用方没自带确认回调、且本轮装配了工具时，才接 broker。
        // 确认回调在「回合内」写事件：ToolConfirmationRequestedEvent 必须跟着本回合的 SSE 流走。
        AgentRunRequest effective = request;
        if (request.EnableTools
            && request.ConfirmHandler is null
            && request.ConfirmHandlerAsync is null
            && options.Value.Confirmation.BrokerEnabled)
        {
            effective = request with
            {
                ConfirmHandlerAsync = async (toolName, arguments, token) =>
                {
                    var pending = confirmations.Register(runtime.SessionId, toolName, arguments);
                    channel.Writer.TryWrite(new ToolConfirmationRequestedEvent(
                        pending.ConfirmationId, toolName, arguments, confirmations.TimeoutSeconds));
                    return await confirmations.WaitAsync(pending, token).ConfigureAwait(false);
                }
            };
        }

        await runtime.EnqueueAsync(new RuntimeTurn
        {
            Text = request.Input,
            CancellationToken = ct,
            RunAsync = async (sessionContext, token) =>
            {
                try
                {
                    // 覆盖项比对与重建必须在串行区间内：此时上一回合已结束，
                    // Dispose 旧 Agent 不会命中正在使用中的实例（C12）。
                    await EnsureAgentAsync(runtime, workspace, token, overrides).ConfigureAwait(false);

                    await foreach (var evt in RunCoreAsync(runtime, sessionContext, effective, processed, token))
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

        // 取消（Esc/新会话）时不能立刻结束事件流：回合委托可能仍在执行（如不可中断的工具），
        // 而会话运行时的串行队列仍被该回合占着。此时若 UI 立刻复位为"空闲"，
        // 用户接着发的下一条消息只会排队，表现为"显示运行中但文字不再增长"的假停。
        while (true)
        {
            bool hasMore;
            try
            {
                hasMore = await channel.Reader.WaitToReadAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // 不再向 UI 推送事件，但要等回合真正收尾（有宽限期，避免死等不可取消的工具）
                await AwaitTurnTeardownAsync(channel.Reader, runtime.SessionId).ConfigureAwait(false);
                throw;
            }

            if (!hasMore)
                yield break;

            while (channel.Reader.TryRead(out var evt))
                yield return evt;
        }
    }

    /// <summary>取消后等待回合真正收尾的宽限期。</summary>
    private static readonly TimeSpan TurnTeardownGrace = TimeSpan.FromSeconds(15);

    /// <summary>
    /// 取消后把本回合剩余事件读空并丢弃，直到回合委托真正结束（或超过宽限期）。
    /// 目的是让宿主知道"上一回合已经彻底收尾"，而不是在残留回合仍占着串行队列时就宣告空闲。
    /// </summary>
    /// <param name="reader">回合事件通道读取端。</param>
    /// <param name="sessionId">会话标识（用于日志）。</param>
    private static async Task AwaitTurnTeardownAsync(ChannelReader<AgentStreamEvent> reader, string sessionId)
    {
        using var grace = new CancellationTokenSource(TurnTeardownGrace);
        try
        {
            while (await reader.WaitToReadAsync(grace.Token).ConfigureAwait(false))
            {
                while (reader.TryRead(out _))
                {
                    // 丢弃：UI 已放弃本回合输出
                }
            }
        }
        catch (OperationCanceledException) when (grace.IsCancellationRequested)
        {
            // 只有宽限期真正到点才告警；否则是回合自身的取消异常经通道完成态重抛（见下）
            Logger.Warn(
                $"回合取消后未在 {TurnTeardownGrace.TotalSeconds:0}s 内收尾（可能存在不可取消的工具），" +
                $"该会话后续消息将继续排队: {sessionId}");
        }
        catch (Exception ex)
        {
            // 通道以异常完成（回合内抛出，通常就是本次取消本身）：收尾异常不影响取消语义
            Logger.Info($"回合取消后收尾期间通道异常结束（预期为取消）: {sessionId} {ex.GetType().Name}");
        }
    }

    /// <summary>确保指定会话运行时内已按工作区构建 Agent。</summary>
    private async Task EnsureAgentAsync(
        SessionRuntime runtime, WorkspaceDescriptor workspace, CancellationToken ct,
        AgentBuildOverrides? overrides = null)
    {
        if (runtime.Agent is not null
            && runtime.Workspace?.Id == workspace.Id
            && (runtime.BuildOverrides?.Matches(overrides) ?? overrides is null))
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
            ruleEngine, pluginRegistry, skillRegistry, mcpRegistry, ct, overrides).ConfigureAwait(false);

        // 释放旧 Agent 实例，避免重建时连接/句柄等资源泄漏
        if (runtime.Agent is IDisposable oldAgent)
        {
            try { oldAgent.Dispose(); }
            catch { /* 忽略释放异常，避免影响新 Agent 使用 */ }
        }
        runtime.Agent = newAgent;
        runtime.Workspace = workspace;
        runtime.BuildOverrides = overrides;
    }

    /// <summary>把「全默认」覆盖项归一化成 null，避免每轮拿 non-null 去比缓存里的 null 而误重建。</summary>
    /// <param name="request">本轮请求。</param>
    /// <returns>归一化后的覆盖项。</returns>
    private static AgentBuildOverrides? NormalizeOverrides(AgentRunRequest request)
        => NormalizeOverrides(new AgentBuildOverrides
        {
            SystemPrompt = request.SystemPrompt,
            EnableTools = request.EnableTools
        });

    /// <summary>把「全默认」覆盖项归一化成 null。</summary>
    /// <param name="overrides">覆盖项。</param>
    /// <returns>归一化后的覆盖项。</returns>
    private static AgentBuildOverrides? NormalizeOverrides(AgentBuildOverrides? overrides)
        => overrides is null || overrides.IsDefault ? null : overrides;

    /// <summary>
    /// 把本轮推理强度覆盖转成运行选项；null 表示沿用 Agent 自身配置。
    /// </summary>
    /// <param name="request">本轮请求。</param>
    /// <returns>运行选项或 null。</returns>
    /// <remarks>
    /// 运行选项会与 Agent 建时选项合并（Instructions / Tools / Temperature 等保留），
    /// 因此关闭思考不需要重建 Agent。
    /// </remarks>
    private static ChatClientAgentRunOptions? BuildRunOptions(AgentRunRequest request)
        => request.ReasoningEffort is { } effort
            ? new ChatClientAgentRunOptions
            {
                ChatOptions = new ChatOptions
                {
                    Reasoning = new ReasoningOptions { Effort = effort }
                }
            }
            : null;

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
        var asyncConfirm = request.ConfirmHandlerAsync
            ?? (request.ConfirmHandler is { } sync
                ? (name, args, _) => Task.FromResult(sync(name, args))
                : null);

        context.ConfigureForTurnAsync(
            request.PermissionMode,
            ct,
            asyncConfirm,
            sessionContext: sessionContext,
            onPlannedAction: request.OnPlannedAction,
            onUserDenied: request.OnUserDenied);

        try
        {
            // 本轮推理强度覆盖（如快捷对话关闭思考）走运行选项，不重建 Agent
            var runOptions = BuildRunOptions(request);
            var updates = processed.Count == 0
                ? agent.RunStreamingAsync(request.Input, runOptions, ct)
                : agent.RunStreamingAsync(request.Input, processed, runOptions, ct);
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