/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Service
*文件名： BaseServiceT
*版本号： V1.0.0.0
*唯一标识：852e1c1f-e8dc-45f1-95d1-5dc1be16a07c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/10 10:42:02
*描述：基础业务实现类
*
*=================================================
*修改标记
*修改时间：2025/9/10 10:42:02
*修改人： yswenli
*版本号： V1.0.0.0
*描述：基础业务实现类
*
*****************************************************************************/

namespace System;


/// <summary>
/// 基础业务实现类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseService<T> : BaseSingleInstance<T> where T : new()
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
    /// <returns></returns>
    public async Task<Result> SuccessResultAsync()
    {
        return await Task.FromResult(new Success());
    }

    /// <summary>
    /// 返回成功
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public Result SuccessResult(dynamic data)
    {
        if (data is Result result)
        {
            return result;
        }
        return new Success(data);
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
    /// 返回异常
    /// </summary>
    /// <param name="data"></param>
    /// <param name="msg"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public Result ErrorResult(dynamic data, string msg, int code = 999)
    {
        return new Fail() { Code = code, Result = data, Message = msg };
    }

    /// <summary>
    /// 返回异常
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public Result ErrorResult(string msg, int code = 999)
    {
        return new Fail(msg, code);
    }
    /// <summary>
    /// 返回异常
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public Result ErrorResult(Exception ex, int code = 999)
    {
        return new Fail(ex, code);
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
            action.Invoke();
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
    public async Task<Result> GetResultAsync(Func<dynamic?> func)
    {
        try
        {
            var data = func.Invoke();
            if (data == null)
            {
                return SuccessResult();
            }
            else
            {
                return SuccessResult(await Task.FromResult(data));
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
