/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Serialization
*文件名： DateTimeJsonConverter.cs
*版本号： V1.0.0.0
*唯一标识：3d713fa0-711a-46fb-8f77-f549aeb1eadd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/5 13:48:34
*描述：DateTimeJsonConverter 类
*
*=================================================
*修改标记
*修改时间：2026/8/5 13:48:34
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DateTimeJsonConverter 类
*
*****************************************************************************/

namespace LuBan.Common.Serialization;

/// <summary>
/// 日期时间 JSON 转换器。
/// </summary>
public sealed class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly string _format;

    /// <summary>
    /// 初始化日期时间转换器。
    /// </summary>
    /// <param name="format">日期格式字符串。</param>
    public DateTimeJsonConverter(string format = "yyyy-MM-dd HH:mm:ss.fff")
    {
        _format = format;
    }

    /// <inheritdoc/>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str)) return default;
        if (DateTime.TryParseExact(str, _format, null, System.Globalization.DateTimeStyles.None, out var exact))
            return exact;
        if (DateTime.TryParse(str, out var fallback))
            return fallback;
        return default;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}
