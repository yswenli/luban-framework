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
        // 空值返回 default，避免 DateTime.Parse("") 抛异常
        if (string.IsNullOrWhiteSpace(str)) return default;
        // 优先按自身写出格式精确解析，保证序列化往返无损
        if (DateTime.TryParseExact(str, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact;
        // RoundtripKind 保留原始时区语义，避免跨时区解析歧义
        if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var roundtrip))
            return roundtrip;
        // 无法解析时返回 default，交由上层校验；不抛异常避免中断整个序列化流程
        return default;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}
