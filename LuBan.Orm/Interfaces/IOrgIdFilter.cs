/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Interfaces
*文件名： IOrgIdFilter
*版本号： V1.0.0.0
*唯一标识：11cfeac3-f0f0-498c-9228-b47303c58aed
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 19:21:17
*描述：机构Id接口过滤器
*
*=================================================
*修改标记
*修改时间：2023/12/1 19:21:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：机构Id接口过滤器
*
*****************************************************************************/
namespace LuBan.Orm.Interfaces
{

    /// <summary>
    /// 机构Id接口过滤器
    /// </summary>
    public interface IOrgIdFilter
    {
        /// <summary>
        /// 创建者部门Id
        /// </summary>
        long? CreateOrgId { get; set; }
    }
}
