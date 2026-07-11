/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： BaseRepository
*版本号： V1.0.0.0
*唯一标识：5ecf6fa5-aa2a-4957-8be1-bddf447ca821
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:21:20
*描述：db 实体仓储基类
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:21:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：db 实体仓储基类
*
*****************************************************************************/

namespace LuBan.Orm;

/// <summary>
/// LuBan.Orm 实体仓储基类,
/// 不建议直接使用，建议继承DBRepository，
/// https://www.donet5.com/home/doc?masterId=1&amp;typeId=1228
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class BaseRepository<TEntity> : SimpleClient<TEntity> where TEntity : EntityBase, IDeletedFilter, new()
{
    /// <summary>
    /// _iTenant
    /// </summary>
    protected ITenant _iTenant;

    /// <summary>
    /// db 连接配置
    /// </summary>
    public DbConnectionOptions DbConnectionOptions
    {
        get
        {
            return LuBanOrm.DbConnectionOptions;
        }
    }

    /// <summary>
    /// LuBan.Orm db 实体仓储
    /// </summary>
    /// <param name="tenantIdStr"></param>
    public BaseRepository(string tenantIdStr)
    {
        if (tenantIdStr.IsNullOrEmpty() || tenantIdStr == "0")
        {
            tenantIdStr = LuBanOrmConst.MainConfigId;
        }
        var ormProvider = LuBanOrm.GetProvider<TEntity>(tenantIdStr);
        _iTenant = ormProvider.Tenant;
        Context = ormProvider.Provider;
    }

    /// <summary>
    /// LuBan.Orm db 实体仓储
    /// </summary>
    /// <param name="tenantId"></param>
    public BaseRepository(long tenantId = LuBanOrmConst.DefaultTenantId) : this(tenantId.ToString())
    {

    }

    /// <summary>
    /// 创建单库事务
    /// </summary>
    /// <returns></returns>
    public DbTran CreateTran()
    {
        return new DbTran(AsSugarClient());
    }

    /// <summary>
    /// 创建跨库事务
    /// </summary>
    /// <returns></returns>
    public DbTran CreateMultiTran()
    {
        return new DbTran(_iTenant);
    }

    /// <summary>
    /// 插入数据
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public new async Task<bool> InsertAsync(TEntity t)
    {
        return await AsInsertable(t).ExecuteCommandAsync() > 0;
    }

    /// <summary>
    /// 插入数据
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public new bool Insert(TEntity t)
    {
        return AsInsertable(t).ExecuteCommand() > 0;
    }

    /// <summary>
    /// 插入数据
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public new async Task<TEntity> InsertReturnEntityAsync(TEntity t)
    {
        return await AsInsertable(t).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 插入数据
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public new TEntity InsertReturnEntity(TEntity t)
    {
        return AsInsertable(t).ExecuteReturnEntity();
    }

    /// <summary>
    /// 插入列表数据
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public async Task<int> InsertAsync(List<TEntity> list)
    {
        if (list == null || list.Count == 0) return 0;
        return await AsInsertable(list).ExecuteCommandAsync();
    }

    /// <summary>
    /// 插入列表数据
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public int Insert(List<TEntity> list)
    {
        if (list == null || list.Count == 0) return 0;
        return AsInsertable(list).ExecuteCommand();
    }


    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="t"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(TEntity t, bool ignoreNull = false)
    {
        if (t.Id < 1) throw FriendlyError.Ex("请输入参数id");
        return await AsUpdateable(t).IgnoreColumns(ignoreNull).ExecuteCommandAsync() > 0;
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="t"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public bool Update(TEntity t, bool ignoreNull = false)
    {
        if (t.Id < 1) throw FriendlyError.Ex("请输入参数id");
        return AsUpdateable(t).IgnoreColumns(ignoreNull).ExecuteCommand() > 0;
    }

    /// <summary>
    /// 更新列表
    /// </summary>
    /// <param name="list"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(List<TEntity> list, bool ignoreNull = false)
    {
        if (list == null || list.Count == 0) return 0;
        return await AsUpdateable(list).IgnoreColumns(ignoreNull).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新列表
    /// </summary>
    /// <param name="list"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public int Update(List<TEntity> list, bool ignoreNull = false)
    {
        if (list == null || list.Count == 0) return 0;
        return AsUpdateable(list).IgnoreColumns(ignoreNull).ExecuteCommand();
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="t"></param>
    /// <param name="where"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(TEntity t, Expression<Func<TEntity, bool>> where, bool ignoreNull = false)
    {
        return await AsUpdateable(t).IgnoreColumns(ignoreNull).Where(where).ExecuteCommandAsync() > 0;
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="t"></param>
    /// <param name="where"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public bool Update(TEntity t, Expression<Func<TEntity, bool>> where, bool ignoreNull = false)
    {
        return AsUpdateable(t).IgnoreColumns(ignoreNull).Where(where).ExecuteCommand() > 0;
    }


    /// <summary>
    /// 按指定内容更新,
    /// .UpdateAsync(q => new SysUser() { Remark = "Test" }, q => q.Id == last.Id)
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="where"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> where, bool ignoreNull = false)
    {
        return await AsUpdateable().SetColumns(columns).IgnoreColumns(ignoreNull).Where(where).ExecuteCommandAsync() > 0;
    }

    /// <summary>
    /// 按指定内容更新,
    /// .UpdateAsync(q => new SysUser() { Remark = "Test" }, q => q.Id == last.Id)
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="where"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public bool Update(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> where, bool ignoreNull = false)
    {
        return AsUpdateable().SetColumns(columns, true).IgnoreColumns(ignoreNull).Where(where).ExecuteCommand() > 0;
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public async Task<bool> SaveAsync(TEntity t)
    {
        return await InsertOrUpdateAsync(t);
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Save(TEntity t)
    {
        return InsertOrUpdate(t);
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public async Task<bool> SaveAsync(List<TEntity> list)
    {
        return await InsertOrUpdateAsync(list);
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public bool Save(List<TEntity> list)
    {
        return InsertOrUpdate(list);
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="t"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public async Task<bool> SaveAsync(TEntity t, Expression<Func<TEntity, bool>> where)
    {
        var entity = await GetFirstAsync(where);
        if (entity == null || entity.Id < 1)
        {
            return await InsertAsync(t);
        }
        else
        {
            return await UpdateAsync(t, where);
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="t"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public bool Save(TEntity t, Expression<Func<TEntity, bool>> where)
    {
        var entity = GetFirst(where);
        if (entity == null || entity.Id < 1)
        {
            return Insert(t);
        }
        else
        {
            return Update(t, where);
        }
    }



    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> exp)
    {
        return await AsQueryable().AnyAsync(exp);
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public bool Any(Expression<Func<TEntity, bool>> exp)
    {
        return AsQueryable().Any(exp);
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> exp)
    {
        return await AsQueryable().AnyAsync(exp);
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public bool Exist(Expression<Func<TEntity, bool>> exp)
    {
        return AsQueryable().Any(exp);
    }


    /// <summary>
    /// 执行sql
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<int> ExecuteSqlAsync(string sql, Dictionary<string, object>? parameters = null)
    {
        if (parameters == null)
        {
            return await AsSugarClient().Ado.ExecuteCommandAsync(sql);
        }
        return await AsSugarClient().Ado.ExecuteCommandAsync(sql, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <param name="storedProcedure"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<int> ExecuteSPAsync<Model>(string storedProcedure, Dictionary<string, object>? parameters = null)
    {
        return await AsSugarClient().Ado.UseStoredProcedure().ExecuteCommandAsync(storedProcedure, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 执行sql
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<object> GetScalarBySqlAsync(string sql, Dictionary<string, object>? parameters = null)
    {
        if (parameters == null)
        {
            return await AsSugarClient().Ado.GetScalarAsync(sql);
        }
        return await AsSugarClient().Ado.GetScalarAsync(sql, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 执行sql
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<DataTable> GetDataTableBySqlAsync(string sql, Dictionary<string, object>? parameters = null)
    {
        if (parameters == null)
        {
            return await AsSugarClient().Ado.GetDataTableAsync(sql);
        }
        return await AsSugarClient().Ado.GetDataTableAsync(sql, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 执行sql
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<List<TEntity>> GetListBySqlAsync(string sql, Dictionary<string, object>? parameters = null)
    {
        if (parameters == null)
        {
            return await AsSugarClient().Ado.SqlQueryAsync<TEntity>(sql);
        }
        return await AsSugarClient().Ado.SqlQueryAsync<TEntity>(sql, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 执行sql
    /// </summary>
    /// <typeparam name="Entity"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<List<Entity>> GetListBySqlAsync<Entity>(string sql, Dictionary<string, object>? parameters = null) where Entity : class, new()
    {
        if (parameters == null)
        {
            return await AsSugarClient().Ado.SqlQueryAsync<Entity>(sql);
        }
        return await AsSugarClient().Ado.SqlQueryAsync<Entity>(sql, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <param name="storedProcedure"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<List<Model>> GetListBySPAsync<Model>(string storedProcedure, Dictionary<string, object>? parameters = null)
    {
        return await AsSugarClient().Ado.UseStoredProcedure().SqlQueryAsync<Model>(storedProcedure, parameters.ToSugarParameters());
    }
    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="storedProcedure"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<object> GetScalarBySPAsync(string storedProcedure, Dictionary<string, object>? parameters = null)
    {
        return await AsSugarClient().Ado.UseStoredProcedure().GetScalarAsync(storedProcedure, parameters.ToSugarParameters());
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    public async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> where)
    {
        return await AsQueryable().Where(where).ToListAsync();
    }

    /// <summary>
    /// 获取首个
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where)
    {
        return await AsQueryable().FirstAsync(where);
    }

    /// <summary>
    /// 获取首个
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    public TEntity First(Expression<Func<TEntity, bool>> where)
    {
        return AsQueryable().First(where);
    }

    /// <summary>
    /// 获取首个（不存在返回null）
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    public new async Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> where)
    {
        return await FirstAsync(where);
    }

    /// <summary>
    /// 获取首个（不存在返回null）
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    public new TEntity? GetFirst(Expression<Func<TEntity, bool>> where)
    {
        return First(where);
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <param name="where"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public async Task<List<M>> ListAsync<M>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, M>>? expression = null) where M : class, new()
    {
        if (expression == null)
        {
            var list = await AsQueryable().Where(where).ToListAsync();
            return list.Select(u => u.Adapt<M>()).ToList();
        }
        else
        {
            return await AsQueryable().Where(where).ToListAsync(expression);
        }
    }


    /// <summary>
    /// 读取子级列表
    /// </summary>
    /// <param name="parentIdExpression"></param>
    /// <param name="primaryKeyValue"></param>
    /// <param name="isContainOneself"></param>
    /// <returns></returns>
    public async Task<List<TEntity>> ToChildListAsync(Expression<Func<TEntity, object>> parentIdExpression, object primaryKeyValue, bool isContainOneself = true)
    {
        return await AsQueryable().ToChildListAsync(parentIdExpression, primaryKeyValue, isContainOneself);
    }


    /// <summary>
    /// 读取子级列表
    /// </summary>
    /// <param name="parentIdExpression"></param>
    /// <param name="primaryKeyValue"></param>
    /// <param name="isContainOneself"></param>
    /// <returns></returns>
    public List<TEntity> ToChildList(Expression<Func<TEntity, object>> parentIdExpression, object primaryKeyValue, bool isContainOneself = true)
    {
        return AsQueryable().ToChildList(parentIdExpression, primaryKeyValue, isContainOneself);
    }

    /// <summary>
    /// 获取首个
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <param name="where"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public async Task<Model> FirstAsync<Model>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, Model>>? expression = null) where Model : class, new()
    {
        if (expression == null)
        {
            var entity = await AsQueryable().Where(where).FirstAsync();
            return entity.Adapt<Model>();
        }
        else
        {
            return await AsQueryable().Where(where).Select(expression).FirstAsync();
        }
    }

    /// <summary>
    /// 获取首个
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <param name="where"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public Model First<Model>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, Model>>? expression = null) where Model : class, new()
    {
        if (expression == null)
        {
            var entity = AsQueryable().Where(where).First();
            return entity.Adapt<Model>();
        }
        else
        {
            return AsQueryable().Where(where).Select(expression).First();
        }
    }


    /// <summary>
    /// 条件查询
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression)
    {
        return AsQueryable().Where(expression);
    }

    /// <summary>
    /// 条件查询
    /// </summary>
    /// <param name="isWhere"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity> WhereIF([NotNullWhen(true)] bool isWhere, Expression<Func<TEntity, bool>> expression)
    {
        return AsQueryable().WhereIF(isWhere, expression);
    }

    /// <summary>
    /// 最后一条
    /// </summary>
    /// <returns></returns>
    public TEntity Last(Expression<Func<TEntity, object>> where)
    {
        return AsQueryable().OrderByDescending(where).First();
    }

    /// <summary>
    /// 最后一条
    /// </summary>
    /// <param name="where"></param>
    /// <param name="orderExpression"></param>
    /// <returns></returns>
    public TEntity Last(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, object>> orderExpression)
    {
        return AsQueryable().Where(where).OrderByDescending(orderExpression).First();
    }

    /// <summary>
    /// 最后一条
    /// </summary>
    /// <param name="orderExpression"></param>
    /// <returns></returns>
    public async Task<TEntity> LastAsync(Expression<Func<TEntity, object>> orderExpression)
    {
        return await AsQueryable().OrderByDescending(orderExpression).FirstAsync();
    }

    /// <summary>
    /// 最后一条
    /// </summary>
    /// <param name="where"></param>
    /// <param name="orderExpression"></param>
    /// <returns></returns>
    public async Task<TEntity> LastAsync(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, object>> orderExpression)
    {
        return await AsQueryable().Where(where).OrderByDescending(orderExpression).FirstAsync();
    }


    /// <summary>
    /// 逻辑删除
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public async Task<bool> LogicDeleteAsync(TEntity t)
    {
        if (t.Id < 1) throw FriendlyError.Ex("请输入参数id");
        return await UpdateAsync(q => new TEntity { IsDelete = true }, q => q.Id == t.Id);
    }
    /// <summary>
    /// 逻辑删除
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public async Task<bool> LogicDeleteAsync(Expression<Func<TEntity, bool>> exp)
    {
        return await UpdateAsync(q => new TEntity { IsDelete = true }, exp);
    }
    /// <summary>
    /// 逻辑删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> LogicDeleteByIdAsync(long id)
    {
        if (id < 1) throw FriendlyError.Ex("请输入参数id");
        return await UpdateAsync(q => new TEntity { IsDelete = true }, q => q.Id == id);
    }

    /// <summary>
    /// 逻辑删除
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public async Task<bool> LogicDeleteAsync(List<TEntity> list)
    {
        if (list == null || list.Count < 1) return true;
        return await UpdateAsync(q => new TEntity { IsDelete = true }, q => list.Select(x => x.Id).Contains(q.Id));
    }

    /// <summary>
    /// order by
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> expression, OrderByType type = OrderByType.Asc)
    {
        return AsQueryable().OrderBy(expression, type);
    }

    /// <summary>
    /// 左连接
    /// </summary>
    /// <typeparam name="T2"></typeparam>
    /// <param name="joinExpression"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity, T2> LeftJoin<T2>(Expression<Func<TEntity, T2, bool>> joinExpression)
    {
        return AsQueryable().LeftJoin(joinExpression);
    }
    /// <summary>
    /// 左连接
    /// </summary>
    /// <typeparam name="T2"></typeparam>
    /// <param name="isWhere"></param>
    /// <param name="joinExpression"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity, T2> LeftJoinIF<T2>(bool isWhere, Expression<Func<TEntity, T2, bool>> joinExpression)
    {
        return AsQueryable().LeftJoinIF(isWhere, joinExpression);
    }

    /// <summary>
    /// 右连接
    /// </summary>
    /// <typeparam name="T2"></typeparam>
    /// <param name="joinExpression"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity, T2> RightJoin<T2>(Expression<Func<TEntity, T2, bool>> joinExpression)
    {
        return AsQueryable().RightJoin(joinExpression);
    }

    /// <summary>
    /// 选择（自动填充）,
    /// 建议手动Select(q => new T2() { Id = q.Id.SelectAll() })，选择当前实体的所有字段，其中当前实体变量为q，
    /// https://www.donet5.com/home/Doc?typeId=1186
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public ISugarQueryable<TResult> Select<TResult>()
    {
        return AsQueryable().Select<TResult>();
    }

    /// <summary>
    /// 选择
    /// 手动Select(q => new T2() { Id = q.Id.SelectAll() })，选择当前实体的所有字段，其中当前实体变量为q,
    /// 多表查一表时：Select((o, i, c) => c).ToList();
    /// 多表返回2表：Select((o, i, c) => new{o,i}).ToList();
    /// https://www.donet5.com/home/Doc?typeId=1186
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="expression"></param>
    /// <param name="isAutoFill"></param>
    /// <returns></returns>
    public ISugarQueryable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> expression, bool isAutoFill = true)
    {
        return AsQueryable().Select(expression, isAutoFill);
    }


    /// <summary>
    /// 导航查询；
    /// https://www.donet5.com/home/Doc?typeId=1188
    /// </summary>
    /// <typeparam name="TReturn1"></typeparam>
    /// <param name="include1"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity> Includes<TReturn1>(Expression<Func<TEntity, TReturn1>> include1)
    {
        return AsQueryable().Includes(include1);
    }
    /// <summary>
    /// 导航查询
    /// https://www.donet5.com/home/Doc?typeId=1188
    /// </summary>
    /// <typeparam name="TReturn1"></typeparam>
    /// <typeparam name="TReturn2"></typeparam>
    /// <param name="include1"></param>
    /// <param name="include2"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity> Includes<TReturn1, TReturn2>(Expression<Func<TEntity, List<TReturn1>>> include1, Expression<Func<TReturn1, TReturn2>> include2)
    {
        return AsQueryable().Includes(include1, include2);
    }

    /// <summary>
    /// 导航查询；
    /// https://www.donet5.com/home/Doc?typeId=1188
    /// </summary>
    /// <typeparam name="TReturn1"></typeparam>
    /// <param name="include1"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity> Includes<TReturn1>(Expression<Func<TEntity, List<TReturn1>>> include1)
    {
        return AsQueryable().Includes(include1);
    }


    /// <summary>
    /// 内连接
    /// </summary>
    /// <typeparam name="T2"></typeparam>
    /// <param name="joinExpression"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity, T2> InnerJoin<T2>(Expression<Func<TEntity, T2, bool>> joinExpression)
    {
        return AsQueryable().InnerJoin(joinExpression);
    }

    /// <summary>
    /// 内连接
    /// </summary>
    /// <typeparam name="T2"></typeparam>
    /// <param name="isWhere"></param>
    /// <param name="joinExpression"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity, T2> InnerJoinIF<T2>(bool isWhere, Expression<Func<TEntity, T2, bool>> joinExpression)
    {
        return AsQueryable().InnerJoinIF(isWhere, joinExpression);
    }


    /// <summary>
    /// 全连接
    /// </summary>
    /// <typeparam name="T2"></typeparam>
    /// <param name="joinExpression"></param>
    /// <returns></returns>
    public ISugarQueryable<TEntity, T2> FullJoin<T2>(Expression<Func<TEntity, T2, bool>> joinExpression)
    {
        return AsQueryable().FullJoin(joinExpression);
    }
}