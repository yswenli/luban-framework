/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： DBUnitOfWork
*版本号： V1.0.0.0
*唯一标识：4a1f070b-222a-45fb-ae0b-1afb8d41e022
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 18:55:51
*描述：事务和工作单元
*
*=================================================
*修改标记
*修改时间：2023/12/1 18:55:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：事务和工作单元
*
*****************************************************************************/
namespace LuBan.Orm;


/// <summary>
/// 事务和工作单元,
/// 用于支持在LuBan.Web.Core中使用UnitOfWorkAttribute标签实现数据库操作事务
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ISqlSugarClient _sqlSugarScope;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sqlSugarScope"></param>
    public UnitOfWork(ISqlSugarClient sqlSugarScope)
    {
        _sqlSugarScope = sqlSugarScope;
    }

    /// <summary>
    /// 开启工作单元处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="unitOfWork"></param>
    public void BeginTransaction(FilterContext context, UnitOfWorkAttribute unitOfWork)
    {
        try
        {
            _sqlSugarScope.AsTenant().BeginTran();
        }
        catch (Exception ex)
        {
            Logger.Error("UnitOfWork BeginTransaction failed", ex);
            throw;
        }
    }

    /// <summary>
    /// 提交工作单元处理
    /// </summary>
    /// <param name="resultContext"></param>
    /// <param name="unitOfWork"></param>
    public void CommitTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
    {
        try
        {
            _sqlSugarScope.AsTenant().CommitTran();
        }
        catch (Exception ex)
        {
            Logger.Error("UnitOfWork CommitTransaction failed", ex);
            throw;
        }
    }

    /// <summary>
    /// 回滚工作单元处理
    /// </summary>
    /// <param name="resultContext"></param>
    /// <param name="unitOfWork"></param>
    public void RollbackTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
    {
        try
        {
            _sqlSugarScope.AsTenant().RollbackTran();
        }
        catch (Exception ex)
        {
            Logger.Error("UnitOfWork RollbackTransaction failed", ex);
        }
    }

    /// <summary>
    /// 执行完毕（无论成功失败）
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resultContext"></param>
    public void OnCompleted(FilterContext context, FilterContext resultContext)
    {
    }
}
