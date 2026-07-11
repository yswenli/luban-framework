/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Models
*文件名： EnumInfo
*版本号： V1.0.0.0
*唯一标识：fa8bee9f-1074-4517-a950-2a8f15c4f7d6
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 14:40:40
*描述：枚举工具类所用实体
*
*=================================================
*修改标记
*修改时间：2022/6/21 14:40:40
*修改人： yswenli
*版本号： V1.0.0.0
*描述：枚举工具类所用实体
*
*****************************************************************************/

namespace LuBan.Common.Models
{
    /// <summary>
    /// 枚举工具类所用实体
    /// </summary>
    public class EnumInfo
    {
        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 枚举值字符串
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}
