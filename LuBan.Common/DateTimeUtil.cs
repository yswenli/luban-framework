/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： DateTimeUtil
*版本号： V1.0.0.0
*唯一标识：3c53a79e-3f1e-4d82-8a4b-9a1733545356
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 10:54:39
*描述：时间处理类
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 10:54:39
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：时间处理类
*
*****************************************************************************/
using LuBan.Common.Calendar;

namespace LuBan.Common;

/// <summary>
/// 时间处理类
/// </summary>
public static class DateTimeUtil
{

    /// <summary>
    /// 当地时间
    /// </summary>
    public static DateTime Now
    {
        get
        {
            return DateTime.Now;
        }
    }

    /// <summary>
    /// utc时间
    /// </summary>
    public static DateTime UtcNow
    {
        get
        {
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, TimeZoneInfo.Local);
        }
    }

    /// <summary>
    /// 今天
    /// </summary>
    public static DateTime Today
    {
        get
        {
            return DateTime.Today;
        }
    }

    /// <summary>
    /// 字符串转换成datetime
    /// </summary>
    /// <param name="dateTimeStr"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static bool TryParse(string dateTimeStr, out DateTime dateTime)
    {
        return DateTime.TryParse(dateTimeStr, out dateTime);
    }

    /// <summary>
    /// 根据时区获取时间,
    /// DateTimeZoneID.中国标准时间
    /// </summary>
    /// <param name="zoneId"></param>
    /// <returns></returns>
    public static DateTime GetDateTimeByZone(string zoneId)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(UtcNow, TimeZoneInfo.FindSystemTimeZoneById(zoneId));
    }

    /// <summary>
    /// 不同时区的日期转换，
    /// DateTimeZoneID.中国标准时间
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="fromZoneId"></param>
    /// <param name="toZoneId"></param>
    /// <returns></returns>
    public static DateTime ToOtherZone(this DateTime dt, string fromZoneId, string toZoneId)
    {
        return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById(fromZoneId), TimeZoneInfo.FindSystemTimeZoneById(toZoneId));
    }

    /// <summary>
    /// 将DateTime.Now.Ticks转换成DateTime
    /// </summary>
    /// <param name="ticks"></param>
    /// <returns></returns>
    public static DateTime GetDateTimeByTickets(long ticks)
    {
        return DateTime.FromBinary(ticks);
    }

    /// <summary>
    /// 当前时间字符串
    /// </summary>
    public static string NowString
    {
        get
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
    }
    /// <summary>
    /// Token所需时间
    /// 5分种间隔的时间
    /// </summary>
    public static DateTime TokenDateTime
    {
        get
        {
            int nm = 0;

            DateTime dt = DateTimeUtil.Now;

            int m = dt.Minute;

            //个位分钟数
            int s = (int)(((m * 0.1) - Math.Floor(m * 0.1)) * 10);

            if (s < 5)
            {
                nm = m - s;
            }
            else
            {
                nm = m - s + 5;
            }

            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, nm, 0);
        }
    }
    /// <summary>
    /// 返回当前时间的总秒数
    /// </summary>
    /// <returns></returns>
    public static int GetTotalSeconds()
    {
        var ticks = DateTime.Now.Ticks;
        var ts = new TimeSpan(ticks);
        return (int)ts.TotalSeconds;
    }


    /// <summary>
    /// 字符串时间类型转换为时间类型
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ConvertStrToTime(string dateTime)
    {
        if (!string.IsNullOrEmpty(dateTime))
            return DateTime.Parse(dateTime);
        else
            return DateTime.Now;
    }

    /// <summary>
    /// 转换成yyyy-MM-dd的格式
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string ToDateString(this DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 返回日期时间字符串，格式：yyyy-MM-dd HH:mm:ss.fff
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToDateTimeString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    /// <summary>
    /// 返回日期时间字符串，格式：yyyy-MM-dd HH:mm:ss.fff
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToDateTimeString(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss.fff")
    {
        return dateTime.ToString(format);
    }

    /// <summary>
    /// 获取日期时间格式MMddHHmmss
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToShortDateTimeString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyyMMdd");
    }
    /// <summary>
    /// 返回日期时间字符串，格式：yyyy-MM-dd HH:mm:ss.fff
    /// 如果为null，则返回空字符串
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToDateTimeString(this DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ToDateTimeString() : string.Empty;
    }

    /// <summary>
    /// 获取当前的时间int值
    /// </summary>
    /// <returns></returns>
    public static int ConvertDateTimeInt()
    {
        return (int)Now.ToUnixTimeStamp(false);
    }

    /// <summary>
    /// 返回简约的中文时间
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string DateStringFromNow(DateTime dt)
    {
        if (dt.ToString("yyyy-MM-dd") == "0001-01-01") return string.Empty;

        TimeSpan span = DateTime.Now - dt;

        if (span.TotalDays > 60)
        {
            return dt.ToString("yyyy-MM-dd");
        }
        else
        {
            if (span.TotalDays > 365)
            {
                return string.Format("{0}年前", (int)Math.Floor(span.TotalDays / 365));
            }
            if (span.TotalDays > 180)
            {
                return "半年前";
            }
            if (span.TotalDays > 30)
            {
                return "1个月前";
            }
            else
            {
                if (span.TotalDays > 14)
                {
                    return "2周前";
                }
                else
                {
                    if (span.TotalDays > 7)
                    {
                        return "1周前";
                    }
                    else
                    {
                        if (span.TotalDays > 1)
                        {
                            return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
                        }
                        else
                        {
                            if (span.TotalHours > 1)
                            {
                                return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                            }
                            else
                            {
                                if (span.TotalMinutes > 1)
                                {
                                    return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
                                }
                                else
                                {
                                    if (span.TotalSeconds >= 1)
                                    {
                                        return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
                                    }
                                    else
                                    {
                                        return "1秒前";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// 默认开始时间:1970/1/1
    /// </summary>
    private static DateTime _defaultStartTime = new DateTime(1970, 1, 1, 0, 0, 0);

    /// <summary>
    /// 将时间转换到UTC时间
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToUTCFormat(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    /// <summary>
    /// 将时间转换到unix时间戳
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="hasMilliseconds"></param>
    /// <returns></returns>
    public static long ToUnixTimeStamp(this DateTime dateTime, bool hasMilliseconds = true)
    {
        if (hasMilliseconds)
            return DateTimeOffset.Now.ToLocalTime().ToUnixTimeMilliseconds();
        else
            return DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds();
    }

    /// <summary>
    /// 将unix时间戳转换为本地时间
    /// </summary>
    /// <param name="unixTime"></param>
    /// <param name="hasMilliseconds"></param>
    /// <returns></returns>
    public static DateTime FromUnixTimeStamp(this long unixTime, bool hasMilliseconds = true)
    {
        if (hasMilliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).ToLocalTime().DateTime;
        }
        else
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime().DateTime;
        }
    }
    /// <summary>
    /// 将unix时间戳转换为本地,单位毫秒(与ToUnixTime为互转)
    /// </summary>
    /// <param name="unixTime"></param>
    /// <param name="hasMilliseconds"></param>
    /// <returns></returns>
    public static DateTime ToUtcTime(this long unixTime, bool hasMilliseconds = true)
    {
        if (hasMilliseconds)
        {
            return _defaultStartTime.AddMilliseconds((double)unixTime);
        }
        else
        {
            return _defaultStartTime.AddSeconds((double)unixTime);
        }
    }


    /// <summary>
    /// ISO8601时间格式,
    /// utc格式
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string ToISO8601String(this DateTime dt)
    {
        return dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    /// <summary>
    /// 转换成UTC
    /// </summary>
    /// <param name="dateTimeStr"></param>
    /// <returns></returns>
    public static DateTime ParseUTC(string dateTimeStr)
    {
        return DateTime.Parse(dateTimeStr, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal);
    }

    /// <summary>
    /// 世界时区名称
    /// </summary>
    public class DateTimeZoneID
    {
        /// <summary>
        /// GMT-12:00
        /// </summary>
        public const string 日界线西国际日期变更线标准时间 = "Dateline Standard Time";
        /// <summary>
        /// GMT-11:00
        /// </summary>
        public const string 中途岛萨摩亚群岛萨摩亚群岛标准时间 = "Samoa Standard Time";
        /// <summary>
        /// GMT-10:00
        /// </summary>
        public const string 夏威夷标准时间 = "Hawaiian Standard Time";
        /// <summary>
        /// GMT-09:00
        /// </summary>
        public const string 阿拉斯加标准时间 = "Alaskan Standard Time";
        /// <summary>
        /// GMT-08:00,
        /// 美国和加拿大
        /// </summary>
        public const string 蒂华纳太平洋标准时间 = "Pacific Standard Time";
        /// <summary>
        /// GMT-07:00
        /// </summary>
        public const string 亚利桑那美国山地标准时间 = "US Mountain Standard Time";
        /// <summary>
        /// GMT-07:00,
        /// 山地时间(美国和加拿大),山地标准时间
        /// </summary>
        public const string 山地标准时间 = "Mountain Standard Time";
        /// <summary>
        /// GMT-07:00,
        /// 奇瓦瓦，拉巴斯，马扎特兰,墨西哥标准时间 2
        /// </summary>
        public const string 墨西哥标准时间2 = "Mexico Standard Time 2";
        /// <summary>
        /// GMT-06:00
        /// </summary>
        public const string 中美洲标准时间 = "Central America Standard Time";
        /// <summary>
        /// GMT-06:00,
        /// 美国和加拿大中部标准时间
        /// </summary>
        public const string 中部标准时间 = "Central Standard Time";
        /// <summary>
        /// GMT-06:00
        /// </summary>
        public const string 萨斯喀彻温加拿大中部标准时间 = "Canada Central Standard Time";
        /// <summary>
        /// GMT-06:00,
        /// 瓜达拉哈拉，墨西哥城，蒙特雷,墨西哥标准时间
        /// </summary>
        public const string 墨西哥标准时间 = "Mexico Standard Time";
        /// <summary>
        /// GMT-05:00,
        /// 东部时间(美国和加拿大),东部标准时间
        /// </summary>
        public const string 东部标准时间 = "Eastern Standard Time";
        /// <summary>
        /// GMT-05:00,
        /// 印地安那州(东部),美国东部标准时间
        /// </summary>
        public const string 美国东部标准时间 = "US Eastern Standard Time";
        /// <summary>
        /// GMT-05:00,
        /// 波哥大，利马，基多,南美州太平洋标准时间
        /// </summary>
        public const string 南美州太平洋标准时间 = "SA Pacific Standard Time";
        /// <summary>
        /// GMT-04:00,
        /// 加拉加斯，拉巴斯,南美州西部标准时间
        /// </summary>
        public const string 南美州西部标准时间 = "SA Western Standard Time";
        /// <summary>
        /// GMT-04:00,
        /// 圣地亚哥,南美州太平洋标准时间
        /// </summary>
        public const string 圣地亚哥南美州太平洋标准时间 = "Pacific SA Standard Time";
        /// <summary>
        /// GMT-04:00,
        /// 大西洋时间(加拿大),大西洋标准时间
        /// </summary>
        public const string 大西洋标准时间 = "Atlantic Standard Time";
        /// <summary>
        /// GMT-03:30
        /// </summary>
        public const string 纽芬兰标准时间 = "Newfoundland Standard Time";
        /// <summary>
        /// GMT-03:00,
        /// 巴西利亚,南美州东部标准时间
        /// </summary>
        public const string 巴西利亚南美州东部标准时间 = "E. South America Standard Time";
        /// <summary>
        /// GMT-03:00,
        /// 布宜诺斯艾利斯，乔治敦,南美州东部标准时间
        /// </summary>
        public const string 南美州东部标准时间 = "SA Eastern Standard Time";
        /// <summary>
        /// GMT-03:00,
        /// 格陵兰东部标准时间
        /// </summary>
        public const string 格陵兰东部标准时间 = "Greenland Standard Time";
        /// <summary>
        /// GMT-02:00
        /// </summary>
        public const string 中大西洋标准时间 = "Mid-Atlantic Standard Time";
        /// <summary>
        /// GMT-01:00
        /// </summary>
        public const string 亚速尔群岛标准时间 = "Azores Standard Time";
        /// <summary>
        /// GMT-01:00
        /// </summary>
        public const string 佛得角群岛标准时间 = "Cape Verde Standard Time";
        /// <summary>
        /// 卡萨布兰卡，蒙罗维亚,格林威治标准时间
        /// </summary>
        public const string 格林威治标准时间 = "Greenwich Standard Time";
        /// <summary>
        /// 格林威治标准时间: 都柏林, 爱丁堡, 伦敦, 里斯本,格林威治标准时间
        /// </summary>
        public const string 伦敦都柏林里斯本格林威治标准时间 = "GMT Standard Time";
        /// <summary>
        /// GMT+01:00
        /// </summary>
        public const string 中非西部标准时间 = "W. Central Africa Standard Time";
        /// <summary>
        /// GMT+01:00,
        /// 布鲁塞尔，哥本哈根，马德里，巴黎,罗马标准时间
        /// </summary>
        public const string 罗马标准时间 = "Romance Standard Time";
        /// <summary>
        /// GMT+01:00,
        /// 萨拉热窝，斯科普里，华沙，萨格勒布,中欧标准时间
        /// </summary>
        public const string 中欧标准时间1 = "Central European Standard Time";
        /// <summary>
        /// GMT+01:00,
        /// 贝尔格莱德，布拉迪斯拉发，布达佩斯，卢布尔雅那，布拉格,中欧标准时间
        /// </summary>
        public const string 中欧标准时间2 = "Central Europe Standard Time";
        /// <summary>
        /// GMT+01:00,
        /// 阿姆斯特丹，柏林，伯尔尼，罗马，斯德哥尔摩，维也纳,西欧标准时间
        /// </summary>
        public const string 西欧标准时间 = "W. Europe Standard Time";
        /// <summary>
        /// GMT+02:00,
        /// 哈拉雷，比勒陀利亚,南非标准时间
        /// </summary>
        public const string 南非标准时间 = "South Africa Standard Time";
        /// <summary>
        /// GMT+02:00,
        /// 布加勒斯特,东欧标准时间
        /// </summary>
        public const string 东欧标准时间 = "E. Europe Standard Time";
        /// <summary>
        /// GMT+02:00,
        /// 开罗,埃及标准时间
        /// </summary>
        public const string 埃及标准时间 = "Egypt Standard Time";
        /// <summary>
        /// GMT+02:00,
        /// 耶路撒冷标准时间
        /// </summary>
        public const string 耶路撒冷标准时间 = "Israel Standard Time";
        /// <summary>
        /// GMT+02:00,
        /// 赫尔辛基，基辅，里加，索非亚，塔林，维尔纽斯,FLE 标准时间
        /// </summary>
        public const string FLE标准时间 = "FLE Standard Time";
        /// <summary>
        /// GMT+02:00,
        /// 雅典，贝鲁特，伊斯坦布尔，明斯克,GTB 标准时间
        /// </summary>
        public const string GTB标准时间 = "GTB Standard Time";
        /// <summary>
        /// GMT+03:00,
        /// 内罗毕,东非标准时间
        /// </summary>
        public const string 东非标准时间 = "E. Africa Standard Time";
        /// <summary>
        /// GMT+03:00,
        /// 巴格达,阿拉伯标准时间
        /// </summary>
        public const string 阿拉伯标准时间1 = "Arabic Standard Time";
        /// <summary>
        /// GMT+03:00,
        /// 科威特，利雅得,阿拉伯标准时间
        /// </summary>
        public const string 阿拉伯标准时间2 = "Arab Standard Time";
        /// <summary>
        /// GMT+03:00,
        /// 莫斯科，圣彼得堡, 伏尔加格勒,俄罗斯标准时间
        /// </summary>
        public const string 俄罗斯标准时间 = "Russian Standard Time";
        /// <summary>
        /// GMT+03:30,
        /// 德黑兰,伊朗标准时间
        /// </summary>
        public const string 伊朗标准时间 = "Iran Standard Time";
        /// <summary>
        /// GMT+04:00,
        /// 巴库，第比利斯，埃里温,高加索标准时间
        /// </summary>
        public const string 高加索标准时间 = "Caucasus Standard Time";
        /// <summary>
        /// GMT+04:00,
        /// 阿布扎比，马斯喀特,阿拉伯半岛标准时间
        /// </summary>
        public const string 阿拉伯半岛标准时间 = "Arabian Standard Time";
        /// <summary>
        /// GMT+04:30,
        /// 喀布尔,阿富汗标准时间
        /// </summary>
        public const string 阿富汗标准时间 = "Afghanistan Standard Time";
        /// <summary>
        /// GMT+05:00,
        /// 伊斯兰堡，卡拉奇，塔什干,西亚标准时间
        /// </summary>
        public const string 西亚标准时间 = "West Asia Standard Time";
        /// <summary>
        /// GMT+05:00
        /// </summary>
        public const string 叶卡捷琳堡标准时间 = "Ekaterinburg Standard Time";
        /// <summary>
        /// GMT+05:30
        /// 马德拉斯，加尔各答，孟买，新德里,印度标准时间
        /// </summary>
        public const string 印度标准时间 = "India Standard Time";
        /// <summary>
        /// GMT+05:45,
        /// 尼泊尔标准时间
        /// </summary>
        public const string 尼泊尔标准时间 = "Nepal Standard Time";
        /// <summary>
        /// GMT+06:00
        /// </summary>
        public const string 斯里兰卡标准时间 = "Sri Lanka Standard Time";
        /// <summary>
        /// GMT+06:00
        /// 阿拉木图，新西伯利亚,中亚北部标准时间
        /// </summary>
        public const string 中亚北部标准时间 = "N. Central Asia Standard Time";
        /// <summary>
        /// GMT+06:00,
        /// 阿斯塔纳，达卡,中亚标准时间
        /// </summary>
        public const string 中亚标准时间 = "Central Asia Standard Time";
        /// <summary>
        /// GMT+06:30
        /// </summary>
        public const string 缅甸标准时间 = "Myanmar Standard Time";
        /// <summary>
        /// GMT+07:00
        /// </summary>
        public const string 北亚标准时间 = "North Asia Standard Time";
        /// <summary>
        /// GMT+07:00
        /// </summary>
        public const string 东南亚标准时间 = "SE Asia Standard Time";
        /// <summary>
        /// GMT+08:00
        /// </summary>
        public const string 北亚东部标准时间 = "North Asia East Standard Time";
        /// <summary>
        /// GMT+08:00,
        /// 北京，重庆，香港特别行政区，乌鲁木齐,中国标准时间
        /// </summary>
        public const string 中国标准时间 = "China Standard Time";
        /// <summary>
        /// GMT+08:00
        /// </summary>
        public const string 台北标准时间 = "Taipei Standard Time";
        /// <summary>
        /// GMT+08:00,
        /// 吉隆坡，新加坡,马来西亚半岛标准时间
        /// </summary>
        public const string 马来西亚半岛标准时间 = "Singapore Standard Time";
        /// <summary>
        /// GMT+08:00
        /// </summary>
        public const string 澳大利亚西部标准时间 = "W. Australia Standard Time";
        /// <summary>
        /// GMT+09:00,
        /// 大坂，札幌，东京,东京标准时间
        /// </summary>
        public const string 东京标准时间 = "Tokyo Standard Time";
        /// <summary>
        /// GMT+09:00,
        /// 汉城,韩国标准时间
        /// </summary>
        public const string 韩国标准时间 = "Korea Standard Time";
        /// <summary>
        /// GMT+09:00
        /// </summary>
        public const string 雅库茨克标准时间 = "Yakutsk Standard Time";
        /// <summary>
        /// GMT+09:30,
        /// 达尔文,澳大利亚中部标准时间
        /// </summary>
        public const string 澳大利亚中部标准时间1 = "AUS Central Standard Time";
        /// <summary>
        /// GMT+09:30,
        /// 阿德莱德,澳大利亚中部标准时间
        /// </summary>
        public const string 澳大利亚中部标准时间2 = "Cen. Australia Standard Time";
        /// <summary>
        /// GMT+10:00,
        /// 关岛，莫尔兹比港,西太平洋标准时间
        /// </summary>
        public const string 西太平洋标准时间 = "West Pacific Standard Time";
        /// <summary>
        /// GMT+10:00,
        /// 堪培拉，墨尔本，悉尼,澳大利亚东部标准时间
        /// </summary>
        public const string 澳大利亚东部标准时间1 = "AUS Eastern Standard Time";
        /// <summary>
        /// GMT+10:00,
        /// 布里斯班,澳大利亚东部标准时间
        /// </summary>
        public const string 澳大利亚东部标准时间2 = "E. Australia Standard Time";
        /// <summary>
        /// GMT+10:00
        /// </summary>
        public const string 符拉迪沃斯托克标准时间 = "Vladivostok Standard Time";
        /// <summary>
        /// GMT+10:00
        /// </summary>
        public const string 塔斯马尼亚岛标准时间 = "Tasmania Standard Time";
        /// <summary>
        /// GMT+11:00,
        /// 马加丹，索罗门群岛，新喀里多尼亚,太平洋中部标准时间
        /// </summary>
        public const string 太平洋中部标准时间 = "Central Pacific Standard Time";
        /// <summary>
        /// GMT+12:00,
        /// 奥克兰，惠灵顿,新西兰标准时间
        /// </summary>
        public const string 新西兰标准时间 = "New Zealand Standard Time";
        /// <summary>
        /// GMT+12:00
        /// </summary>
        public const string 斐济标准时间 = "Fiji Standard Time";
        /// <summary>
        /// GMT+13:00
        /// </summary>
        public const string 汤加标准时间 = "Tonga Standard Time";
    }



    /// <summary>
    /// 太平洋日期
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime PacificTime(this DateTime dateTime)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        var pacificNow = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), zone);
        return pacificNow;
    }

    /// <summary>
    /// 中国日期
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime CSTTime(this DateTime dateTime)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        var CSTNow = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), zone);
        return CSTNow;
    }

    /// <summary>
    /// 山区日期
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime MSTTime(this DateTime dateTime)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
        var ss = dateTime.ToUniversalTime();
        var CSTNow = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), zone);
        return CSTNow;
    }

    /// <summary>
    /// 太平洋标准时间
    /// </summary>
    /// <param name="pacificDateTime"></param>
    /// <returns></returns>
    public static DateTime FromPST2CSTTime(this DateTime pacificDateTime)
    {
        var cstTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(pacificDateTime, "Pacific Standard Time", "China Standard Time");
        return cstTime;
    }

    /// <summary>
    /// 获取上个月日期
    /// </summary>
    /// <returns></returns>
    public static DateTime GetLastMonth()
    {
        return Now.AddMonths(-1);
    }

    /// <summary>
    /// 获取开始和结束日期字符串
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static Tuple<string, string> GetBeginAndEndByMonth(this DateTime dateTime, string format = "yyyyMMdd")
    {
        var dt = new DateTime(dateTime.Year, dateTime.Month, 1);
        var begin = dt.ToString(format);
        var end = dt.AddMonths(1).AddDays(-1).ToString(format);
        return new Tuple<string, string>(begin, end);
    }

    /// <summary>
    /// 某时间之后的开始结束timestamp
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="days"></param>
    /// <returns></returns>
    public static Tuple<long, long> GetBeginAndEndForAfter(this DateTime dateTime, int days)
    {
        var begin = dateTime.ToUnixTimeStamp(false);
        var end = dateTime.AddDays(days).ToUnixTimeStamp(false);
        return new Tuple<long, long>(begin, end);
    }
    /// <summary>
    /// 某时间之前的开始结束timestamp
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="days"></param>
    /// <returns></returns>
    public static Tuple<long, long> GetBeginAndEndForBefore(this DateTime dateTime, int days)
    {
        var begin = dateTime.AddDays(-days).ToUnixTimeStamp(false);
        var end = dateTime.ToUnixTimeStamp(false);
        return new Tuple<long, long>(begin, end);
    }

    #region Week 相关方法

    /// <summary>
    /// 返回今天是第几周
    /// </summary>
    /// <returns></returns>
    public static int GetWeekOfYearByDate(DateTime dateTime)
    {
        DateTime time = Convert.ToDateTime(dateTime.ToString("yyyy") + "-01-01");
        TimeSpan ts = dateTime - time;
        int iii = (int)time.DayOfWeek;
        int day = int.Parse(ts.TotalDays.ToString("F0"));
        if (iii == 0)
        {
            day--;
        }
        else
        {
            day = day - (7 - iii) - 1;
        }
        int week = ((day + 7) / 7) + 1;
        return week;
    }

    /// <summary>
    /// 返回某周开始和结束日期字符串
    /// </summary>
    /// <param name="weekIndex"></param>
    /// <returns></returns>
    public static string GetDateRangeStrByWeek(int weekIndex)
    {
        var dTime = Now.AddDays(weekIndex * 7);
        //确定星期几
        int index = (int)dTime.DayOfWeek;

        index = index == 0 ? 7 : index;

        //当前周的范围
        DateTime retStartDay = dTime.AddDays(-(index - 1));
        DateTime retEndDay = dTime.AddDays(7 - index);
        return retStartDay.ToString("MM/dd") + "-" + retEndDay.ToString("MM/dd");
    }

    /// <summary>
    /// 返回某周开始日期
    /// </summary>
    /// <param name="weekIndex"></param>
    /// <returns></returns>
    public static DateTime GetDateRangeStartByWeek(int weekIndex)
    {
        var dTime = Now.AddDays(weekIndex * 7);
        //确定星期几
        int index = (int)dTime.DayOfWeek;

        index = index == 0 ? 7 : index;

        return DateTime.Parse(dTime.AddDays(-(index - 1)).ToShortDateString());
    }
    /// <summary>
    /// 返回某周开始和结束日期
    /// </summary>
    /// <param name="weekOfYear"></param>
    /// <returns></returns>
    public static Tuple<DateTime, DateTime> GetDateRangeByWeekOfYear(int weekOfYear)
    {
        var weekIndex = weekOfYear - GetWeekOfYearByDate(Now);
        var dTime = DateTime.Now.AddDays(weekIndex * 7);
        //确定星期几
        int index = (int)dTime.DayOfWeek;

        index = index == 0 ? 7 : index;

        var dateStart = DateTime.Parse(dTime.AddDays(-(index - 1)).ToShortDateString());
        var dateEnd = DateTime.Parse(dTime.AddDays(7 - index).ToShortDateString());
        var dates = new Tuple<DateTime, DateTime>(dateStart, dateEnd);
        return dates;
    }

    #endregion

    /// <summary>
    /// 转换成中国农历
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static ChineseCalendar ToChineseCalendar(this DateTime date)
    {
        return new ChineseCalendar(date);
    }

    /// <summary>
    /// 全局设置中文区域和时间格式
    /// </summary>
    public static void SetCultureAndFormat()
    {
        var culture = new CultureInfo("zh-CN", true);
        DateTimeFormatInfo dtfi = culture.DateTimeFormat;
        dtfi.LongDatePattern = "yyyy-MM-dd";
        dtfi.LongTimePattern = "HH:mm:ss";
        dtfi.ShortDatePattern = "yyyy-MM-dd";
        dtfi.ShortTimePattern = "HH:mm:ss";
        CultureInfo.CurrentCulture = culture;
    }

    /// <summary>
    /// 将ticks转换成日期
    /// </summary>
    /// <param name="ticks"></param>
    /// <returns></returns>
    public static DateTime FromTicks(long ticks)
    {
        return DateTime.FromBinary(ticks);
    }
    /// <summary>
    /// 将日期点转换成时间
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DateTimeOffset dateTimeOffset)
    {
        return FromTicks(dateTimeOffset.Ticks);
    }

    /// <summary>
    /// 将string转换为DateTime，若失败则返回日期最小值
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static DateTime ParseToDateTime(this string str)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return DateTime.MinValue;
            }
            if (str.Contains('-') || str.Contains('/'))
            {
                return DateTime.Parse(str);
            }
            else
            {
                int length = str.Length;
                switch (length)
                {
                    case 4:
                        return DateTime.ParseExact(str, "yyyy", CultureInfo.CurrentCulture);

                    case 6:
                        return DateTime.ParseExact(str, "yyyyMM", CultureInfo.CurrentCulture);

                    case 8:
                        return DateTime.ParseExact(str, "yyyyMMdd", CultureInfo.CurrentCulture);

                    case 10:
                        return DateTime.ParseExact(str, "yyyyMMddHH", CultureInfo.CurrentCulture);

                    case 12:
                        return DateTime.ParseExact(str, "yyyyMMddHHmm", CultureInfo.CurrentCulture);

                    case 14:
                        return DateTime.ParseExact(str, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

                    default:
                        return DateTime.ParseExact(str, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
                }
            }
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// 毫秒转天时分秒
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public static string FormatTime(long ms)
    {
        int ss = 1000;
        int mi = ss * 60;
        int hh = mi * 60;
        int dd = hh * 24;

        long day = ms / dd;
        long hour = (ms - day * dd) / hh;
        long minute = (ms - day * dd - hour * hh) / mi;
        long second = (ms - day * dd - hour * hh - minute * mi) / ss;
        long milliSecond = ms - day * dd - hour * hh - minute * mi - second * ss;

        string sDay = day < 10 ? "0" + day : "" + day; //天
        string sHour = hour < 10 ? "0" + hour : "" + hour;//小时
        string sMinute = minute < 10 ? "0" + minute : "" + minute;//分钟
        string sSecond = second < 10 ? "0" + second : "" + second;//秒
        string sMilliSecond = milliSecond < 10 ? "0" + milliSecond : "" + milliSecond;//毫秒
        sMilliSecond = milliSecond < 100 ? "0" + sMilliSecond : "" + sMilliSecond;

        return string.Format("{0} 天 {1} 小时 {2} 分 {3} 秒", sDay, sHour, sMinute, sSecond);
    }


    /// <summary>
    /// 转换时间格式为HH:mm:ss
    /// </summary>
    /// <param name="hourMiniteSeconds"></param>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <returns></returns>
    public static bool TryParseHourMiniteSecond(this string hourMiniteSeconds, out int hour, out int minute, out int second)
    {
        hour = 0;
        minute = 0;
        second = 1;
        var dt = DateTime.ParseExact(hourMiniteSeconds, "HH:mm:ss", CultureInfo.InvariantCulture);
        hour = dt.Hour;
        minute = dt.Minute;
        second = dt.Second;
        return true;
    }
}
