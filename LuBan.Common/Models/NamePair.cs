/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Models
*文件名： NamePair
*版本号： V1.0.0.0
*唯一标识：4ca568cf-5261-4c81-864b-794e4cbc25b0
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 14:36:35
*描述：
*
*=================================================
*修改标记
*修改时间：2022/6/21 14:36:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.Models
{
    /// <summary>
    /// 名称对
    /// </summary>
    public class NamePair
    {
        /// <summary>
        /// 源名称
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// 目标名称
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// 名称对
        /// </summary>
        public NamePair() { }

        /// <summary>
        /// 名称对
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="targetName"></param>
        public NamePair(string sourceName, string targetName)
        {
            SourceName = sourceName;
            TargetName = targetName;
        }
    }

    /// <summary>
    /// 名称对集合
    /// </summary>
    public class NamePairCollection : IEnumerable<NamePair>, IDisposable
    {
        List<NamePair> _namePairs;

        /// <summary>
        /// 名称对集合
        /// </summary>
        public NamePairCollection()
        {
            _namePairs = new List<NamePair>();
        }

        /// <summary>
        /// 名称对集合
        /// </summary>
        /// <param name="namePairs"></param>
        public NamePairCollection(params NamePair[] namePairs) : this()
        {
            if (namePairs != null)
            {
                foreach (var namePair in namePairs)
                {
                    _namePairs.Add(namePair);
                }
            }
        }

        /// <summary>
        /// 名称对集合
        /// </summary>
        /// <param name="namePairs"></param>
        public NamePairCollection(IEnumerable<NamePair> namePairs) : this()
        {
            if (namePairs != null)
            {
                foreach (var namePair in namePairs)
                {
                    _namePairs.Add(namePair);
                }
            }
        }

        /// <summary>
        /// 添加名称对
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="targetName"></param>
        public void Add(string sourceName, string targetName)
        {
            Add(new NamePair(sourceName, targetName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namePair"></param>
        public void Add(NamePair namePair)
        {
            _namePairs.Add(namePair);
        }

        /// <summary>
        /// 获取目标名
        /// </summary>
        /// <param name="sourceName"></param>
        /// <returns></returns>
        public string GetTargetName(string sourceName)
        {
            if (!_namePairs.Any()) return string.Empty;
            return _namePairs.FirstOrDefault(b => b.SourceName == sourceName)?.TargetName ?? "";
        }

        /// <summary>
        /// 获取源名称
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public string GetSourceName(string targetName)
        {
            if (!_namePairs.Any()) return string.Empty;
            return _namePairs.FirstOrDefault(b => b.TargetName == targetName)?.SourceName ?? "";
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<NamePair> GetEnumerator()
        {
            return _namePairs.GetEnumerator();
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _namePairs.Clear();
        }
    }
}


