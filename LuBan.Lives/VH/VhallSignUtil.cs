/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.VH
*文件名： VhallSignUtil
*版本号： V1.0.0.0
*唯一标识：56ec840a-315f-408b-b07d-903b36a5d8ed
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/29 10:54:57
*描述：参数签名校验算法工具类
*
*=================================================
*修改标记
*修改时间：2025/4/29 10:54:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：参数签名校验算法工具类
*
*****************************************************************************/
namespace LuBan.Lives.VH;

/// <summary>
/// 参数签名校验算法工具类,
/// https://saas-doc.vhall.com/opendocs/show/1423
/// </summary>
public static class VhallSignUtil
{
    /// <summary>
    /// 获取签名
    /// </summary>
    /// <param name="secretKey"></param>
    /// <param name="sortedDic"></param>
    /// <returns></returns>
    public static string GetSign(string secretKey, SortedDictionary<string, object> sortedDic)
    {
        StringPlus sp = new();
        foreach (var item in sortedDic)
        {
            if (item.Value is ICollection)
            {
                sp.Append($"{item.Key}");
            }
            else
            {
                sp.Append(item.Key);
                sp.Append(item.Value?.ToString() ?? "");
            }
        }
        sp.Insert(0, secretKey);
        sp.Append(secretKey);
        return sp.ToString().GetMD5Str();
    }
}
