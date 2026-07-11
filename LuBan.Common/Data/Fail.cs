/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Data
*文件名： Fail
*版本号： V1.0.0.0
*唯一标识：6c0a2e77-c98a-4720-a91f-239a6c69e094
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/19 11:17:32
*描述：api 返回失败结果
*
*=================================================
*修改标记
*修改时间：2024/12/19 11:17:32
*修改人： yswenli
*版本号： V1.0.0.0
*描述：api 返回失败结果
*
*****************************************************************************/
namespace LuBan.Common.Data;


/// <summary>
/// api 返回失败结果
/// </summary>
public sealed class Fail : Result
{
    /// <summary>
    /// api 返回失败结果
    /// </summary>
    public Fail()
    {
        Message = "Server API error, please contact administrator support to resolve this issue.";
        Code = (int)EnumErrorCode.SystemError999;
        Type = "Fail";
        Time = DateTimeUtil.Now;
    }

    /// <summary>
    /// api 返回失败结果
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    public Fail(Exception? ex, int code = 999)
    {
        Code = code;
        if (ex == null)
            Message = "";
        else
        {
            try
            {
                if (ex is FriendlyException fe)
                {
                    Message = $"{fe.ErrorMessage?.ToString() ?? ""}";
                }
                else
                {
                    Message = "Server API error, please contact administrator support to resolve this issue.";
                    Logger.Error(ex);
                }
            }
            catch { }
        }

        Type = "Fail";
        Time = DateTimeUtil.Now;
    }
    /// <summary>
    /// api 返回失败结果
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    public Fail(Exception? ex, EnumErrorCode code) : this(ex, (int)code)
    {

    }
    /// <summary>
    /// api 返回失败结果
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="code"></param>
    public Fail(string msg, int code = 999)
    {
        Message = msg;
        Code = code;
        Type = "Fail";
        Time = DateTimeUtil.Now;
    }

    /// <summary>
    /// api 返回失败结果
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="code"></param>
    public Fail(string msg, EnumErrorCode code) : this(msg, (int)code)
    {

    }
}

/// <summary>
/// api 返回失败结果
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Fail<T> : Result<T>
{
    /// <summary>
    /// api 返回失败结果
    /// </summary>
    public Fail()
    {
        Message = "Server API error, please contact administrator support to resolve this issue.";
        Code = (int)EnumErrorCode.SystemError999;
        Type = "Fail";
        Time = DateTimeUtil.Now;
    }
    /// <summary>
    /// api 返回失败结果
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    public Fail(Exception? ex, int code = 999)
    {
        Code = code;
        if (ex == null)
            Message = "Server API error, please contact administrator support to resolve this issue.";
        else
            Message = ex.ToString();
        Type = "Fail";
        Time = DateTimeUtil.Now;
    }
    /// <summary>
    /// api 返回失败结果
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    public Fail(Exception? ex, EnumErrorCode code) : this(ex, (int)code)
    {

    }
    /// <summary>
    /// api 返回失败结果
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="code"></param>
    public Fail(string msg, int code = 999)
    {
        Message = msg;
        Code = code;
        Type = "Fail";
        Time = DateTimeUtil.Now;
    }

    /// <summary>
    /// api 返回失败结果
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="code"></param>
    public Fail(string msg, EnumErrorCode code) : this(msg, (int)code)
    {

    }
}
