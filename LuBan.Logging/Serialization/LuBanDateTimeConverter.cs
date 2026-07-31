namespace LuBan.Logging.Serialization;

/// <summary>
/// 日期时间 JSON 转换器，输出格式为 yyyy-MM-dd HH:mm:ss.fff。
/// </summary>
internal sealed class LuBanDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _format;

    /// <summary>
    /// 初始化日期时间转换器。
    /// </summary>
    /// <param name="format">日期格式字符串。</param>
    public LuBanDateTimeConverter(string format = "yyyy-MM-dd HH:mm:ss.fff")
    {
        _format = format;
    }

    /// <inheritdoc/>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString()!, _format, null);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}
