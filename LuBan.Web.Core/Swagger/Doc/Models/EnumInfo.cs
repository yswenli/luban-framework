/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger.Doc.Models
*文件名： EnumInfo
*版本号： V1.0.0.0
*唯一标识：aad4d09b-6f70-4d36-913c-b085026cc19f
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 11:22:58
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 11:22:58
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Web.Core.Swagger.Doc.Models
{
    /// <summary>
    /// 枚举信息
    /// </summary>
    public class EnumInfo
    {
        /// <summary>
        /// 枚举类型
        /// </summary>
        public string 枚举名称 { get; set; }
        /// <summary>
        /// 枚举值类型
        /// </summary>
        public string 枚举类型 { get; set; }
        /// <summary>
        /// 枚举值
        /// </summary>
        public int[] 枚举范围 { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string 枚举描述 { get; set; }
    }
}
