using System.Reflection;

namespace LuBan.Logging.Serialization;

/// <summary>
/// MemberInfo JSON 转换器，复刻原 MemberInfoConverter 输出格式。
/// </summary>
internal sealed class MemberInfoJsonConverter : JsonConverter<MemberInfo>
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
        writer.WriteString("memberName", value.Name);
        writer.WriteString("memberType", value.MemberType.ToString());
        writer.WriteString("declaringTypeName", value.DeclaringType?.FullName);
        writer.WriteEndObject();
    }
}
