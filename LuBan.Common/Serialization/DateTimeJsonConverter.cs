namespace LuBan.Common.Serialization;

/// <summary>
/// 日期时间 JSON 转换器。
/// </summary>
public sealed class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly string _format;

    /// <summary>
    /// 初始化日期时间转换器。
    /// </summary>
    /// <param name="format">日期格式字符串。</param>
    public DateTimeJsonConverter(string format = "yyyy-MM-dd HH:mm:ss.fff")
    {
        _format = format;
    }

    /// <inheritdoc/>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str)) return default;
        if (DateTime.TryParseExact(str, _format, null, System.Globalization.DateTimeStyles.None, out var exact))
            return exact;
        if (DateTime.TryParse(str, out var fallback))
            return fallback;
        return default;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}
