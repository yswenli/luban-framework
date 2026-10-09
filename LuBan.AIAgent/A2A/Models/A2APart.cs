/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Models
*文件名： A2APart
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 消息内容部件，字段存在式联合（Text/Raw/Url/Data），框架原生、SDK 无关
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A.Models;

/// <summary>
/// A2A 消息内容部件。采用<b>字段存在式联合</b>（<see cref="Text"/>/<see cref="Raw"/>/<see cref="Url"/>/<see cref="Data"/>），
/// 与 A2A v1.0.0 对齐；工厂方法用于构造。
/// </summary>
public class A2APart
{
    /// <summary>文本内容。</summary>
    public string? Text { get; set; }

    /// <summary>原始二进制（base64 编码）。</summary>
    public string? Raw { get; set; }

    /// <summary>外部资源 URL。</summary>
    public string? Url { get; set; }

    /// <summary>结构化数据（JSON 字符串）。</summary>
    public string? Data { get; set; }

    /// <summary>文件名（二进制/资源部件可选）。</summary>
    public string? Filename { get; set; }

    /// <summary>媒体类型（MIME）。</summary>
    public string? MediaType { get; set; }

    /// <summary>附加元数据。</summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>构造文本部件。</summary>
    public static A2APart FromText(string text) => new() { Text = text };

    /// <summary>构造原始二进制部件（base64）。</summary>
    public static A2APart FromRaw(string rawBase64, string? mediaType = null, string? filename = null)
        => new() { Raw = rawBase64, MediaType = mediaType, Filename = filename };

    /// <summary>构造 URL 部件。</summary>
    public static A2APart FromUrl(string url, string? mediaType = null, string? filename = null)
        => new() { Url = url, MediaType = mediaType, Filename = filename };

    /// <summary>构造结构化数据部件（JSON 字符串）。</summary>
    public static A2APart FromData(string json, string? mediaType = "application/json")
        => new() { Data = json, MediaType = mediaType };
}
