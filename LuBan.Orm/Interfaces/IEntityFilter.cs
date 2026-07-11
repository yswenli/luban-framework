/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Interfaces
*文件名： IEntityFilter
*版本号： V1.0.0.0
*唯一标识：9f58fcbb-9dbe-4930-abb6-d82bc4908d89
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/7 19:17:48
*描述：自定义实体过滤器接口
*
*=================================================
*修改标记
*修改时间：2023/12/7 19:17:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：自定义实体过滤器接口
*
*****************************************************************************/
namespace LuBan.Orm.Interfaces
{

    /// <summary>
    /// 自定义实体过滤器接口
    /// </summary>
    public interface IEntityFilter
    {
        /// <summary>
        /// 实体过滤器
        /// </summary>
        /// <returns></returns>
        IEnumerable<TableFilterItem<object>> AddEntityFilter();
    }
}
