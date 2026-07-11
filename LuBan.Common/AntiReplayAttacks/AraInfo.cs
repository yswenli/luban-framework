/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.AntiReplayAttacks
*文件名： AraInfo
*版本号： V1.0.0.0
*唯一标识：213d6394-207f-49a2-907d-0da11a6e59b2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/14 17:40:41
*描述：安全较验类
*
*=================================================
*修改标记
*修改时间：2025/4/14 17:40:41
*修改人： yswenli
*版本号： V1.0.0.0
*描述：安全较验类
*
*****************************************************************************/
namespace LuBan.Common.AntiReplayAttacks;

using Result = Data.Result;

/// <summary>
/// 安全较验类
/// </summary>
public class AraInfo
{
    /// <summary>
    /// unix时间戳，验证请求传输有效时长
    /// </summary>
    public string TimeStamp { get; set; }
    /// <summary>
    /// 客户生成的唯一随机码，验证重复提交
    /// </summary>
    public string Nonce { get; set; }
    /// <summary>
    /// 签名，验数据有效性
    /// </summary>
    public string Signature { get; set; }

    /// <summary>
    /// 安全较验类
    /// </summary>
    public AraInfo()
    {
    }

    /// <summary>
    /// 安全较验类
    /// </summary>
    /// <param name="model"></param>
    public AraInfo(object model)
    {
        var data = AraReplayAttacksUtil.GetSafeComparisonInfoForObject(model, out Result _);
        if (data != null)
        {
            TimeStamp = data.TimeStamp;
            Nonce = data.Nonce;
            Signature = data.Signature;
        }
    }

    /// <summary>
    /// 安全较验类
    /// </summary>
    /// <param name="json"></param>
    public AraInfo(string json)
    {
        if (json.IsNullOrEmpty())
        {
            var data = AraReplayAttacksUtil.GetSafeComparisonInfo([], out Result _);
            if (data != null)
            {
                TimeStamp = data.TimeStamp;
                Nonce = data.Nonce;
                Signature = data.Signature;
            }
        }
        else
        {
            var data = AraReplayAttacksUtil.GetSafeComparisonInfoForJson(json, out Result _);
            if (data != null)
            {
                TimeStamp = data.TimeStamp;
                Nonce = data.Nonce;
                Signature = data.Signature;
            }
        }
    }

    /// <summary>
    /// 安全较验类
    /// </summary>
    /// <param name="parameters"></param>
    public AraInfo(SortedDictionary<string, string> parameters)
    {
        var data = AraReplayAttacksUtil.GetSafeComparisonInfo(parameters, out Result _);
        if (data != null)
        {
            TimeStamp = data.TimeStamp;
            Nonce = data.Nonce;
            Signature = data.Signature;
        }
    }

    /// <summary>
    /// 转换为参数字典
    /// </summary>
    /// <returns></returns>
    public SortedDictionary<string, string> ToParameters()
    {
        return new SortedDictionary<string, string>
        {
            { "TimeStamp", TimeStamp.ToString() },
            { "Nonce", Nonce },
            { "Signature", Signature }
        };
    }

}
