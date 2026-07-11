/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： StringUtil
*版本号： V1.0.0.0
*唯一标识：08754b56-6e5a-4e23-aa54-edb2f2069742
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 10:53:58
*描述：字符串工具类
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 10:53:58
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：字符串工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 字符串工具类
/// </summary>
public static class StringUtil
{
    static int k = 1024;

    static int m = k * k;

    static long g = m * k;

    static long t = g * k;

    /// <summary>
    /// 转换单位
    /// </summary>
    /// <param name="len"></param>
    /// <returns></returns>
    public static string Convert(long len)
    {
        string result = string.Empty;

        if (len < k)
        {
            result = string.Format("{0:F} B", len);
        }
        else if (len < m)
        {
            result = string.Format("{0} KB", ((len / 1.00 / k)).ToString("f2"));
        }
        else if (len < g)
        {
            result = string.Format("{0} MB", ((len / 1.00 / m)).ToString("f2"));
        }
        else
        {
            result = string.Format("{0} GB", ((len / 1.00 / g)).ToString("f2"));
        }
        return result;
    }

    /// <summary>
    /// 转换单位
    /// </summary>
    /// <param name="l"></param>
    /// <returns></returns>
    public static string ToSpeedString(this long l)
    {
        return Convert(l);
    }

    /// <summary>
    /// 字符串拆分成ip和port
    /// </summary>
    /// <param name="ipStr"></param>
    /// <returns></returns>
    public static ValueTuple<string, int> ToIPPort(this string ipStr)
    {
        try
        {
            ValueTuple<string, int> result;

            var arr = ipStr.Split([":"], StringSplitOptions.None);

            if (string.IsNullOrEmpty(arr[0])) arr[0] = "127.0.0.1";

            var ip = arr[0];

            if (string.IsNullOrEmpty(arr[1]))
            {
                throw new Exception("port:" + arr[1]);
            }

            var ai = arr[1].IndexOf("@");

            if (ai > -1)
            {
                arr[1] = arr[1].Substring(0, ai);
            }

            var port = int.Parse(arr[1]);

            result = new ValueTuple<string, int>(ip, port);

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception("转换成IPPort失败，ipStr 内容格式不正确，ipStr：" + ipStr, ex);
        }
    }

    /// <summary>
    /// 将字符串转换成IPEndPoint
    /// </summary>
    /// <param name="remote"></param>
    /// <returns></returns>
    public static IPEndPoint ToIPEndPoint(this string remote)
    {
        var tuple = remote.ToIPPort();
        return new IPEndPoint(IPAddress.Parse(tuple.Item1), tuple.Item2);
    }

    /// <summary>
    /// Split
    /// </summary>
    /// <param name="str"></param>
    /// <param name="none"></param>
    /// <param name="splits"></param>
    /// <returns></returns>
    public static string[] SplitToArray(this string str, bool none = false, params string[] splits)
    {
        if (string.IsNullOrEmpty(str)) return [];
        return str.Split(splits, none ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splits"></param>
    /// <returns></returns>
    public static List<string> SplitToList(this string str, params string[] splits)
    {
        if (string.IsNullOrEmpty(str)) return [];
        return str.Split(splits, StringSplitOptions.None).ToList();
    }

    /// <summary>
    /// 自定义分隔
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splitStr"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public static string[] Split(this string str, string splitStr, StringSplitOptions option)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(splitStr) || str.IndexOf(splitStr) == -1) return new string[] { str };

        var strSpan = str.AsSpan();

        var splitSapn = splitStr.AsSpan();

        int m = 0, n = 0;

        var arr = new List<string>();

        while (true)
        {
            m = n;
            n = strSpan.IndexOf(splitSapn);
            if (n > -1)
            {
                arr.Add(strSpan.Slice(0, n).ToString());
                strSpan = strSpan.Slice(n + splitSapn.Length);
            }
            else
            {
                arr.Add(strSpan.Slice(0).ToString());
                break;
            }
        }
        if (option == StringSplitOptions.RemoveEmptyEntries)
        {
            arr.RemoveAll(b => string.IsNullOrEmpty(b));
        }
        return arr.ToArray();
    }

    /// <summary>
    /// Split
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splitStr"></param>
    /// <returns></returns>
    public static string[] Split(this string str, string splitStr)
    {
        if (str.IsNullOrEmpty()) return [];
        if (splitStr.IsNullOrEmpty()) return [str];
        return str.Split(splitStr, StringSplitOptions.None);
    }

    /// <summary>
    /// Substring
    /// </summary>
    /// <param name="str"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string Substring(this string str, int start, int length)
    {
        return str.AsSpan(start, length).ToString();
    }
    /// <summary>
    /// Substring
    /// </summary>
    /// <param name="str"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static string Substring(this string str, int start)
    {
        return str.AsSpan(start, str.Length - start).ToString();
    }
    /// <summary>
    /// ParseToInt
    /// </summary>
    /// <param name="str"></param>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static int ParseToInt(this string str, int start, int count)
    {
        return int.Parse(Substring(str, start, count));
    }

    /// <summary>
    /// byte[]转为16进制字符串
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ByteToHexStr(this byte[] bytes)
    {
        StringBuilder returnStr = new StringPlus();
        if (bytes != null)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                returnStr.Append(bytes[i].ToString("X2"));
            }
        }
        return returnStr.ToString();
    }

    /// <summary>
    /// 将16进制的字符串转为byte[]
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public static byte[] StrToHexByte(this string hexString)
    {
        hexString = hexString.Replace(" ", "");
        if ((hexString.Length % 2) != 0)
            hexString += " ";
        byte[] returnBytes = new byte[hexString.Length / 2];
        for (int i = 0; i < returnBytes.Length; i++)
            returnBytes[i] = System.Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        return returnBytes;
    }


    /// <summary>
    /// 将16进制字符串转为字符串
    /// </summary>
    /// <param name="hs"></param>
    /// <param name="encode"></param>
    /// <returns></returns>
    public static string HexStringToString(this string hs, Encoding encode)
    {
        string strTemp = "";
        byte[] b = new byte[hs.Length / 2];
        for (int i = 0; i < hs.Length / 2; i++)
        {
            strTemp = hs.Substring(i * 2, 2);
            b[i] = System.Convert.ToByte(strTemp, 16);
        }
        //按照指定编码将字节数组变为字符串
        return encode.GetString(b);
    }
    /// <summary>
    /// 将字符串转为16进制字符，允许中文
    /// </summary>
    /// <param name="s"></param>
    /// <param name="encode"></param>
    /// <param name="spanString"></param>
    /// <returns></returns>
    public static string StringToHexString(this string s, Encoding encode, string spanString)
    {
        byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
        string result = string.Empty;
        for (int i = 0; i < b.Length; i++)//逐字节变为16进制字符
        {
            result += System.Convert.ToString(b[i], 16) + spanString;
        }
        return result;
    }




    /// <summary>
    /// 计算含有emoji表情字符串的长度
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int GetStringLength(this string str)
    {
        int len = 0;

        if (!string.IsNullOrEmpty(str))
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            int i = 0;

            while (i < bytes.Length)
            {
                var k = bytes[i];

                if (k <= 127)
                {
                    i += 1;
                }
                else if (k < 224)
                {
                    i += 2;
                }
                else if (k < 240)
                {
                    i += 3;
                }
                else
                {
                    i += 4;
                }
                len++;
            }

        }

        return len;
    }


    /// <summary>
    /// 将字符串转为列表
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splitStrs"></param>
    /// <returns></returns>
    public static List<string> GetList(this string str, params string[] splitStrs)
    {
        if (!string.IsNullOrEmpty(str) && splitStrs != null && splitStrs.Any())
        {
            return str.Split(splitStrs, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        return [];
    }

    /// <summary>
    /// 获取指定数量的重复字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string Repeat(this string str, int size)
    {
        if (str.IsNullOrEmpty() || size < 2) return str;
        var sp = new StringPlus(str);
        for (int i = 0; i < size - 1; i++)
        {
            sp.Append(str);
        }
        return sp.ToString();
    }

    /// <summary>
    /// 合并集合到字符中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="splitStr"></param>
    /// <returns></returns>
    public static string? GetString<T>(this IEnumerable<T> list, string splitStr = ";")
    {
        if (list != null && list.Any())
        {
            return string.Join(splitStr, list);
        }

        return null;
    }

    /// <summary>
    /// 返回去除utf8头的字符
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string? ToNoTitleString(this byte[]? data)
    {
        if (data != null && data.Length > 3)
        {
            if (data[0] == 239 && data[1] == 187 && data[2] == 191)
            {
                return Encoding.UTF8.GetString(data.AsSpan().Slice(3));
            }
            else
            {
                return Encoding.UTF8.GetString(data);
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 比较字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="txt"></param>
    /// <param name="isIgnoreCase"></param>
    /// <returns></returns>
    public static bool Equals(this string? str, string txt, bool isIgnoreCase)
    {
        if (str.IsNullOrEmpty() || txt.IsNullOrEmpty()) return false;
        if (isIgnoreCase)
            return str.Equals(txt, StringComparison.InvariantCultureIgnoreCase);
        else
            return str.Equals(txt);
    }

    /// <summary>
    /// 查找字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="txt"></param>
    /// <param name="isIgnoreCase"></param>
    /// <returns></returns>
    public static int IndexOf(this string? str, string txt, bool isIgnoreCase)
    {
        if (str.IsNullOrEmpty() || txt.IsNullOrEmpty()) return -1;
        if (isIgnoreCase)
            return str.IndexOf(txt, StringComparison.InvariantCultureIgnoreCase);
        else
            return str.IndexOf(txt);
    }

    /// <summary>
    /// 查找字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="txt"></param>
    /// <param name="isIgnoreCase"></param>
    /// <returns></returns>
    public static bool Contains(this string? str, string txt, bool isIgnoreCase)
    {
        if (str.IsNullOrEmpty() || txt.IsNullOrEmpty()) return false;

        if (isIgnoreCase)
            return str.Contains(txt, StringComparison.InvariantCultureIgnoreCase);
        else
            return str.Contains(txt);
    }

    /// <summary>
    /// 首字母大写
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ToTitleCase(this string? text)
    {
        if (text.IsNullOrEmpty()) return string.Empty;
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
    }

    /// <summary>
    /// 转换成安全字符,
    /// 防止sql注入
    /// </summary>
    /// <param name="sqlParams"></param>
    /// <returns></returns>
    public static string ToSafeString(this string? sqlParams)
    {
        if (string.IsNullOrWhiteSpace(sqlParams)) return string.Empty;
        sqlParams = sqlParams.Replace("\"", "“");
        sqlParams = sqlParams.Replace("\'", "‘");
        sqlParams = sqlParams.Replace("<", "《");
        sqlParams = sqlParams.Replace(">", "》");
        sqlParams = sqlParams.Replace("[", "【");
        sqlParams = sqlParams.Replace("]", "】");
        sqlParams = sqlParams.Replace("=", "﹦");
        sqlParams = sqlParams.Replace("+", "﹢");
        sqlParams = sqlParams.Replace("!", "！");
        sqlParams = sqlParams.Replace("(", "（");
        sqlParams = sqlParams.Replace(")", "）");
        sqlParams = sqlParams.Replace(";", "；");

        sqlParams = sqlParams.Replace("alter", "");
        sqlParams = sqlParams.Replace("and", "");
        sqlParams = sqlParams.Replace("applet", "");

        sqlParams = sqlParams.Replace("char", "");
        sqlParams = sqlParams.Replace("cast", "");
        sqlParams = sqlParams.Replace("count", "");
        sqlParams = sqlParams.Replace("create", "");
        sqlParams = sqlParams.Replace("chr", "");

        sqlParams = sqlParams.Replace("delete", "");
        sqlParams = sqlParams.Replace("drop", "");

        sqlParams = sqlParams.Replace("execute", "");
        sqlParams = sqlParams.Replace("exec", "");
        sqlParams = sqlParams.Replace("exists", "");

        sqlParams = sqlParams.Replace("insert", "");

        sqlParams = sqlParams.Replace("join", "");

        sqlParams = sqlParams.Replace("like", "");

        sqlParams = sqlParams.Replace("nchar", "");

        sqlParams = sqlParams.Replace("or", "");
        sqlParams = sqlParams.Replace("object", "");

        sqlParams = sqlParams.Replace("rename", "");

        sqlParams = sqlParams.Replace("select", "");
        sqlParams = sqlParams.Replace("show", "");
        sqlParams = sqlParams.Replace("script", "");

        sqlParams = sqlParams.Replace("truncate", "");

        sqlParams = sqlParams.Replace("union", "");
        sqlParams = sqlParams.Replace("update", "");

        sqlParams = sqlParams.Replace("where", "");

        sqlParams = sqlParams.Replace("xp_", "x p_");
        sqlParams = sqlParams.Replace("sp_", "s p_");
        sqlParams = sqlParams.Replace("0x", "0 x");
        sqlParams = sqlParams.Replace("?", "？");
        sqlParams = sqlParams.Replace("$", "￥");
        return sqlParams;
    }

    /// <summary>
    /// 首字母大小写
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="upper"></param>
    /// <returns></returns>
    public static string? ToTitleCase(this string? txt, bool upper = true)
    {
        if (txt.IsNullOrEmpty()) return txt;

        if (txt.Length < 1)
        {
            return upper ? txt.ToUpper() : txt.ToLower();
        }

        var first = txt.Substring(0, 1);

        first = upper ? first.ToUpper() : first.ToLower();

        return first + txt.Substring(1);
    }

    /// <summary>
    /// 获取字符串
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStr(this byte[] data)
    {
        if (data == null) return string.Empty;
        return Encoding.UTF8.GetString(data);
    }


    /// <summary>
    /// 是否包含
    /// </summary>
    /// <param name="strs"></param>
    /// <param name="str"></param>
    /// <param name="comparison"></param>
    /// <returns></returns>
    public static bool Contains(this IEnumerable<string> strs, string str, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(str)) return false;
        if (strs == null) return false;
        foreach (var item in strs)
        {
            if (!string.IsNullOrEmpty(item) && str.Equals(item, comparison)) return true;
        }
        return false;
    }



    /// <summary>
    /// IsNullOrEmpty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrEmpty(str);
    }
    /// <summary>
    /// NullOrWhiteSpace
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// IsNotNullOrEmpty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// IsNullOrWhiteSpace
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNotNullOrWhiteSpace([NotNullWhen(true)] this string? str)
    {
        return !string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// 是否相等(不区分大小写)
    /// </summary>
    /// <param name="str"></param>
    /// <param name="strs"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualIgnoreCase(this string? str, params string?[] strs)
    {
        if (strs == null || strs.Length == 0)
        {
            return false;
        }

        foreach (string? b in strs)
        {
            if (b.IsNotNullOrEmpty() && string.Equals(str, b, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 组合成一个字符串
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string Join(this object[] txt, string separator)
    {
        return string.Join(separator, txt);
    }

    /// <summary>
    /// 组合成一个字符串
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string Join(this IEnumerable<dynamic> txt, string separator)
    {
        return string.Join(separator, txt);
    }


    #region 拼音汉字互转

    /// <summary>
    /// 取中文文本的拼音首字母
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string GetInitials(this string text)
    {
        return Pinyin.GetInitials(text);
    }

    /// <summary>
    /// 取中文文本的拼音
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string GetPinyin(this string text)
    {
        return Pinyin.GetPinyin(text);
    }

    /// <summary>
    /// 取和拼音相同的汉字列表
    /// </summary>
    /// <param name="pinyin"></param>
    /// <returns></returns>
    public static string GetChineseText(this string pinyin)
    {
        return Pinyin.GetChineseText(pinyin);
    }

    #endregion

    /// <summary>
    /// 转换指定格式字符串，
    /// 保留两位小数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string ToString(this object? obj, string format)
    {
        return float.TryParse(obj?.ToString() ?? format, out float val) ? val.ToString(format) : "0.00";
    }

    /// <summary>
    /// 移除尾字符
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    public static string RemoveFirst(this string txt)
    {
        if (txt.IsNullOrEmpty()) return string.Empty;
        return txt.Substring(1);
    }

    /// <summary>
    /// 移除首字符
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    public static string RemoveLast(this string txt)
    {
        if (txt.IsNullOrEmpty()) return string.Empty;
        return txt.Substring(0, txt.Length - 1);
    }

    /// <summary>
    /// 将jwt字符串转成header
    /// </summary>
    /// <param name="jwt"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ToJwtHeader(this string jwt)
    {
        if (!jwt.StartsWith("Bearer "))
        {
            jwt = $"Bearer {jwt}";
        }
        Dictionary<string, string> header = new()
        {
            { "Authorization", jwt }
        };
        return header;
    }

    /// <summary>
    /// 从字符串中检索子字符串，在指定头部字符串之后，指定尾部字符串之前
    /// </summary>
    /// <param name="str"></param>
    /// <param name="after"></param>
    /// <param name="before"></param>
    /// <param name="startIndex"></param>
    /// <param name="positions"></param>
    /// <returns></returns>
    public static string Substring(this string str, string? after, string? before = null, int startIndex = 0, int[]? positions = null)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        if (string.IsNullOrEmpty(after) && string.IsNullOrEmpty(before))
        {
            return str;
        }

        int num = -1;
        if (!after.IsNullOrEmpty())
        {
            num = str.IndexOf(after, startIndex);
            if (num < 0)
            {
                return string.Empty;
            }

            num += after.Length;
            if (positions != null && positions.Length != 0)
            {
                positions[0] = num;
            }
        }

        if (string.IsNullOrEmpty(before))
        {
            int num2 = num;
            return str.Substring(num2, str.Length - num2);
        }

        int num3 = str.IndexOf(before, (num >= 0) ? num : startIndex);
        if (num3 < 0)
        {
            return string.Empty;
        }

        if (positions != null && positions.Length > 1)
        {
            positions[1] = num3;
        }

        if (num >= 0)
        {
            int num2 = num;
            return str.Substring(num2, num3 - num2);
        }

        return str.Substring(0, num3);
    }


    /// <summary>
    /// 切割骆驼命名式字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string[] SplitCamelCase(this string str)
    {
        if (str == null) return Array.Empty<string>();

        if (string.IsNullOrWhiteSpace(str)) return [str];
        if (str.Length == 1) return [str];

        return Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})")
            .Where(u => u.Length > 0)
            .ToArray();
    }


    /// <summary>
    /// 将下划线分隔的字符串转换为帕斯卡命名
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public static string ConvertToPropertyName(this string columnName)
    {
        var txt = Regex.Replace(columnName, @"_([a-z])", m => m.Groups[1].Value.ToUpper());
        return string.Concat(txt.Select((c, i) => i == 0 ? char.ToUpper(c).ToString() : c.ToString()));
    }

    /// <summary>
    /// 将驼峰命名的字符串转换为下划线分隔
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static string ConvertToColumnName(this string propertyName)
    {
        return Regex.Replace(propertyName, @"([A-Z])([A-Z])([a-z])|([a-z])([A-Z])", "$1$4_$2$3$5");
    }
    /// <summary>
    /// 将下划线分隔的字符串转换为驼峰命名
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public static string ConvertToCamelCase(this string columnName)
    {
        return Regex.Replace(columnName, @"_([a-z])", m => m.Groups[1].Value.ToUpper());
    }


    /// <summary>
    /// 转换大驼峰法命名
    /// </summary>
    /// <param name="columnName">字段名</param>
    /// <param name="dbColumnNames">EntityBase 实体属性名称</param>
    /// <returns></returns>
    public static string ConvertToCamelCase(this string columnName, string[]? dbColumnNames)
    {
        if (columnName.Contains('_'))
        {
            var arrColName = columnName.Split('_');
            var sb = new StringBuilder();
            foreach (var col in arrColName)
            {
                if (col.Length > 0)
                    sb.Append(col[..1].ToUpper() + col[1..].ToLower());
            }
            columnName = sb.ToString();
        }
        else
        {
            if (dbColumnNames != null && dbColumnNames.Length > 0)
            {
                var propertyName = dbColumnNames.FirstOrDefault(c => c.ToLower() == columnName.ToLower());
                if (!string.IsNullOrEmpty(propertyName))
                {
                    columnName = propertyName;
                }
                else
                {
                    columnName = columnName[..1].ToUpper() + columnName[1..].ToLower();
                }
            }
            else
            {
                columnName = columnName[..1].ToUpper() + columnName[1..].ToLower();
            }

        }
        return columnName;

    }

    /// <summary>
    /// 获取截取的字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="indexOfStr"></param>
    /// <param name="toSize"></param>
    /// <returns></returns>
    public static string SubstringFrom(this string str, string indexOfStr, int toSize = 0)
    {
        if (str.IsNullOrEmpty()) return str;
        var index = str.IndexOf(indexOfStr);
        if (index < 0) return string.Empty;
        if (toSize < 1)
        {
            return str.Substring(index);
        }
        return str.Substring(index, toSize);
    }

    /// <summary>
    /// 获取截取的字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="indexOfStr"></param>
    /// <param name="fromIndex"></param>
    /// <returns></returns>
    public static string SubstringTo(this string str, string indexOfStr, int fromIndex = 0)
    {
        if (str.IsNullOrEmpty()) return str;
        var index = str.IndexOf(indexOfStr);
        if (index < 0) return string.Empty;
        if (fromIndex < 0 || fromIndex > str.Length)
        {
            return string.Empty;
        }
        return str.Substring(fromIndex, index);
    }

    /// <summary>
    /// 转义 Markdown 特殊符号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string EscapeMarkdown(this string input)
    {
        if (input.IsNullOrEmpty()) return input;

        // 转义 Markdown 特殊符号
        return input
            .Replace(@"\", @"\\")
            .Replace("`", @"\`")
            .Replace("*", @"\*")
            .Replace("_", @"\_")
            .Replace("{", @"\{")
            .Replace("}", @"\}")
            .Replace("[", @"\[")
            .Replace("]", @"\]")
            .Replace("(", @"\(")
            .Replace(")", @"\)")
            .Replace("#", @"\#")
            .Replace("+", @"\+")
            .Replace("-", @"\-")
            .Replace(".", @"\.")
            .Replace("!", @"\!")
            .Replace("|", @"\|");
    }


    /// <summary>
    /// 驼峰转下划线
    /// </summary>
    /// <param name="str"></param>
    /// <param name="isToUpper"></param>
    /// <returns></returns>
    public static string ToUnderLine(this string str, bool isToUpper = false)
    {
        if (string.IsNullOrEmpty(str) || str.Contains("_"))
        {
            return str;
        }

        int length = str.Length;
        var result = new System.Text.StringBuilder(length + (length / 3));

        result.Append(char.ToLowerInvariant(str[0]));

        int lastIndex = length - 1;

        for (int i = 1; i < length; i++)
        {
            char current = str[i];
            if (!char.IsUpper(current))
            {
                result.Append(current);
                continue;
            }

            bool prevIsLower = char.IsLower(str[i - 1]);
            bool nextIsLower = (i < lastIndex) && char.IsLower(str[i + 1]);

            if (prevIsLower || nextIsLower)
            {
                result.Append('_');
            }

            result.Append((char)(current | 0x20));
        }

        string converted = result.ToString();
        return isToUpper ? converted.ToUpperInvariant() : converted;
    }
}
