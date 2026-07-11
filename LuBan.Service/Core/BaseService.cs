/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： BaseService
*版本号： V1.0.0.0
*唯一标识：b3deeffd-31c4-456b-9e1b-dcc3dfffa06e
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/22 11:46:39
*描述：基础业务实现类
*
*=================================================
*修改标记
*修改时间：2022/7/22 11:46:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：基础业务实现类
*
*****************************************************************************/
using Result = LuBan.Common.Data.Result;

namespace System;

/// <summary>
/// 基础业务实现类
/// </summary>
public abstract class BaseService
{
    /// <summary>
    /// 系统初始化时根据配置注入的缓存服务
    /// </summary>
    /// <returns></returns>
    public static IServiceCache ServiceCache
    {
        get
        {
            return ServiceProviderUtil.GetRequiredService<IServiceCache>();
        }
    }

    /// <summary>
    /// 返回成功
    /// </summary>
    /// <returns></returns>
    public Result SuccessResult()
    {
        return new Success();
    }

    /// <summary>
    /// 返回成功
    /// </summary>
    /// <param name="data"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public Result SuccessResult(dynamic data, int code = 200)
    {

        if (data is Result result)
        {
            return result;
        }
        return new Success(data, code);
    }
    /// <summary>
    /// 返回成功
    /// </summary>
    /// <param name="data"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public Result SuccessResult(dynamic data, EnumErrorCode code)
    {
        if (data is Result result)
        {
            return result;
        }
        return new Success(data, code);
    }

    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="errorMsg"></param>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public Result ErrorResult(string errorMsg, Exception ex, int code, params object[] args)
    {
        if (ex != null)
        {
            if (ex is FriendlyException friendlyException)
            {
                return ErrorResult(friendlyException.Message, int.Parse(friendlyException.ErrorCode.ToString()));
            }
            string targetName = "";
            var t = ReflectionUtil.GetCurrentMethodFullName();
            if (t != null)
            {
                targetName = $"{t.Item1}.{t.Item2}";
                Logger.Error(targetName, ex, t.Item3);
            }
            else
            {
                Logger.Error(targetName, ex);
            }
        }
        return new Fail() { Code = code, Result = args, Message = errorMsg };
    }

    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="errorMsg"></param>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public Result ErrorResult(string errorMsg, Exception ex, EnumErrorCode code, params object[] args) => ErrorResult(errorMsg, ex, (int)code, args);

    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public Result ErrorResult(Exception ex, int code = 999, params object[] args)
    {
        return ErrorResult("系统异常，详情请在管理系统中查阅", ex, code, args);
    }

    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="code"></param>
    /// <param name="ex"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public Result ErrorResult(Exception ex, EnumErrorCode code, params object[] args) => ErrorResult(ex, (int)code, args);

    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public Result ErrorResult(Exception ex, params object[] args)
    {
        return ErrorResult("系统异常，详情请在管理系统中查阅", ex, EnumErrorCode.SystemError999, args);
    }


    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="errorMsg"></param>
    /// <param name="code"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public Result ErrorResult(string errorMsg, int code = 999, params object[] args)
    {
        return ErrorResult(errorMsg, new Exception(errorMsg), code, args);
    }
    /// <summary>
    /// 返回失败
    /// </summary>
    /// <param name="errorMsg"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public Result ErrorResult(string errorMsg, params object[] args)
    {
        return ErrorResult(errorMsg, new Exception(errorMsg), 999, args);
    }

    /// <summary>
    /// 返回异常
    /// </summary>
    /// <returns></returns>
    public Result ErrorResult()
    {
        return new Fail();
    }


    /// <summary>
    /// 快捷业务处理
    /// </summary>
    /// <param name="action">要执行的业务逻辑</param>
    /// <returns></returns>
    public Result GetResult(Action action)
    {
        try
        {
            action?.Invoke();
            return SuccessResult();
        }
        catch (Exception ex)
        {
            return ErrorResult(ex);
        }
    }

    /// <summary>
    /// 快捷业务处理，任意值转换为SuccessResult(result)
    /// </summary>
    /// <param name="func">要执行的业务逻辑</param>
    /// <returns></returns>
    public Result GetResult(Func<dynamic> func)
    {
        try
        {
            var result = func();
            if (result == null)
            {
                return SuccessResult();
            }
            else
            {
                return SuccessResult(result);
            }
        }
        catch (Exception ex)
        {
            return ErrorResult(ex);
        }
    }
    /// <summary>
    /// 快捷业务处理，任意值转换为SuccessResult(result)
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public async Task<Result> GetResultAsync(Func<Task<dynamic>> func)
    {
        try
        {
            var result = await func.Invoke();
            if (result == null)
            {
                return SuccessResult();
            }
            else
            {
                return SuccessResult(result);
            }
        }
        catch (Exception ex)
        {
            return ErrorResult(ex);
        }
    }

    /// <summary>
    /// 快捷业务处理，直接返回自定义的Result
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result GetResult(Func<Result> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return ErrorResult(ex);
        }
    }

    /// <summary>
    /// 快捷业务处理，直接返回自定义的Result
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public async Task<Result> GetResultAsync(Func<Task<Result>> func)
    {
        try
        {
            return await func.Invoke();
        }
        catch (Exception ex)
        {
            return ErrorResult(ex);
        }
    }
}

