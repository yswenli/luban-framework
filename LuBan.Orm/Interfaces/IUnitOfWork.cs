/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Interfaces
*文件名： IUnitOfWork
*版本号： V1.0.0.0
*唯一标识：71b4ef38-1a12-4f08-a844-e46261fbc7f3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/10 17:44:27
*描述：工作单元依赖接口
*
*=================================================
*修改标记
*修改时间：2025/9/10 17:44:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：工作单元依赖接口
*
*****************************************************************************/
namespace LuBan.Orm.Interfaces;



/// <summary>
/// 工作单元依赖接口
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 开启工作单元处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="unitOfWork"></param>
    void BeginTransaction(FilterContext context, UnitOfWorkAttribute unitOfWork);

    /// <summary>
    /// 提交工作单元处理
    /// </summary>
    /// <param name="resultContext"></param>
    /// <param name="unitOfWork"></param>
    void CommitTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork);

    /// <summary>
    /// 回滚工作单元处理
    /// </summary>
    /// <param name="resultContext"></param>
    /// <param name="unitOfWork"></param>
    void RollbackTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork);

    /// <summary>
    /// 执行完毕（无论成功失败）
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resultContext"></param>
    void OnCompleted(FilterContext context, FilterContext resultContext);
}
