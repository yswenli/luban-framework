/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger.Doc.Models
*文件名： BaseModelInfo
*版本号： V1.0.0.0
*唯一标识：1dd91843-a83d-48d8-988f-a61656e4540a
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 11:22:35
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 11:22:35
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Web.Core.Swagger.Doc.Models
{
    /// <summary>
    /// BaseModelInfo
    /// </summary>
    public abstract class BaseModelInfo
    {
        /// <summary>
        /// 参数类型
        /// </summary>
        public object 参数类型 { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string 描述 { get; set; }
        /// <summary>
        /// 可空类型
        /// </summary>
        public bool 可空类型 { get; set; }
    }
}
