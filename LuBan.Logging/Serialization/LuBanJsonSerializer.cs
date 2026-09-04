/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.Serialization
*文件名： LuBanJsonSerializer.cs
*版本号： V1.0.0.0
*唯一标识：d72ba61e-1cb2-4d54-ac3f-d7d3a1ac5a60
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:35:33
*描述：LuBanJsonSerializer 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:35:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBanJsonSerializer 类
*
*****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;

namespace LuBan.Logging.Serialization;

/// <summary>
/// LuBan 日志 JSON 序列化器，使用 System.Text.Json。
/// 输出格式与 SerializeUtil.Serialize(obj, indented:false, defalutVal:false, nullValue:true, camelCase:true) 一致。
/// </summary>
[RequiresUnreferencedCode("LuBanJsonSerializer uses JsonSerializer.Serialize(object, Type, JsonSerializerOptions) which is not trim-safe.")]
internal static class LuBanJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new LuBanDateTimeConverter("yyyy-MM-dd HH:mm:ss.fff"),
            new ExceptionJsonConverter(),
            new AssemblyJsonConverter(),
            new MemberInfoJsonConverter()
        }
    };

    /// <summary>
    /// 序列化对象为 JSON 字符串。
    /// </summary>
    /// <param name="obj">要序列化的对象。</param>
    /// <returns>JSON 字符串。</returns>
    public static string Serialize(object obj)
    {
        if (obj == null) return string.Empty;
        return JsonSerializer.Serialize(obj, obj.GetType(), _options);
    }
}
