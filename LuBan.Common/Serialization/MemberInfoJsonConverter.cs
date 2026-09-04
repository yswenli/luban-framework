/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Serialization
*文件名： MemberInfoJsonConverter.cs
*版本号： V1.0.0.0
*唯一标识：e0486586-30fb-44b5-a90c-f34728d5995d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/5 13:48:53
*描述：MemberInfoJsonConverter 类
*
*=================================================
*修改标记
*修改时间：2026/8/5 13:48:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：MemberInfoJsonConverter 类
*
*****************************************************************************/

namespace LuBan.Common.Serialization;

/// <summary>
/// MemberInfo JSON 转换器。
/// </summary>
public sealed class MemberInfoJsonConverter : JsonConverter<MemberInfo>
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(MemberInfo).IsAssignableFrom(typeToConvert);
    }

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
