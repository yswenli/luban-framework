/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.File
*文件名： ParseFileRequest.cs
*版本号： V1.0.0.0
*唯一标识：c17e76ac-ae89-45c1-8e15-afea5936ae52
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ParseFileRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ParseFileRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.File;

/// <summary>
/// 文件解析请求模型
/// </summary>
public class ParseFileRequest
{
    /// <summary>
    /// 文件 ID 列表
    /// </summary>
    [JsonPropertyName("file_ids")]
    public List<string> FileIds { get; set; } = new();
}