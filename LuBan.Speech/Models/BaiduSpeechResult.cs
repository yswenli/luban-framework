/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Speech.Models
*文件名： BaiduSpeechResult
*版本号： V1.0.0.0
*唯一标识：49b9869e-07af-43e5-a547-1a8c9ada0cd3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/26 14:57:46
*描述：BaiduSpeechResult 结果类
*
*=================================================
*修改标记
*修改时间：2023/12/26 14:57:46
*修改人： yswenli
*版本号： V1.0.0.0
*描述：BaiduSpeechResult 结果类
*
*****************************************************************************/
namespace LuBan.Speech.Models;

[DataContract]
/// <summary>
/// BaiduSpeechResult 结果类
/// </summary>
public class BaiduSpeechResult
{

    /// <summary>
    /// 语音识别使用的语料库编号（接口字段 corpus_no）。
    /// </summary>
    [DataMember(Name = "corpus_no")]
    public string CorpusNo { get; set; }

    /// <summary>
    /// 错误描述信息（接口字段 err_msg）。
    /// </summary>
    [DataMember(Name = "err_msg")]
    public string ErrMsg { get; set; }

    /// <summary>
    /// 错误码，0 表示识别成功（接口字段 err_no）。
    /// </summary>
    [DataMember(Name = "err_no")]
    public int ErrNo { get; set; }

    /// <summary>
    /// 识别结果文本数组，通常第一个元素为最佳结果（接口字段 result）。
    /// </summary>
    [DataMember(Name = "result")]
    public IList<string> Result { get; set; }

    /// <summary>
    /// 本次请求的流水号（接口字段 sn）。
    /// </summary>
    [DataMember(Name = "sn")]
    public string Sn { get; set; }
}
