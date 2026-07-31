namespace LuBan.Logging.Serialization;

/// <summary>
/// Exception JSON 转换器，复刻原 NewtonsoftExceptionConverter 输出格式。
/// </summary>
internal sealed class ExceptionJsonConverter : JsonConverter<Exception>
{
    /// <inheritdoc/>
    public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("exceptionType", value.GetType().FullName);
        writer.WriteString("message", value.Message);
        writer.WriteString("stackTrace", value.StackTrace);
        if (value.InnerException != null)
        {
            writer.WritePropertyName("innerException");
            Write(writer, value.InnerException, options);
        }
        writer.WriteString("source", value.Source);
        writer.WriteEndObject();
    }
}
