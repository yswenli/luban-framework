/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Wechat.Models
*文件名： WechatCorpAppOptions
*版本号： V1.0.0.0
*唯一标识：7225a856-2bc3-4e13-a834-8521f86d3e5e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/13 10:06:05
*描述：企业微信Crop代开发应用配置
*
*=================================================
*修改标记
*修改时间：2024/11/13 10:06:05
*修改人： yswenli
*版本号： V1.0.0.0
*描述：企业微信Crop代开发应用配置
*
*****************************************************************************/
namespace LuBan.Wechat.Models;

/// <summary>
/// 企业微信Crop代开发应用配置
/// </summary>
public class WechatCorpAppOptions
{
    /// <summary>
    /// 授权企业Id
    /// </summary>
    public string CorpId { get; set; }
    /// <summary>
    /// 授权永久码
    /// </summary>
    public string PermanentCode { get; set; }
    /// <summary>
    /// 应用id
    /// </summary>
    public int AgentId { get; set; }

}
