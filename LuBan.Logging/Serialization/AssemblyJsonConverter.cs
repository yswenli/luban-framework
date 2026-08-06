using System.Reflection;

namespace LuBan.Logging.Serialization;

/// <summary>
/// Assembly JSON 转换器，复刻原 AssemblyConverter 输出格式。
/// </summary>
internal sealed class AssemblyJsonConverter : JsonConverter<Assembly>
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Assembly).IsAssignableFrom(typeToConvert);
    }

    /// <inheritdoc/>
    public override Assembly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Assembly value, JsonSerializerOptions options)
    {
        // 与原 AssemblyConverter 保持一致：手动写入的属性名不受 camelCase 策略影响
        writer.WriteStartObject();
        writer.WriteString("AssemblyName", value.FullName);
        writer.WriteString("Location", value.Location);
        writer.WriteEndObject();
    }
}
