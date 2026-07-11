/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Data
*文件名： Success
*版本号： V1.0.0.0
*唯一标识：3ee401b3-0805-4b61-824a-edc10936458f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/19 11:16:58
*描述：api 返回成功结果
*
*=================================================
*修改标记
*修改时间：2024/12/19 11:16:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：api 返回成功结果
*
*****************************************************************************/
namespace LuBan.Common.Data;


/// <summary>
/// api 返回成功结果
/// </summary>
public sealed class Success : Result
{
    /// <summary>
    /// api 返回成功结果
    /// </summary>
    public Success()
    {
        Code = (int)EnumErrorCode.SystemOk200;
        Message = "OK";
        Type = "Success";
        Time = DateTimeUtil.Now;
    }
    /// <summary>
    /// api 返回成功结果
    /// </summary>
    /// <param name="d"></param>
    public Success(object? d, int code = 200)
    {
        Code = code;
        Result = d;
        Message = "OK";
        Type = "Success";
        Time = DateTimeUtil.Now;
    }
    /// <summary>
    /// api 返回成功结果
    /// </summary>
    /// <param name="d"></param>
    /// <param name="code"></param>
    public Success(object? d, EnumErrorCode code) : this(d, (int)code)
    {

    }
}
/// <summary>
/// api 返回成功结果
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Success<T> : Result<T>
{
    /// <summary>
    /// api 返回成功结果
    /// </summary>
    public Success()
    {
        Code = (int)EnumErrorCode.SystemOk200;
        Message = "OK";
        Type = "Success";
        Time = DateTimeUtil.Now;
    }

    /// <summary>
    /// api 返回成功结果
    /// </summary>
    /// <param name="t"></param>
    /// <param name="code"></param>
    public Success(T t, int code = 200)
    {
        Result = t;
        Code = code;
        Message = "OK";
        Type = "Success";
        Time = DateTimeUtil.Now;
    }
    /// <summary>
    /// api 返回成功结果
    /// </summary>
    /// <param name="t"></param>
    /// <param name="code"></param>
    public Success(T t, EnumErrorCode code) : this(t, (int)code)
    {

    }
}
