/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common.IPToRegion
*文件名： Header
*版本号： V1.0.0.0
*唯一标识：d13f644f-0058-4283-a703-9924e2b93cf2
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/10/19 9:47:39
*描述：Header
*
*=====================================================================
*修改标记
*修改时间：2021/10/19 9:47:39
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：Header
*
*****************************************************************************/

namespace LuBan.Common.IPToRegion
{
    /// <summary>
    /// 头部
    /// </summary>
    public class Header
    {
        /// <summary>
        /// 版本
        /// </summary>
        public int Version { get; }
        /// <summary>
        /// 索引
        /// </summary>
        public int IndexPolicy { get; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public int CreatedAt { get; }
        /// <summary>
        /// 开始
        /// </summary>
        public int StartIndexPtr { get; }
        /// <summary>
        /// 结束
        /// </summary>
        public int EndIndexPtr { get; }
        /// <summary>
        /// 内容
        /// </summary>
        public byte[] Buffer { get; }
        /// <summary>
        /// 头部
        /// </summary>
        /// <param name="buff"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Header(byte[] buff)
        {
            if (buff == null) throw new ArgumentNullException(nameof(buff));
            if (buff.Length < 16) throw new ArgumentOutOfRangeException(nameof(buff));
            Version = Searcher.GetInt2(buff, 0);
            IndexPolicy = Searcher.GetInt2(buff, 2);
            CreatedAt = Searcher.GetInt(buff, 4);
            StartIndexPtr = Searcher.GetInt(buff, 8);
            EndIndexPtr = Searcher.GetInt(buff, 12);
            Buffer = buff;

        }
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{" +
            "Version: " + Version + ',' +
            "IndexPolicy: " + IndexPolicy + ',' +
            "CreatedAt: " + CreatedAt + ',' +
            "StartIndexPtr: " + StartIndexPtr + ',' +
            "EndIndexPtr: " + EndIndexPtr +
        '}';
        }
    }
}
