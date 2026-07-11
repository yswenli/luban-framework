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
/// 事务
/// </summary>
public class DbTran : IDisposable
{
    ITenant _tenant;

    ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 事务
    /// </summary>
    /// <param name="tenant"></param>
    public DbTran(ITenant tenant)
    {
        _tenant = tenant;
        _tenant.BeginTran();
    }

    /// <summary>
    /// 事务
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public DbTran(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
        _sqlSugarClient.Ado.BeginTran();
    }


    /// <summary>
    /// 释放资源时提交或回退
    /// </summary>
    public void Dispose()
    {
        if (_tenant != null)
        {
            try
            {
                _tenant.CommitTran();
            }
            catch
            {
                _tenant.RollbackTran();
                throw;
            }
        }
        if (_sqlSugarClient != null)
        {
            try
            {
                _sqlSugarClient.Ado.CommitTran();
            }
            catch
            {
                _sqlSugarClient.Ado.RollbackTran();
                throw;
            }
        }
    }
}
