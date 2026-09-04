/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Serialization
*文件名： ExceptionJsonConverter.cs
*版本号： V1.0.0.0
*唯一标识：5989a6be-f190-4609-9aaf-4a1eb022c025
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/5 13:48:41
*描述：ExceptionJsonConverter 类
*
*=================================================
*修改标记
*修改时间：2026/8/5 13:48:41
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ExceptionJsonConverter 类
*
*****************************************************************************/

namespace LuBan.Common.Serialization;

/// <summary>
/// Exception JSON 转换器。
/// </summary>
public sealed class ExceptionJsonConverter : JsonConverter<Exception>
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Exception).IsAssignableFrom(typeToConvert);
    }

    /// <inheritdoc/>
    public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("ExceptionType", value.GetType().FullName);
        writer.WriteString("Message", value.Message);
        writer.WriteString("StackTrace", value.StackTrace);
        if (value.InnerException != null)
        {
            writer.WritePropertyName("InnerException");
            Write(writer, value.InnerException, options);
        }
        writer.WriteString("Source", value.Source);
        writer.WriteEndObject();
    }
}
