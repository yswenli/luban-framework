/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： UnitOfWorkAttribute
*版本号： V1.0.0.0
*唯一标识：15b2aa2c-77a8-4a1d-9364-6edff10e3fc9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 18:56:55
*描述：工作单元配置特性
*
*=================================================
*修改标记
*修改时间：2023/12/1 18:56:55
*修改人： yswenli
*版本号： V1.0.0.0
*描述：工作单元配置特性
*
*****************************************************************************/

using IsolationLevel = System.Transactions.IsolationLevel;

namespace LuBan.Orm.Attributes;


/// <summary>
/// 工作单元配置特性，
/// 保证接口操作的原子性，
/// 底层为自动注入的LuBanOrm的数据库事务
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class UnitOfWorkAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
{
    /// <summary>
    /// 确保事务可用
    /// <para>此方法为了解决静态类方式操作数据库的问题</para>
    /// </summary>
    public bool EnsureTransaction { get; set; } = false;

    /// <summary>
    /// 是否使用分布式环境事务
    /// </summary>
    public bool UseAmbientTransaction { get; set; } = false;

    /// <summary>
    /// 分布式环境事务范围
    /// </summary>
    /// <remarks><see cref="UseAmbientTransaction"/> 为 true 有效</remarks>
    public TransactionScopeOption TransactionScope { get; set; } = TransactionScopeOption.Required;

    /// <summary>
    /// 分布式环境事务隔离级别
    /// </summary>
    /// <remarks><see cref="UseAmbientTransaction"/> 为 true 有效</remarks>
    public IsolationLevel TransactionIsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

    /// <summary>
    /// 分布式环境事务超时时间
    /// </summary>
    /// <remarks>单位秒</remarks>
    public int TransactionTimeout { get; set; } = 0;

    /// <summary>
    /// 支持分布式环境事务异步流
    /// </summary>
    /// <remarks><see cref="UseAmbientTransaction"/> 为 true 有效</remarks>
    public TransactionScopeAsyncFlowOption TransactionScopeAsyncFlow { get; set; } = TransactionScopeAsyncFlowOption.Enabled;

    /// <summary>
    ///  MiniProfiler 分类名
    /// </summary>
    private const string MiniProfilerCategory = "LuBanOrm UnitOfWork";

    /// <summary>
    /// 过滤器排序
    /// </summary>
    private const int FilterOrder = 9999;

    /// <summary>
    /// 排序属性
    /// </summary>
    public int Order => FilterOrder;

    /// <summary>
    /// 构造函数
    /// </summary>
    public UnitOfWorkAttribute()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="ensureTransaction"></param>
    public UnitOfWorkAttribute(bool ensureTransaction)
    {
        EnsureTransaction = ensureTransaction;
    }

    /// <summary>
    /// 拦截请求
    /// </summary>
    /// <param name="context">动作方法上下文</param>
    /// <param name="next">中间件委托</param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 获取动作方法描述器
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var method = actionDescriptor?.MethodInfo;
        if (method == null) return;
        var methodFullName = $"{method.ReflectedType?.FullName ?? ""}.{method.Name}";

        // 创建分布式环境事务
        var transactionScope = CreateTransactionScope(context);

        try
        {
            // 打印工作单元开始消息
            if (UseAmbientTransaction) Logger.Info(MiniProfilerCategory, $"UnitOfWork Transaction Beginning ({methodFullName})", ConsoleColor.Yellow);

            // 开始事务
            BeginTransaction(context, method, out var _unitOfWork, out var unitOfWorkAttribute);

            // 获取执行 Action 结果
            var resultContext = await next();

            // 提交事务
            if (unitOfWorkAttribute != null) CommitTransaction(context, _unitOfWork, unitOfWorkAttribute, resultContext);


            // 提交分布式环境事务
            if (resultContext.Exception == null)
            {
                transactionScope?.Complete();

                // 打印事务提交消息
                if (UseAmbientTransaction) Logger.Info(MiniProfilerCategory, $"LuBanOrm UnitOfWork Transaction Completed ({methodFullName})", ConsoleColor.Yellow);
            }
            else
            {
                // 打印事务回滚消息
                if (UseAmbientTransaction) Logger.Info(MiniProfilerCategory, $"LuBanOrm UnitOfWork Transaction Rollback ({methodFullName})", ConsoleColor.Yellow);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("LuBanOrm UnitOfWork Transaction Failed.", ex);

            // 打印事务回滚消息
            if (UseAmbientTransaction) Logger.Info(MiniProfilerCategory, $"LuBanOrm UnitOfWork Transaction Rollback ({methodFullName})", ConsoleColor.Yellow);

            throw;
        }
        finally
        {
            transactionScope?.Dispose();
        }
    }

    /// <summary>
    /// 创建分布式环境事务
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private TransactionScope? CreateTransactionScope(FilterContext context)
    {
        // 是否启用分布式环境事务
        return UseAmbientTransaction
             ? new TransactionScope(TransactionScope,
            new TransactionOptions { IsolationLevel = TransactionIsolationLevel, Timeout = TransactionTimeout > 0 ? TimeSpan.FromSeconds(TransactionTimeout) : default }
            , TransactionScopeAsyncFlow)
             : default;
    }

    /// <summary>
    /// 开始事务
    /// </summary>
    /// <param name="context"></param>
    /// <param name="method"></param>
    /// <param name="_unitOfWork"></param>
    /// <param name="unitOfWorkAttribute"></param>
    private static void BeginTransaction(FilterContext context, MethodInfo method, out IUnitOfWork _unitOfWork, out UnitOfWorkAttribute? unitOfWorkAttribute)
    {
        // 解析工作单元服务
        _unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        // 获取工作单元特性
        unitOfWorkAttribute = method.GetCustomAttribute<UnitOfWorkAttribute>();
        if (unitOfWorkAttribute != null)
        {
            // 调用开启事务方法
            _unitOfWork.BeginTransaction(context, unitOfWorkAttribute);
            // 打印工作单元开始消息
            if (!unitOfWorkAttribute.UseAmbientTransaction) Logger.Info(MiniProfilerCategory, "LuBanOrm UnitOfWork Transaction Beginning", ConsoleColor.Yellow);
        }
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    /// <param name="context"></param>
    /// <param name="_unitOfWork"></param>
    /// <param name="unitOfWorkAttribute"></param>
    /// <param name="resultContext"></param>
    private static void CommitTransaction(FilterContext context, IUnitOfWork _unitOfWork, UnitOfWorkAttribute unitOfWorkAttribute, FilterContext resultContext)
    {
        // 获取动态结果上下文
        dynamic dynamicResultContext = resultContext;

        if (dynamicResultContext.Exception == null)
        {
            try
            {
                // 调用提交事务方法
                _unitOfWork.CommitTransaction(resultContext, unitOfWorkAttribute);
            }
            catch (Exception commitEx)
            {
                Logger.Error("LuBanOrm UnitOfWork CommitTransaction failed, rolling back.", commitEx);
                // 提交失败时回滚事务，避免事务悬挂在连接上
                try
                {
                    _unitOfWork.RollbackTransaction(resultContext, unitOfWorkAttribute);
                }
                catch (Exception rollbackEx)
                {
                    Logger.Error("LuBanOrm UnitOfWork RollbackTransaction after commit failure failed.", rollbackEx);
                }
                throw;
            }
        }
        else
        {
            // 调用回滚事务方法
            _unitOfWork.RollbackTransaction(resultContext, unitOfWorkAttribute);
        }

        // 调用执行完毕方法
        _unitOfWork.OnCompleted(context, resultContext);

        // 打印工作单元结束消息
        if (!unitOfWorkAttribute.UseAmbientTransaction) Logger.Info(MiniProfilerCategory, "LuBanOrm UnitOfWork Transaction Ending", ConsoleColor.Yellow);
    }
}


