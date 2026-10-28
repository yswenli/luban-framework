/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Sms.Models
*文件名： SmsRequestResult
*版本号： V1.0.0.0
*唯一标识：e77d54e2-85e4-4a4e-af78-82e6c89bcb3c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/4/6 11:35:14
*描述：短信请求结果
*
*=================================================
*修改标记
*修改时间：2023/4/6 11:35:14
*修改人： yswenli
*版本号： V1.0.0.0
*描述：短信请求结果
*
*****************************************************************************/
namespace LuBan.Common.Sms.Models;

/// <summary>
/// 短信请求结果,
/// https://doc.zthysms.com/web/?#/1?page_id=13
/// </summary>
[DataContract]
public class SmsRequestResult
{
    /// <summary>
    /// 无效号码对应的模板变量节点，当前只包含 <c>var1</c>。
    /// </summary>
    [DataContract]
    public class TpContent2
    {

        /// <summary>
        /// 模板变量 1 的取值（接口字段 var1）。
        /// </summary>
        [DataMember(Name = "var1")]
        public string Var1 { get; set; }
    }

    /// <summary>
    /// 无效号码列表项（接口字段 invalidList 的元素）。
    /// </summary>
    [DataContract]
    public class InvalidList2
    {

        /// <summary>
        /// 判定为无效的手机号（接口字段 mobile）。
        /// </summary>
        [DataMember(Name = "mobile")]
        public string Mobile { get; set; }

        /// <summary>
        /// 该号码对应的模板变量（接口字段 tpContent）。
        /// </summary>
        [DataMember(Name = "tpContent")]
        public TpContent2 TpContent { get; set; }
    }

    /// <summary>
    /// 状态码
    /// </summary>
    [DataMember(Name = "code")]
    public int Code { get; set; }
    /// <summary>
    /// 消息
    /// </summary>
    [DataMember(Name = "msg")]
    public string Msg { get; set; }
    /// <summary>
    /// 模板ID
    /// </summary>
    [DataMember(Name = "tpId")]
    public string TpId { get; set; }
    /// <summary>
    /// 消息ID
    /// </summary>
    [DataMember(Name = "msgId")]
    public string MsgId { get; set; }
    /// <summary>
    /// 无效变量
    /// </summary>
    [DataMember(Name = "invalidList")]
    public IList<InvalidList2> InvalidList { get; set; }
}
