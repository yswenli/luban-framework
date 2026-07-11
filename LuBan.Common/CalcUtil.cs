/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： CalcUtil
*版本号： V1.0.0.0
*唯一标识：388a8fd5-2e78-452a-8b02-b3e6cad68e55
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/1/26 15:41:22
*描述：计算工具类
*
*=====================================================================
*修改标记
*修改时间：2022/1/26 15:41:22
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：计算工具类
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// 计算工具类
    /// </summary>
    public static class CalcUtil
    {
        /// <summary>
        /// 获取组合
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="n">最大位数组合</param>
        /// <returns></returns>
        public static List<T[]>? GetCombination<T>(T[] data, int n = 0)
        {
            if (data == null || data.Length < 1) return null;

            var list = new List<T[]>();

            if (n < 1 || n >= data.Length)
            {
                foreach (var s in data)
                {
                    var lst = list.GetRange(0, list.Count);

                    var nArr = new T[] { s };

                    list.Add(nArr);

                    foreach (T[] ss in lst)
                    {
                        list.Add(ss.Concat(nArr).ToArray());
                    }
                }
            }
            else
            {
                foreach (var s in data)
                {
                    var lst = list.Where(q => q.Length < n).ToList();

                    var nArr = new T[] { s };

                    list.Add(nArr);

                    foreach (T[] ss in lst)
                    {
                        list.Add(ss.Concat(nArr).ToArray());
                    }
                }
            }
            return list.OrderByDescending(p => p.Length).ToList();
        }


    }
}
