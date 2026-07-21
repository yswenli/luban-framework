/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm
*文件名： DbTran
*版本号： V1.0.0.0
*唯一标识：8316dd44-4876-4734-9c7d-fe51f95e4544
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/20 18:36:19
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/20 18:36:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Orm;

/// <summary>
/// 事务，
/// 使用using包裹，需显式调用Commit()提交，Dispose时未Commit则自动回滚
/// </summary>
public class DbTran : IDisposable
{
    ITenant? _tenant;

    ISqlSugarClient? _sqlSugarClient;

    bool _committed;

    bool _disposed;

    /// <summary>
    /// 事务
    /// </summary>
    /// <param name="tenant"></param>
    public DbTran(ITenant tenant)
    {
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        _tenant.BeginTran();
    }

    /// <summary>
    /// 事务
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public DbTran(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient ?? throw new ArgumentNullException(nameof(sqlSugarClient));
        _sqlSugarClient.Ado.BeginTran();
    }

    /// <summary>
    /// 显式提交事务，
    /// 必须在Dispose之前调用，否则Dispose时自动回滚
    /// </summary>
    public void Commit()
    {
        //注意：_tenant 与 _sqlSugarClient 由两个互斥构造函数保证仅其一非空，
        //若未来允许同时非空，此处需处理部分提交问题
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_committed) return;
        if (_tenant != null)
        {
            _tenant.CommitTran();
        }
        if (_sqlSugarClient != null)
        {
            _sqlSugarClient.Ado.CommitTran();
        }
        _committed = true;
    }

    /// <summary>
    /// 释放资源时，未Commit则自动回滚
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (!_committed)
        {
            if (_tenant != null)
            {
                try { _tenant.RollbackTran(); }
                catch (Exception ex) { Logger.Error("DbTran rollback failed", ex); }
            }
            if (_sqlSugarClient != null)
            {
                try { _sqlSugarClient.Ado.RollbackTran(); }
                catch (Exception ex) { Logger.Error("DbTran rollback failed", ex); }
            }
        }
        _tenant = null;
        _sqlSugarClient = null;
    }
}
