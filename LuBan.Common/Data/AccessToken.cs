/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Data
*文件名： AccessToken
*版本号： V1.0.0.0
*唯一标识：0c8440df-c972-4454-9b90-ab7cfdec8ff1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/23 17:06:19
*描述：
*
*=================================================
*修改标记
*修改时间：2024/12/23 17:06:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.Data;

/// <summary>
/// 微信接口调用凭证
/// </summary>
public class AccessToken
{
    /// <summary>
    /// 接口调用凭证
    /// </summary>
    public string Token { get; set; }
    /// <summary>
    /// 凭证有效时间
    /// </summary>
    public DateTime ExpireTime { get; set; }
}
