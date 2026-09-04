/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.File
*文件名： FileInfo.cs
*版本号： V1.0.0.0
*唯一标识：2b21764e-e863-4b18-9d2b-7e2e769e9697
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：FileInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：FileInfo 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.File;

/// <summary>
/// 文件信息模型
/// </summary>
public class FileInfo
{
    /// <summary>
    /// 文件 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 文件名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间戳
    /// </summary>
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    /// <summary>
    /// 更新时间戳
    /// </summary>
    [JsonPropertyName("update_time")]
    public long UpdateTime { get; set; }
}