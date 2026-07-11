/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： GuidUtil
*版本号： V1.0.0.0
*唯一标识：6583a0bd-b849-4828-a7d7-919ad29af68a
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 15:18:53
*描述：Guid相关操作
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 15:18:53
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：Guid相关操作
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// Guid相关操作
    /// </summary>
    public static class GuidUtil
    {
        /// <summary>
        /// 获取新guid字符串，不含有 '-'
        /// </summary>
        public static string GuidString
        {
            get { return Guid.NewGuid().ToString("N"); }
        }

        /// <summary>
        /// 获取新guid字符串，不含有 '-'
        /// </summary>
        public static string New
        {
            get
            {
                return GuidString;
            }
        }

        /// <summary>
        /// 将字符串(不含有'-')转成Guid
        /// </summary>
        /// <param name="guidStr"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Guid ConvertToGuid(string guidStr, string format = "N")
        {
            return Guid.ParseExact(guidStr, format);
        }

        /// <summary>
        /// 将guid字符串(不含有'-')转成数字
        /// </summary>
        /// <param name="guidStr"></param>
        /// <returns></returns>
        public static ulong ConvertToLong(string guidStr)
        {
            var guid = ConvertToGuid(guidStr);
            return ConvertToLong(guid);
        }

        /// <summary>
        /// 将GUID转换成为ulong
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static ulong ConvertToLong(Guid guid)
        {
            var buffer = guid.ToByteArray();
            return BitConverter.ToUInt64(buffer, 0);
        }

        /// <summary>
        /// 将数字转成guid字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ConvertToStr(ulong data)
        {
            var buffer = BitConverter.GetBytes(data);
            return BitConverter.ToString(buffer);
        }

        /// <summary>
        /// 获取长整形字符串
        /// </summary>
        /// <returns></returns>
        public static ulong GetGuidToLong()
        {
            return ConvertToLong(Guid.NewGuid());
        }

        /// <summary>
        /// 获取字符串型Ulong类型GUID
        /// </summary>
        /// <returns></returns>
        public static string GetGuidToLongStr()
        {
            return GetGuidToLong().ToString();
        }

        /// <summary>
        /// 获取字符串型Ulong类型GUID
        /// </summary>
        /// <returns></returns>
        public static string ConvertGuidToLongStr(string guid)
        {
            return ConvertToLong(guid).ToString();
        }
    }
}
