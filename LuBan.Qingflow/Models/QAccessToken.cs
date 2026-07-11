/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： QAccessToken
*版本号： V1.0.0.0
*唯一标识：1c139ea0-cce7-43a4-a8f9-cc5a8df5d421
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/23 17:46:25
*描述：QAccessToken
*
*=================================================
*修改标记
*修改时间：2024/12/23 17:46:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：QAccessToken
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;

/// <summary>
/// QAccessToken
/// </summary>
[DataContract]
public class QAccessToken
{
    /// <summary>
    /// token
    /// </summary>
    [DataMember(Name = "accessToken")]
    public string AccessToken { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>

    [DataMember(Name = "expireTime")]
    public int ExpireTime { get; set; }
}
