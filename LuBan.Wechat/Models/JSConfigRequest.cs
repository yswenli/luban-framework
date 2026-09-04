/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Wechat.Models
*文件名： JSConfigRequest.cs
*版本号： V1.0.0.0
*唯一标识：05b0a4b7-48f9-4eb0-bfc9-f3d4438d0975
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:29
*描述：JSConfigRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：JSConfigRequest 类
*
*****************************************************************************/

namespace LuBan.Wechat.Models;
/// <summary>
/// 获取微信二维码请求数据
/// </summary>
public class JSConfigRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// OpenId
    /// </summary>
    public string OpenId { get; set; }
    /// <summary>
    /// UnionId
    /// </summary>
    public string Debug { get; set; }
}
