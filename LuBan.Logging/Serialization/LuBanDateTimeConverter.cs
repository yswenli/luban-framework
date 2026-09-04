/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.Serialization
*文件名： LuBanDateTimeConverter.cs
*版本号： V1.0.0.0
*唯一标识：6a17a84a-cdcf-4ea9-96fb-433bc3e997c8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:35:09
*描述：LuBanDateTimeConverter 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:35:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBanDateTimeConverter 类
*
*****************************************************************************/

namespace LuBan.Logging.Serialization;

/// <summary>
/// 日期时间 JSON 转换器，输出格式为 yyyy-MM-dd HH:mm:ss.fff。
/// </summary>
internal sealed class LuBanDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _format;

    /// <summary>
    /// 初始化日期时间转换器。
    /// </summary>
    /// <param name="format">日期格式字符串。</param>
    public LuBanDateTimeConverter(string format = "yyyy-MM-dd HH:mm:ss.fff")
    {
        _format = format;
    }

    /// <inheritdoc/>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString()!, _format, null);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}
