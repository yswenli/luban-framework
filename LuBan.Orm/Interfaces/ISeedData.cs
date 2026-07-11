/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Interfaces
*文件名： ISeedData
*版本号： V1.0.0.0
*唯一标识：0709e310-d832-47be-b909-1d4b1dadb956
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 11:23:02
*描述：实体种子数据接口
*
*=================================================
*修改标记
*修改时间：2023/12/4 11:23:02
*修改人： yswenli
*版本号： V1.0.0.0
*描述：实体种子数据接口
*
*****************************************************************************/
namespace LuBan.Orm.Interfaces;

/// <summary>
/// 实体种子数据接口
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ISeedData<TEntity>
    where TEntity : class, new()
{
    /// <summary>
    /// 初始化种子数据
    /// </summary>
    /// <returns></returns>
    IEnumerable<TEntity> InitData();
}
