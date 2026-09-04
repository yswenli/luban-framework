/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.Serialization
*文件名： ExceptionJsonConverter.cs
*版本号： V1.0.0.0
*唯一标识：ae0a3e90-6509-4c68-b378-348ccdc15063
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:35:16
*描述：ExceptionJsonConverter 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:35:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ExceptionJsonConverter 类
*
*****************************************************************************/

namespace LuBan.Logging.Serialization;

/// <summary>
/// Exception JSON 转换器，复刻原 NewtonsoftExceptionConverter 输出格式。
/// </summary>
internal sealed class ExceptionJsonConverter : JsonConverter<Exception>
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
        // 与原 NewtonsoftExceptionConverter 保持一致：手动写入的属性名不受 camelCase 策略影响
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
