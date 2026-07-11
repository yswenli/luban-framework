/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.JWT
*文件名： JWTAuthConfig
*版本号： V1.0.0.0
*唯一标识：f8a46c52-8482-410a-866e-5bbf2abb1b56
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 13:39:03
*描述：jwt配置
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 13:39:03
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：jwt配置
*
*****************************************************************************/

namespace LuBan.Web.Core.Jwt;

/// <summary>
/// jwt配置
/// </summary>
public class JwtAuthConfig
{
    /// <summary>
    /// jwt密码
    /// </summary>
    public string Secret { get; set; }
    /// <summary>
    /// 发行人
    /// </summary>
    public string Issuer { get; set; }
    /// <summary>
    /// 用户
    /// </summary>
    public string Audience { get; set; }
    /// <summary>
    /// 访问超时
    /// </summary>
    public int AccessExpiration { get; set; }
}
