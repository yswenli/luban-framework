/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Interfaces
*文件名： IDeletedFilter
*版本号： V1.0.0.0
*唯一标识：53d6db29-dc9f-48a4-8d33-78fd0255fa7a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 19:19:58
*描述：假删除接口过滤器
*
*=================================================
*修改标记
*修改时间：2023/12/1 19:19:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：假删除接口过滤器
*
*****************************************************************************/
namespace LuBan.Orm.Interfaces
{
    /// <summary>
    /// 假删除接口过滤器
    /// </summary>
    public interface IDeletedFilter
    {
        /// <summary>
        /// 软删除
        /// </summary>
        bool IsDelete { get; set; }
    }
}
