/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.LogCom
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

    /// <summary>
    /// 日志创建时间，默认取 <c>DateTimeUtil.Now</c>（JSON 字段 created）。
    /// </summary>
    [JsonPropertyName("created")]
    [JsonPropertyOrder(0)]
    public DateTime Created { get; set; } = DateTimeUtil.Now;


    /// <summary>
    /// 服务名，默认取启动时缓存的配置值（JSON 字段 serviceName）。
    /// </summary>
    [JsonPropertyName("serviceName")]
    [JsonPropertyOrder(0)]
    public string ServiceName { get; set; } = _cachedServiceName;


    /// <summary>
    /// 日志级别数值，默认 0（JSON 字段 level）。
    /// </summary>
    [JsonPropertyName("level")]
    [JsonPropertyOrder(1)]
    public int Level { get; set; } = 0;


    /// <summary>
    /// 日志描述文本（JSON 字段 description）。
    /// </summary>
    [JsonPropertyName("description")]
    [JsonPropertyOrder(2)]
    public string Description { get; set; }

    /// <summary>
    /// 日志附带的参数数组（JSON 字段 params）。
    /// </summary>
    [JsonPropertyName("params")]
    [JsonPropertyOrder(9)]
    public object[] Params { get; set; }

    /// <summary>
    /// 关联的异常对象，无异常时为 null（JSON 字段 exception）。
    /// </summary>
    [JsonPropertyName("exception")]
    [JsonPropertyOrder(12)]
    public Exception? Exception { get; set; }

    /// <summary>
    /// 是否输出调试信息，不参与 JSON 序列化。
    /// </summary>
    [JsonIgnore]
    public bool EnableDebug { get; set; }
}
