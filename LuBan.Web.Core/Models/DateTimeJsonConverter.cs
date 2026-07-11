/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
        return DateTime.Parse(reader.GetString() ?? "");
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
