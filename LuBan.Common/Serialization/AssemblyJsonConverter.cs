/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Serialization
*文件名： AssemblyJsonConverter.cs
*版本号： V1.0.0.0
*唯一标识：119ed4de-bb9b-4df4-ada2-380924d6da9f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/5 13:48:47
*描述：AssemblyJsonConverter 类
*
*=================================================
*修改标记
*修改时间：2026/8/5 13:48:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AssemblyJsonConverter 类
*
*****************************************************************************/

namespace LuBan.Common.Serialization;

/// <summary>
/// Assembly JSON 转换器。
/// </summary>
public sealed class AssemblyJsonConverter : JsonConverter<Assembly>
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
        writer.WriteStartObject();
        writer.WriteString("AssemblyName", value.FullName);
        writer.WriteString("Location", value.Location);
        writer.WriteEndObject();
    }
}
