/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Common
*文件名： CommonResponse.cs
*版本号： V1.0.0.0
*唯一标识：5c83fec1-4518-47ca-a952-f765f4760079
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：CommonResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：CommonResponse 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Common;

/// <summary>
/// 通用响应模型
/// </summary>
public class CommonResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => Code == 0;
}