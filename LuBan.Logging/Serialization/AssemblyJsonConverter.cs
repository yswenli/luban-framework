/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.Serialization
*文件名： AssemblyJsonConverter.cs
*版本号： V1.0.0.0
*唯一标识：c7b470ab-6225-42c8-a8f1-d31eff745388
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:35:22
*描述：AssemblyJsonConverter 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:35:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AssemblyJsonConverter 类
*
*****************************************************************************/

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
