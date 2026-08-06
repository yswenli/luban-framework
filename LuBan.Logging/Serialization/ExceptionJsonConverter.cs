namespace LuBan.Logging.Serialization;

/// <summary>
/// Exception JSON 转换器，复刻原 NewtonsoftExceptionConverter 输出格式。
/// </summary>
internal sealed class ExceptionJsonConverter : JsonConverter<Exception>
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Exception).IsAssignableFrom(typeToConvert);
    }

    /// <inheritdoc/>
    public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
    {
        // 与原 NewtonsoftExceptionConverter 保持一致：手动写入的属性名不受 camelCase 策略影响
        writer.WriteStartObject();
        writer.WriteString("ExceptionType", value.GetType().FullName);
        writer.WriteString("Message", value.Message);
        writer.WriteString("StackTrace", value.StackTrace);
        if (value.InnerException != null)
        {
            writer.WritePropertyName("InnerException");
            Write(writer, value.InnerException, options);
        }
        writer.WriteString("Source", value.Source);
        writer.WriteEndObject();
    }
}
