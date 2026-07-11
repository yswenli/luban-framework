/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumPlatformType
*版本号： V1.0.0.0
*唯一标识：97adc943-8959-42ab-b983-3ce83be55e15
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:16:23
*描述：平台类型枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:16:23
*修改人： yswenli
*版本号： V1.0.0.0
*描述：平台类型枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 平台类型枚举
/// </summary>
[Description("平台类型枚举")]
public enum EnumPlatformType
{
    /// <summary>
    /// 微信公众号
    /// </summary>
    [Description("微信公众号")]
    微信公众号 = 10,

    /// <summary>
    /// 微信小程序
    /// </summary>
    [Description("微信小程序")]
    微信小程序 = 11,

    /// <summary>
    /// 微信小程序
    /// </summary>
    [Description("企业微信")]
    企业微信 = 12,

    /// <summary>
    /// QQ
    /// </summary>
    [Description("QQ")]
    QQ = 20,

    /// <summary>
    /// 支付宝
    /// </summary>
    [Description("支付宝")]
    Alipay = 30,

    /// <summary>
    /// Gitee
    /// </summary>
    [Description("Gitee")]
    Gitee = 40,
}
