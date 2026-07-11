/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Models
*文件名： EnumConvertMatchType
*版本号： V1.0.0.0
*唯一标识：17e74fb5-6bc1-404a-8b3a-4eab0550e6ce
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 15:23:32
*描述：转换匹配类型
*
*=================================================
*修改标记
*修改时间：2022/6/21 15:23:32
*修改人： yswenli
*版本号： V1.0.0.0
*描述：转换匹配类型
*
*****************************************************************************/

namespace LuBan.Common.Models
{
    /// <summary>
    /// 转换匹配类型
    /// </summary>
    public enum EnumConvertMatchType : byte
    {
        /// <summary>
        /// 属性名称完全匹配
        /// </summary>
        ExactlyMatch = 0,

        /// <summary>
        /// 属性名称忽略大小写
        /// </summary>
        IgnoreCase = 1,

        /// <summary>
        /// 属性名称包含
        /// </summary>
        Contain = 2,

        /// <summary>
        /// 属性名称包含并忽略大小写
        /// </summary>
        ContainAndIgnoreCase = 3
    }
}
