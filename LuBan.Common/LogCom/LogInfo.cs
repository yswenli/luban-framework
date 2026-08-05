/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Logs
*文件名： LogInfo
*版本号： V1.0.0.0
*唯一标识：1bcf4493-7645-4017-b350-371f8071eea0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/4/8 18:10:22
*描述：日志信息
*
*=================================================
*修改标记
*修改时间：2024/4/8 18:10:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志信息
*
*****************************************************************************/
namespace LuBan.Common.LogCom;

/// <summary>
/// 日志信息
/// </summary>
public class LogInfo
{
    /// <summary>
    /// 服务名在运行期不变，缓存为静态字段避免每条日志都查配置。
    /// </summary>
    private static readonly string _cachedServiceName = ConfigUtil.GetServiceName();

    [JsonPropertyName("created")]
    [JsonPropertyOrder(0)]
    public DateTime Created { get; set; } = DateTimeUtil.Now;


    [JsonPropertyName("serviceName")]
    [JsonPropertyOrder(0)]
    public string ServiceName { get; set; } = _cachedServiceName;


    [JsonPropertyName("level")]
    [JsonPropertyOrder(1)]
    public int Level { get; set; } = 0;


    [JsonPropertyName("description")]
    [JsonPropertyOrder(2)]
    public string Description { get; set; }

    [JsonPropertyName("params")]
    [JsonPropertyOrder(9)]
    public object[] Params { get; set; }

    [JsonPropertyName("exception")]
    [JsonPropertyOrder(12)]
    public Exception? Exception { get; set; }

    [JsonIgnore]
    public bool EnableDebug { get; set; }
}
