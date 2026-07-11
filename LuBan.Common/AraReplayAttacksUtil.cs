/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： SignatureHelper
*版本号： V1.0.0.0
*唯一标识：d4860cfe-80b8-4cad-adcc-2c730b7e35ad
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/5/29 15:59:13
*描述：防重放攻击类
*
*=================================================
*修改标记
*修改时间：2023/5/29 15:59:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：防重放攻击类
*
*****************************************************************************/
using Result = LuBan.Common.Data.Result;


namespace LuBan.Common;

/// <summary>
/// 防重放攻击类
/// </summary>
public static class AraReplayAttacksUtil
{
    /// <summary>
    /// 获取安全较验信息
    /// </summary>
    /// <param name="json"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static AraInfo? GetSafeComparisonInfoForJson(string json, out Result result)
    {
        if (json.IsNullOrEmpty())
        {
            result = new Fail("The json cannot be empty.");
            return null;
        }

        AraData sd = [];
        sd.TryAdd("md5", json.GetMD5Str());

        return GetSafeComparisonInfo(sd, out result);
    }

    /// <summary>
    /// 获取安全较验信息
    /// </summary>
    /// <param name="data">根据字段名称排序</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static AraInfo? GetSafeComparisonInfo(AraData data, out Result result)
    {
        if (data == null)
        {
            data = new AraData();
        }

        var sp = new StringPlus();

        var timestamp = "";

        if (!data.Contain("timestamp"))
        {
            timestamp = DateTimeUtil.Now.ToUnixTimeStamp(false).ToString();
            data.TryAdd("timestamp", timestamp.ToString());
        }
        var nonce = "";
        if (!data.Contain("nonce"))
        {
            nonce = RandomUtil.GetString(8, 1);
            data.TryAdd("nonce", nonce);
        }

        foreach (var keyValue in data)
        {
            if (keyValue.Key.Equals("signature", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            sp.Append($"{keyValue.Key.ToLower()}={keyValue.Value}&");

            if (keyValue.Key.Equals("timestamp", StringComparison.InvariantCultureIgnoreCase))
            {
                timestamp = keyValue.Value;
            }
            if (keyValue.Key.Equals("nonce", StringComparison.InvariantCultureIgnoreCase))
            {
                nonce = keyValue.Value;
            }
        }

        var ps = sp.ToString(0, sp.Length - 1);


        var signature = ps.GetMD5Str();

        result = new Success();

        return new AraInfo()
        {
            TimeStamp = timestamp,
            Nonce = nonce,
            Signature = signature
        };
    }

    /// <summary>
    /// 获取安全较验信息
    /// </summary>
    /// <param name="queryStr"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static AraInfo? GetSafeComparisonInfoForQuery(string queryStr, out Result result)
    {
        var sd = queryStr.ToQueryDic();
        return GetSafeComparisonInfo(sd, out result);
    }


    /// <summary>
    /// 获取安全较验信息
    /// </summary>
    /// <param name="model"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static AraInfo? GetSafeComparisonInfoForObject(object model, out Result result)
    {
        if (model == null)
        {
            result = new Fail("The required parameter cannot be empty.");
            return null;
        }

        var type = model.GetType();

        var properties = type.GetProperties().OrderBy(q => q.Name);

        if (properties == null || !properties.Any())
        {
            result = new Fail("The required parameter cannot be empty.");
            return null;
        }

        AraData sd = new AraData();

        foreach (var property in properties)
        {
            var val = property.GetValue(model);
            sd.TryAdd(property.Name.ToLower(), val?.ToString() ?? "");
        }

        return GetSafeComparisonInfo(sd, out result);
    }


    /// <summary>
    /// 校验
    /// </summary>
    /// <param name="data"></param>
    /// <param name="safeComparisonExpired"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool Valide(AraData data, int safeComparisonExpired, out Result result)
    {
        if (data == null || data.Count < 1)
        {
            throw FriendlyError.Ex("The required parameter cannot be empty.", EnumErrorCode.D0001, 200);
        }

        if (safeComparisonExpired <= 0) safeComparisonExpired = 5;

        var sp = new StringPlus();

        string signature = string.Empty;
        long timeStamp = -1;
        string nonce = string.Empty;

        foreach (var item in data)
        {
            var value = item.Value;
            if (item.Key.Equals("Signature", StringComparison.InvariantCultureIgnoreCase))
            {
                signature = value?.ToString() ?? "";
            }
            else
            {
                sp.Append($"{item.Key.ToLower()}={value}&");

                if (item.Key.Equals("TimeStamp", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (value.IsNotNullOrEmpty() && (value.Trim().Length == 10 || value.Trim().Length == 13))
                    {
                        timeStamp = long.Parse(value.ToString());
                        if (value.Trim().Length == 10)
                        {
                            if (DateTimeUtil.ToUtcTime(timeStamp, false).AddMinutes(safeComparisonExpired) < DateTimeUtil.UtcNow)
                            {
                                throw FriendlyError.Ex("The timestamp of the current operation has expired.", EnumErrorCode.P0002, 410);
                            }
                        }
                        else
                        {
                            if (DateTimeUtil.ToUtcTime(timeStamp, true).AddMinutes(safeComparisonExpired) < DateTimeUtil.UtcNow)
                            {
                                throw FriendlyError.Ex("The timestamp of the current operation has expired.", EnumErrorCode.P0002, 410);
                            }
                        }
                    }
                    else
                    {
                        throw FriendlyError.Ex("The timestamp of the current operation cannot be empty or .", EnumErrorCode.D0001, 200);
                    }
                }

                if (item.Key.Equals("Nonce", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (value != null)
                    {
                        nonce = value.ToString();
                        var old = MemoryCache.Instance.Get<string>(nonce);
                        if (!string.IsNullOrEmpty(old) &&
                            old.Equals(value))
                        {
                            throw FriendlyError.Ex("The current operation cannot be repeated.", EnumErrorCode.P0001, 400);
                        }
                        else
                        {
                            MemoryCache.Instance.Set(nonce, nonce, TimeSpan.FromMinutes(safeComparisonExpired));
                        }
                    }
                    else
                    {
                        throw FriendlyError.Ex("The nonce of the current operation cannot be empty.", EnumErrorCode.D0001, 200);
                    }
                }
            }
        }

        if (sp.Length < 1)
        {
            throw FriendlyError.Ex("The required parameter cannot be empty.", EnumErrorCode.D0001, 200);
        }

        if (timeStamp < 0)
        {
            throw FriendlyError.Ex("The timestamp of the current operation cannot be empty.", EnumErrorCode.D0001, 200);
        }

        if (nonce.IsNullOrEmpty())
        {
            throw FriendlyError.Ex("The nonce of the current operation cannot be empty.", EnumErrorCode.D0001, 200);
        }

        var ps = sp.ToString(0, sp.Length - 1);

        if (ps.IsNullOrEmpty())
        {
            throw FriendlyError.Ex("The current operation signature cannot be empty.", EnumErrorCode.D0001, 200);
        }

        var sign = ps.GetMD5Str();

        if (!sign.Equals(signature, StringComparison.InvariantCultureIgnoreCase))
        {
            throw FriendlyError.Ex("The current operation signature error.", EnumErrorCode.P0003, 400);
        }

        result = new Success();
        return true;
    }
}
