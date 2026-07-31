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
        // 与原 MemberInfoConverter 保持一致：手动写入的属性名不受 camelCase 策略影响
        writer.WriteStartObject();
        writer.WriteString("MemberName", value.Name);
        writer.WriteString("MemberType", value.MemberType.ToString());
        writer.WriteString("DeclaringTypeName", value.DeclaringType?.FullName);
        writer.WriteEndObject();
    }
}
