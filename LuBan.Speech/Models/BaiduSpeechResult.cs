/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/26 14:57:46
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Speech.Models;

[DataContract]
public class BaiduSpeechResult
{

    [DataMember(Name = "corpus_no")]
    public string CorpusNo { get; set; }

    [DataMember(Name = "err_msg")]
    public string ErrMsg { get; set; }

    [DataMember(Name = "err_no")]
    public int ErrNo { get; set; }

    [DataMember(Name = "result")]
    public IList<string> Result { get; set; }

    [DataMember(Name = "sn")]
    public string Sn { get; set; }
}
