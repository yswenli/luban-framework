namespace LuBan.Common.Serialization;

/// <summary>
/// Assembly JSON 转换器。
/// </summary>
public sealed class AssemblyJsonConverter : JsonConverter<Assembly>
{
    /// <inheritdoc/>
    public override Assembly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Assembly value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("AssemblyName", value.FullName);
        writer.WriteString("Location", value.Location);
        writer.WriteEndObject();
    }
}
