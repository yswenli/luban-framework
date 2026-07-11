/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Wechat.CorpProvider
*文件名： AppClientAndAccessToken
*版本号： V1.0.0.0
*唯一标识：b5fd9e11-6641-467e-ad9e-b60dda303f08
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/15 13:15:07
*描述：授权应用和token
*
*=================================================
*修改标记
*修改时间：2024/11/15 13:15:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：授权应用和token
*
*****************************************************************************/
namespace LuBan.Wechat.CorpProvider;

/// <summary>
/// 授权应用和token
/// </summary>
public class AppClientAndAccessToken
{
    /// <summary>
    /// 授权应用
    /// </summary>
    public WechatCorpAppClient AppClient { get; set; }
    /// <summary>
    /// token
    /// </summary>
    public string AccessToken { get; set; }
}
