/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools
*文件名： RequiredArgumentGuardAIFunction
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：必填参数守卫装饰器，缺参时返回中文可自纠结果而非抛原始英文异常
*
*****************************************************************************/
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Tools;

/// <summary>
/// 必填参数守卫装饰器：按工具 JSON Schema 校验必填参数，缺失或空白时直接返回
/// 中文可自纠结果（ToolResult），避免 Microsoft.Extensions.AI 参数绑定抛出
/// <c>ArgumentException: The arguments dictionary is missing a value for the required parameter ...</c>。
/// </summary>
/// <remarks>
/// 背景：模型（尤其长上下文/多工具场景）偶发漏传必填参数。若不拦截，
/// 非路径类必填参数（如 RunShell 的 command）会直接抛英文异常，模型无法据此自纠；
/// 路径类必填参数则被工作区根兜底静默替换，产生"路径被误识别为目录"等误导结果。
/// </remarks>
public class RequiredArgumentGuardAIFunction : DelegatingAIFunction
{
    private readonly IReadOnlyList<string> _requiredParameters;

    /// <summary>
    /// 创建必填参数守卫装饰器
    /// </summary>
    /// <param name="innerFunction">被装饰的工具函数</param>
    public RequiredArgumentGuardAIFunction(AIFunction innerFunction)
        : base(innerFunction)
    {
        _requiredParameters = ExtractRequiredParameters(innerFunction.JsonSchema);
    }

    /// <summary>
    /// 调用前校验必填参数；缺失时返回中文提示给模型，否则透传执行。
    /// </summary>
    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var missing = _requiredParameters.Where(p => IsMissing(arguments, p)).ToList();
        if (missing.Count > 0)
        {
            var providedKeys = string.Join(", ", arguments.Keys);
            Logger.Warn(
                $"工具 {Name} 缺少必填参数 [{string.Join(", ", missing)}]，已拦截；"
                + $"模型实际传入的参数键: [{providedKeys}]");

            return new ToolResult
            {
                IsSuccess = false,
                Message = $"调用 {Name} 失败：缺少必填参数 {string.Join("、", missing)}。"
                    + "请核对参数名与取值后重新调用；不要以相同参数重复重试。"
            };
        }

        return await base.InvokeCoreAsync(arguments, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 从 JSON Schema 提取 required 参数名列表。
    /// </summary>
    private static IReadOnlyList<string> ExtractRequiredParameters(JsonElement schema)
    {
        if (schema.ValueKind != JsonValueKind.Object
            || !schema.TryGetProperty("required", out var required)
            || required.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var names = new List<string>();
        foreach (var item in required.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } name)
            {
                names.Add(name);
            }
        }
        return names;
    }

    /// <summary>
    /// 判断必填参数是否缺失（键不存在、值为 null、或字符串为空白）。
    /// </summary>
    private static bool IsMissing(AIFunctionArguments arguments, string name)
    {
        if (!TryGetValue(arguments, name, out var value))
            return true;

        return value switch
        {
            null => true,
            string s => string.IsNullOrWhiteSpace(s),
            JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } => true,
            JsonElement { ValueKind: JsonValueKind.String } je => string.IsNullOrWhiteSpace(je.GetString()),
            _ => false
        };
    }

    /// <summary>
    /// 大小写不敏感地取值（AIFunctionArguments 的键通常已与参数名一致，此处仅作兜底）。
    /// </summary>
    private static bool TryGetValue(AIFunctionArguments arguments, string name, out object? value)
    {
        if (arguments.TryGetValue(name, out value))
            return true;

        foreach (var kv in arguments)
        {
            if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                value = kv.Value;
                return true;
            }
        }

        value = null;
        return false;
    }
}