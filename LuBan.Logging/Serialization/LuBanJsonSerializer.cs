using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;

namespace LuBan.Logging.Serialization;

/// <summary>
/// LuBan 日志 JSON 序列化器，使用 System.Text.Json。
/// 输出格式与 SerializeUtil.Serialize(obj, indented:true, defalutVal:false, nullValue:true, camelCase:true) 一致。
/// </summary>
[RequiresUnreferencedCode("LuBanJsonSerializer uses JsonSerializer.Serialize(object, Type, JsonSerializerOptions) which is not trim-safe.")]
internal static class LuBanJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
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
