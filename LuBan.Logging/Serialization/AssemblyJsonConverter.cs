using System.Reflection;

namespace LuBan.Logging.Serialization;

/// <summary>
/// Assembly JSON 转换器，复刻原 AssemblyConverter 输出格式。
/// </summary>
internal sealed class AssemblyJsonConverter : JsonConverter<Assembly>
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
        writer.WriteString("assemblyName", value.GetName().Name);
        writer.WriteString("location", value.Location);
        writer.WriteEndObject();
    }
}
