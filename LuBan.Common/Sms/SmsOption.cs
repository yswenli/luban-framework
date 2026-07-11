/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Sms
*文件名： SmsOption
*版本号： V1.0.0.0
*唯一标识：a4953277-c154-4135-91f4-190abc5521f6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/28 10:57:49
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/28 10:57:49
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.Sms;

/// <summary>
/// 短信配置选项
/// </summary>
public sealed class SmsOption
{
    /// <summary>
    /// 助通
    /// </summary>
    public ZhuTongSmsSetting ZhuTong { get; set; }
}


/// <summary>
/// 助通科技
/// </summary>
public sealed class ZhuTongSmsSetting
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = "LuBan";
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = "";
    /// <summary>
    /// 短信模板id
    /// </summary>
    public long TemplateId { get; set; }

    /// <summary>
    /// 签名key
    /// </summary>
    public string Signature { get; set; }
}
