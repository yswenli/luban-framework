/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： OpenAccessToken
*版本号： V1.0.0.0
*唯一标识：93a27749-b86e-4548-bf86-7c655f64fab9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/10 14:18:43
*描述：开放访问令牌
*
*=================================================
*修改标记
*修改时间：2025/4/10 14:18:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：开放访问令牌
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;


/// <summary>
/// 开放访问令牌
/// </summary>
public class OpenAccessToken
{
    /// <summary>
    /// 获取或设置刷新令牌。
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// 获取或设置访问令牌。
    /// </summary>
    public string AccessToken { get; set; }
}

/// <summary>
/// 开放访问令牌请求参数
/// </summary>
public class OpenAccessTokenInput
{
    /// <summary>
    /// key
    /// </summary>
    [Required]
    public string AccessKey { get; set; }
    /// <summary>
    /// secret
    /// </summary>
    [Required]
    public string AccessSecret { get; set; }
}