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
    private readonly IReadOnlySet<string> _stringParameters;

    /// <summary>
    /// 创建必填参数守卫装饰器
    /// </summary>
    /// <param name="innerFunction">被装饰的工具函数</param>
    public RequiredArgumentGuardAIFunction(AIFunction innerFunction)
        : base(innerFunction)
    {
        _requiredParameters = ExtractRequiredParameters(innerFunction.JsonSchema);
        _stringParameters = ExtractStringParameters(innerFunction.JsonSchema);
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
    /// 判断必填参数是否缺失。
    /// </summary>
    /// <remarks>
    /// 参数值来自模型返回的 JSON 反序列化，运行时通常是 <see cref="JsonElement"/> 而非 string，
    /// 故统一走 <see cref="ToolArgumentValue"/> 解释，与路径参数绑定保持同一套判断。
    /// 仅对 Schema 声明为 string 的必填参数附加"空白或不可解释为字符串即缺参"的校验；
    /// 其他类型（数字/对象/数组等）保持"存在且非 null 即满足"，避免误伤 MCP 等结构化参数。
    /// </remarks>
    private bool IsMissing(AIFunctionArguments arguments, string name)
    {
        if (!ToolArgumentValue.TryGet(arguments, name, out var value))
            return true;

        if (value is null || value is JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
            return true;

        if (_stringParameters.Contains(name))
        {
            return !ToolArgumentValue.TryGetString(value, out var text) || string.IsNullOrWhiteSpace(text);
        }

        return false;
    }

    /// <summary>
    /// 从 JSON Schema 提取"类型为 string"的属性名集合（兼容 type 为 ["string","null"] 的联合类型）。
    /// </summary>
    private static IReadOnlySet<string> ExtractStringParameters(JsonElement schema)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (schema.ValueKind != JsonValueKind.Object
            || !schema.TryGetProperty("properties", out var properties)
            || properties.ValueKind != JsonValueKind.Object)
        {
            return names;
        }

        foreach (var property in properties.EnumerateObject())
        {
            if (IsStringType(property.Value))
                names.Add(property.Name);
        }
        return names;
    }

    /// <summary>
    /// 判断属性 Schema 的类型是否为 string（兼容 type 为字符串或字符串数组的写法）。
    /// </summary>
    private static bool IsStringType(JsonElement propertySchema)
    {
        if (propertySchema.ValueKind != JsonValueKind.Object
            || !propertySchema.TryGetProperty("type", out var type))
        {
            return false;
        }

        return type.ValueKind switch
        {
            JsonValueKind.String => IsStringKeyword(type.GetString()),
            JsonValueKind.Array => type.EnumerateArray().Any(t =>
                t.ValueKind == JsonValueKind.String && IsStringKeyword(t.GetString())),
            _ => false
        };

        static bool IsStringKeyword(string? value)
            => string.Equals(value, "string", StringComparison.Ordinal);
    }
}