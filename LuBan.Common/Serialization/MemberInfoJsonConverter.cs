namespace LuBan.Common.Serialization;

/// <summary>
/// MemberInfo JSON 转换器。
/// </summary>
public sealed class MemberInfoJsonConverter : JsonConverter<MemberInfo>
{
    /// <inheritdoc/>
    public override MemberInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, MemberInfo value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("MemberName", value.Name);
        writer.WriteString("MemberType", value.MemberType.ToString());
        writer.WriteString("DeclaringTypeName", value.DeclaringType?.FullName);
        writer.WriteEndObject();
    }
}
