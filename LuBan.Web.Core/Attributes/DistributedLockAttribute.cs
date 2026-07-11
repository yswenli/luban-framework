/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Attributes
*文件名： DistributedLockAtrribute
*版本号： V1.0.0.0
*唯一标识：c503d03e-c8e5-4b04-90c4-520057988ce9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/15 16:01:10
*描述：分布式锁
*
*=================================================
*修改标记
*修改时间：2025/9/15 16:01:10
*修改人： yswenli
*版本号： V1.0.0.0
*描述：分布式锁
*
*****************************************************************************/
using LuBan.Redis.Interfaces;

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 分布式锁
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DistributedLockAttribute : BaseFilterAttribute
{
    private IDistributedLock _distributedLock;
    private DistributedLockToken? _token;

    int _dbIndex = 0;

    /// <summary>
    /// 锁名称
    /// </summary>
    public string LockName { get; set; }
    /// <summary>
    /// 超时时间
    /// </summary>
    public int Timeout { get; set; } = 10000;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// 变量类型
    /// </summary>
    public EnumVariableType VariableType { get; set; } = EnumVariableType.None;

    /// <summary>
    /// 变量名称
    /// </summary>
    public string VariableName { get; set; } = string.Empty;

    /// <summary>
    /// 分布式锁
    /// </summary>
    /// <param name="lockName"></param>
    /// <param name="variableType"></param>
    /// <param name="variableName"></param>
    /// <param name="timeout"></param>
    /// <param name="maxRetries"></param>
    /// <param name="dbIndex"></param>
    public DistributedLockAttribute(string lockName, EnumVariableType variableType = EnumVariableType.None, string variableName = "", int timeout = 10000, int maxRetries = 5, int dbIndex = 0)
    {
        LockName = lockName;
        VariableType = variableType;
        VariableName = variableName;
        Timeout = timeout;
        MaxRetries = maxRetries;
        _dbIndex = dbIndex;
        Order = 2;
        if (variableType != EnumVariableType.None && variableName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(variableName));
    }

    /// <summary>
    /// 执行业务前
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var lockKey = LockName;
        switch (VariableType)
        {
            case EnumVariableType.None:
                break;
            case EnumVariableType.Query:
                lockKey = $"{LockName}_{context.HttpContext.Request.Query[VariableName]}";
                break;
            case EnumVariableType.Form:
                lockKey = $"{LockName}_{context.HttpContext.Request.Form[VariableName]}";
                break;
            case EnumVariableType.Route:
                lockKey = $"{LockName}_{context.RouteData.Values[VariableName]}";
                break;
            case EnumVariableType.Header:
                lockKey = $"{LockName}_{context.HttpContext.Request.Headers[VariableName]}";
                break;
            case EnumVariableType.Cookie:
                lockKey = $"{LockName}_{context.HttpContext.Request.Cookies[VariableName]}";
                break;
            case EnumVariableType.ContextUser:
                lockKey = $"{LockName}_{SessionUser.UserId}";
                break;
        }
        _distributedLock = LuBanRedis.Instance.GetDistributedLock(lockKey, Timeout, _dbIndex, "1");

        if (Timeout > 0 && MaxRetries > 0)
        {
            var retryTime = Timeout / MaxRetries;
            if (retryTime < 50) retryTime = 50;
            _token = await _distributedLock.AcquireAsync(TimeSpan.FromMilliseconds(Timeout), TimeSpan.FromMilliseconds(Timeout / MaxRetries));
        }
        else
        {
            _token = await _distributedLock.AcquireAsync();
        }
        if (_token == null)
        {
            context.Result = new ConflictResult();
            return;
        }
        await next.Invoke();
    }


    /// <summary>
    /// 执行业务后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        try
        {
            await next.Invoke();
        }
        finally
        {
            if (_token != null)
            {
                await _token.DisposeAsync();
            }
        }
    }
}
