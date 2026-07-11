/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Data
*文件名： JsonData
*版本号： V1.0.0.0
*唯一标识：bf80f7c4-4f86-4784-90b9-babc5c6a8dd5
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 13:44:38
*描述：api 返回结果
*
*=================================================
*修改标记
*修改时间：2022/6/21 13:44:38
*修改人： yswenli
*版本号： V1.0.0.0
*描述：api 返回结果
*
*****************************************************************************/
namespace LuBan.Common.Data;

/// <summary>
/// api 返回结果
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ApiResult<T>
{
    /// <summary>
    /// 错误码枚举类,
    /// 200 表示成功
    /// 其他表示失败
    /// </summary>
    [JsonProperty(propertyName: "code")]
    public int Code { get; set; } = 200;

    /// <summary>
    /// 类型
    /// </summary>
    [JsonProperty(propertyName: "type")]
    public string Type { get; set; }

    /// <summary>
    /// 异常消息
    /// </summary>
    [JsonProperty(propertyName: "message")]
    public string Message { get; set; }

    /// <summary>
    /// 泛型结果
    /// </summary>
    [JsonProperty(propertyName: "result")]
    public T? Result { get; set; }
    /// <summary>
    /// 扩展内容
    /// </summary>
    [JsonProperty(propertyName: "extras")]
    public dynamic Extras { get; set; }
    /// <summary>
    /// 时间
    /// </summary>
    [JsonProperty(propertyName: "time")]
    public DateTime Time { get; set; }

    /// <summary>
    /// api 返回结果
    /// </summary>
    public ApiResult()
    {
        Message = string.Empty;
        Result = default;
        Time = DateTimeUtil.Now;
    }
}

/// <summary>
/// api 返回结果
/// </summary>
public abstract class Result : ApiResult<dynamic>
{

}

/// <summary>
/// api 返回结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T> : ApiResult<T>
{
    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator Result(Result<T> result)
    {
        if (result.Code == 200)
        {
            return new Success(result.Result);
        }
        return new Fail(result.Message, result.Code);
    }
}


