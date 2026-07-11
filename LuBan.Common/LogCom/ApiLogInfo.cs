/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.LogCom
*文件名： ApiLogInfo
*版本号： V1.0.0.0
*唯一标识：bdf74659-bde1-4da2-84a2-0ab1b61dc98f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/4/8 18:12:33
*描述：
*
*=================================================
*修改标记
*修改时间：2024/4/8 18:12:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.LogCom;


// 使用Json序列化时的对象标记
[JsonObject]
public class ApiLogInfo : LogInfo
{
    /// <summary>
    /// 请求跟踪标识，用于唯一标识一个请求的全流程跟踪
    /// </summary>
    [JsonProperty(Order = 3)]
    public string TraceId { get; set; }

    /// <summary>
    /// 调用方的IP地址
    /// </summary>
    [JsonProperty(Order = 4)]
    public string CallIp { get; set; }

    /// <summary>
    /// 请求的URL地址
    /// </summary>
    [JsonProperty(Order = 5)]
    public string Url { get; set; }

    /// <summary>
    /// 请求的方法，如GET、POST等
    /// </summary>
    [JsonProperty(Order = 6)]
    public string RequestMethod { get; set; }

    /// <summary>
    /// 请求头信息，可记录请求的header内容
    /// </summary>
    [JsonProperty(Order = 7)]
    public string Header { get; set; }

    /// <summary>
    /// 用户代理信息，例如浏览器信息等
    /// </summary>
    [JsonProperty(Order = 7)]
    public string UserAgent { get; set; }

    /// <summary>
    /// 请求的输入数据，通常用于记录请求参数或者请求体
    /// </summary>
    [JsonProperty(Order = 8)]
    public string Input { get; set; }

    /// <summary>
    /// 响应的输出数据，记录返回给客户端的内容
    /// </summary>
    [JsonProperty(Order = 10)]
    public string Output { get; set; }

    /// <summary>
    /// HTTP状态码，记录请求响应的状态
    /// </summary>
    [JsonProperty(Order = 11)]
    public int StatusCode { get; set; }

    /// <summary>
    /// 用户标识，标记操作对应的用户ID
    /// </summary>
    [JsonProperty(Order = 13)]
    public string UserID { get; set; }

    /// <summary>
    /// 请求耗时，以毫秒为单位记录请求处理所花费的时间
    /// </summary>
    [JsonProperty(Order = 14)]
    public long Cost { get; set; }
}
