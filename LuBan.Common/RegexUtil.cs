/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： RegexUtil
*版本号： V1.0.0.0
*唯一标识：7d555db4-658c-4569-96e5-cf8b3420ee3a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/5/16 15:21:08
*描述：正则表达式工具类
*
*=================================================
*修改标记
*修改时间：2023/5/16 15:21:08
*修改人： yswenli
*版本号： V1.0.0.0
*描述：正则表达式工具类
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// 正则表达式工具类
    /// </summary>
    public static class RegexUtil
    {
        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static bool IsMatch(string txt, string regex)
        {
            if (txt.IsNullOrEmpty()) return false;
            return Regex.IsMatch(txt, regex);
        }

        /// <summary>
        /// 返回所有匹配的内容列表
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static List<string> ToList(this string txt, string pattern)
        {
            List<string> list = new List<string>();
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = reg.Matches(txt);
            foreach (Match item in matches)
            {
                list.Add(item.Value);
            }
            return list;
        }

        /// <summary>
        /// 是否是中文
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsChinese(this string txt)
        {
            return IsMatch(txt, "[\u4e00-\u9fa5]");
        }
        /// <summary>
        /// 是否是邮箱地址
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsEmail(this string txt)
        {
            return IsMatch(txt, "\\w[-\\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\\.)+[A-Za-z]{2,14}");
        }
        /// <summary>
        /// 是否是url
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsUrl(this string txt)
        {
            return IsMatch(txt, "[a-zA-z]+://[^\\s]*");
        }
        /// <summary>
        /// 是否是手机号
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsMobile(this string txt)
        {
            return IsMatch(txt, "(1)[0-9]{10}");
        }

        /// <summary>
        /// 是否是固话
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsPhone(this string txt)
        {
            return IsMatch(txt, @"^0\d{2,3}-\d{7,8}$");
        }


        /// <summary>
        /// 是否是身份证
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsIDcard(this string txt)
        {
            return IsMatch(txt, @"(^\d{18}$)|(^\d{15}$)");
        }

        /// <summary>
        /// 验证邮政编码
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsPostalcode(this string txt)
        {
            return IsMatch(txt, @"^\d{6}$");
        }


        /// <summary>
        /// 验证小数格式
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsDecimal(this string txt)
        {
            return IsMatch(txt, @"^[0-9]+\.[0-9]{2}$");
        }
        /// <summary>
        /// 验证月份
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsMonth(this string txt)
        {
            return IsMatch(txt, @"^(0?[[1-9]|1[0-2])$");
        }
        /// <summary>
        /// 验证天数
        /// </summary>
        /// <param name="str_day"></param>
        /// <returns></returns>
        public static bool IsDay(this string str_day)
        {
            return IsMatch(str_day, @"^((0?[1-9])|((1|2)[0-9])|30|31)$");
        }
        /// <summary>
        /// 验证是否为数字
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsNumber(this string txt)
        {
            return IsMatch(txt, @"^\d*$");
        }
        /// <summary>
        /// 验证正整数
        /// </summary>
        /// <param name="str_intNumber"></param>
        /// <returns></returns>
        public static bool IsIntNumber(this string str_intNumber)
        {
            return IsMatch(str_intNumber, @"^\+?[1-9][0-9]*$");
        }
        /// <summary>
        /// 验证大小写
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsUpChar(this string txt)
        {
            return IsMatch(txt, @"^[A-Z]+$");
        }
        /// <summary>
        /// 验证大小写
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsLowerChar(this string txt)
        {
            return IsMatch(txt, @"^[a-z]+$");
        }

        /// <summary>
        /// 验证是否为字母
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsLetter(this string txt)
        {
            return IsMatch(txt, @"^[A-Za-z]+$");
        }

        /// <summary>
        /// 验证密码强度，
        /// 大小写+数据+特殊符号+指定位数
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="size"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsPassword(this string txt, int size = 16, int type = 1)
        {
            switch (type)
            {
                case 2:
                    return IsMatch(txt, @"(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{" + size + "}");
                case 3:
                    return IsMatch(txt, @"(?=.*[0-9])(?=.*[a-zA-Z])(?=([\x21-\x7e]+)[^a-zA-Z0-9]).{" + size + "}");
                case 1:
                default:
                    return IsMatch(txt, @"\d{" + size + "}");
            }
        }

        /// <summary>
        /// 验证IP
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsIPAddress(this string txt)
        {
            string num = @"(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)";
            return IsMatch(txt, ("^" + num + "\\." + num + "\\." + num + "\\." + num + "$"));
        }

        /// <summary>
        /// 是否是日期格式
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsDateTime(this string txt)
        {
            return IsMatch(txt, "\\d{4}(\\-|\\/|.)\\d{1,2}\\1\\d{1,2}");
        }



        /// <summary>
        /// 手机号码脱敏（中间4位用星号代替）
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static string MobileEncryption(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return string.Empty;
            }
            Regex re = new Regex(@"(\d{3})(\d{4})(\d{4})", RegexOptions.None);
            mobile = re.Replace(mobile, "$1****$3");
            return mobile;
        }
    }
}
