/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools
*文件名： ToolArgumentValue
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：工具参数值读取工具，集中处理 Microsoft.Extensions.AI 下发的参数值形态
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools;

/// <summary>
/// 工具参数值读取工具：集中处理 Microsoft.Extensions.AI 下发的参数值形态。
/// </summary>
/// <remarks>
/// 参数值来自模型返回的 JSON 反序列化，运行时类型最常见的是 <see cref="JsonElement"/>，
/// 而非 <see cref="string"/>。守卫（<see cref="RequiredArgumentGuardAIFunction"/>）与路径参数绑定
/// （<see cref="AIFunctionFactoryHelper"/>）必须共享同一套取值判断，否则会出现
/// "守卫认为已提供、绑定却取不到值"的不一致：曾导致文件工具的 path 被绑定成 null，
/// 进而在 <c>new FileInfo(null)</c> 抛 <c>ArgumentNullException(Parameter 'fileName')</c>。
/// </remarks>
internal static class ToolArgumentValue
{
    /// <summary>
    /// 大小写不敏感地读取参数原始值（AIFunctionArguments 的键通常已与参数名一致，此处仅作兜底）。
    /// </summary>
    /// <param name="arguments">工具参数</param>
    /// <param name="name">参数名</param>
    /// <param name="value">读取到的原始值</param>
    /// <returns>是否存在该参数</returns>
    public static bool TryGet(AIFunctionArguments arguments, string name, out object? value)
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

    /// <summary>
    /// 把参数原始值解释为字符串。
    /// </summary>
    /// <param name="value">参数原始值（可能为 <see cref="string"/>、<see cref="JsonElement"/> 或其他基础类型）</param>
    /// <param name="text">解释后的字符串</param>
    /// <returns>
    /// true 表示值形态可解释为字符串（string、JsonElement 的字符串/数值/布尔标量、其他 IFormattable 标量）；
    /// false 表示不是可用的字符串值（null、JsonElement 的 Null/Undefined/对象/数组，或未知复杂类型）。
    /// </returns>
    public static bool TryGetString(object? value, out string? text)
    {
        switch (value)
        {
            case null:
                text = null;
                return false;

            case string s:
                text = s;
                return true;

            case JsonElement je:
                switch (je.ValueKind)
                {
                    case JsonValueKind.String:
                        text = je.GetString();
                        return true;
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        text = je.GetRawText();
                        return true;
                    default:
                        text = null;
                        return false;
                }

            case IFormattable formattable:
                text = formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
                return true;

            default:
                text = null;
                return false;
        }
    }
}