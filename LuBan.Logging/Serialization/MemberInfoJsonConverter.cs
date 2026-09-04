/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.Serialization
*文件名： MemberInfoJsonConverter.cs
*版本号： V1.0.0.0
*唯一标识：541be1d3-7e23-4692-a246-6790502094d7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:35:27
*描述：MemberInfoJsonConverter 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:35:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：MemberInfoJsonConverter 类
*
*****************************************************************************/

using System.Reflection;

namespace LuBan.Logging.Serialization;

/// <summary>
/// MemberInfo JSON 转换器，复刻原 MemberInfoConverter 输出格式。
/// </summary>
internal sealed class MemberInfoJsonConverter : JsonConverter<MemberInfo>
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
        // 与原 MemberInfoConverter 保持一致：手动写入的属性名不受 camelCase 策略影响
        writer.WriteStartObject();
        writer.WriteString("MemberName", value.Name);
        writer.WriteString("MemberType", value.MemberType.ToString());
        writer.WriteString("DeclaringTypeName", value.DeclaringType?.FullName);
        writer.WriteEndObject();
    }
}
