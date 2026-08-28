/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： DateTimeJsonConverter
*版本号： V1.0.0.0
*唯一标识：ed984913-e1ae-4d4b-8f99-d957e19d941f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/8 16:22:56
*描述：json日期格式化
*
*=================================================
*修改标记
*修改时间：2023/12/8 16:22:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：json日期格式化
*
*****************************************************************************/


using System.Globalization;

namespace LuBan.Web.Core.Models;

/// <summary>
/// json日期格式化
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// json日期格式化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        // 空值返回 default，避免 DateTime.Parse("") 抛异常
        if (string.IsNullOrWhiteSpace(str)) return default;

        // RoundtripKind 保留原始时区语义，避免跨时区解析歧义
        if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
            return result;

        // 无法解析时返回 default，交由上层校验；不抛异常避免序列化中断整个请求
        return default;
    }

    /// <summary>
    /// json日期格式化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}
