/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common
*文件名： NumberUtil
*版本号： V1.0.0.0
*唯一标识：3463abaa-3603-4ce0-8b78-fdac894d41dd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 15:52:42
*描述：数字工具类
*
*=================================================
*修改标记
*修改时间：2023/12/5 15:52:42
*修改人： yswenli
*版本号： V1.0.0.0
*描述：数字工具类
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// 数字工具类
    /// </summary>
    public static class NumberUtil
    {

        /// <summary>
        /// 将object转换为long，若失败则返回0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long ParseToLong(this object obj)
        {
            try
            {
                if (obj == null) return 0L;
                var objString = obj.ToString();
                if (string.IsNullOrEmpty(objString)) return 0L;
                if (long.TryParse(objString, out var result))
                {
                    return result;
                }
                return 0L;
            }
            catch
            {
                return 0L;
            }
        }

        /// <summary>
        /// 将object转换为long，若失败则返回指定值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ParseToLong(this string str, long defaultValue = 0)
        {
            try
            {
                return long.Parse(str);
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// 将object转换为double，若失败则返回指定值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double ParseToDouble(this object obj, double defaultValue = 0)
        {
            if (obj == null) return defaultValue;
            var objString = obj.ToString();
            if (objString.IsNullOrEmpty()) return defaultValue;
            try
            {
                if (double.TryParse(objString, out var val))
                {
                    return val;
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
