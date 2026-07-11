/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Sms.Models
*文件名： TemplateMsgInfo
*版本号： V1.0.0.0
*唯一标识：35b5af17-ab8b-44f1-98c9-d71f92529a39
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/4/6 13:50:39
*描述：模板变量内容
*
*=================================================
*修改标记
*修改时间：2023/4/6 13:50:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：模板变量内容
*
*****************************************************************************/
namespace LuBan.Common.Sms.Models;

/// <summary>
/// 模板变量内容
/// </summary>
[DataContract]
public class TemplateMsgInfo
{
    /// <summary>
    /// 手机号
    /// </summary>

    [DataMember(Name = "mobile")]
    public string Mobile { get; set; }

    /// <summary>
    /// 模板变量内容
    /// </summary>

    [DataMember(Name = "tpContent")]
    public Dictionary<string, string> TpContent { get; set; }
}
