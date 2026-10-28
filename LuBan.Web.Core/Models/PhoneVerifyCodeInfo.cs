/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： PhoneVerifyCodeInfo
*版本号： V1.0.0.0
*唯一标识：d7c2f1a3-e5b6-4c9d-8f2a-4e6d8b0f3c7e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/09/08 10:00:00
*描述：手机验证码缓存模型
*
*=================================================
*修改标记
*修改时间：2026/09/08 10:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：创建
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;

/// <summary>
/// 手机验证码的缓存信息，记录验证码、生成时间、使用状态与有效期。
/// </summary>
public class PhoneVerifyCodeInfo
{
    /// <summary>
    /// 下发给用户的验证码内容。
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// 验证码的生成时间。
    /// </summary>
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// 验证码是否已被使用，已使用的不能再次校验通过。
    /// </summary>
    public bool IsUsed { get; set; }
    /// <summary>
    /// 验证码的有效期，单位为分钟。
    /// </summary>
    public int ExpireMinutes { get; set; }
}