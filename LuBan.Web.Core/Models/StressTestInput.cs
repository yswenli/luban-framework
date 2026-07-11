/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： StressTestInput
*版本号： V1.0.0.0
*唯一标识：4e6984dd-04e7-406c-b749-f3ca60db01b8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/23 13:39:23
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/23 13:39:23
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;


/// <summary>
/// 接口压测输入参数
/// </summary>
public class StressTestInput
{
    /// <summary>
    /// 接口请求地址
    /// </summary>
    /// <example>https://gitee.com/yswenli</example>
    [Required(ErrorMessage = "接口请求地址不能为空")]
    public string RequestUri { get; set; }

    /// <summary>
    /// 请求方式
    /// </summary>
    [Required(ErrorMessage = "请求方式不能为空")]
    public string RequestMethod { get; set; } = nameof(HttpMethod.Get);

    /// <summary>
    /// 每轮请求量
    /// </summary>
    /// <example>100</example>
    [Required(ErrorMessage = "每轮请求量不能为空")]
    [Range(1, 100000, ErrorMessage = "每轮请求量必须为1-100000")]
    public int? NumberOfRequests { get; set; }

    /// <summary>
    /// 压测轮数
    /// </summary>
    /// <example>5</example>
    [Required(ErrorMessage = "压测轮数不能为空")]
    [Range(1, 10000, ErrorMessage = "压测轮数必须为1-10000")]
    public int? NumberOfRounds { get; set; }

    /// <summary>
    /// 最大并行量（默认为当前主机逻辑处理器的数量）
    /// </summary>
    /// <example>500</example>
    [Range(0, 10000, ErrorMessage = "最大并行量必须为0-10000")]
    public int? MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// 请求参数
    /// </summary>
    public List<KeyValueStringString> RequestParameters { get; set; } = new();

    /// <summary>
    /// 请求头参数
    /// </summary>
    public List<KeyValueStringString> Headers { get; set; } = new();

    /// <summary>
    /// 路径参数
    /// </summary>
    public List<KeyValueStringString> PathParameters { get; set; } = new();

    /// <summary>
    /// Query参数
    /// </summary>
    public List<KeyValueStringString> QueryParameters { get; set; } = new();
}



/// <summary>
/// 接口压测输出参数
/// </summary>
public class StressTestOutput
{
    /// <summary>
    /// 总请求次数
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// 总用时（秒）
    /// </summary>
    public double TotalTimeInSeconds { get; set; }

    /// <summary>
    /// 成功请求次数
    /// </summary>
    public long SuccessfulRequests { get; set; }

    /// <summary>
    /// 失败请求次数
    /// </summary>
    public long FailedRequests { get; set; }

    /// <summary>
    /// 每秒查询率（QPS）
    /// </summary>
    public double QueriesPerSecond { get; set; }

    /// <summary>
    /// 最小响应时间（毫秒）
    /// </summary>
    public double MinResponseTime { get; set; }

    /// <summary>
    /// 最大响应时间（毫秒）
    /// </summary>
    public double MaxResponseTime { get; set; }

    /// <summary>
    /// 平均响应时间（毫秒）
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// P10 响应时间（毫秒）
    /// </summary>
    public double Percentile10ResponseTime { get; set; }

    /// <summary>
    /// P25 响应时间（毫秒）
    /// </summary>
    public double Percentile25ResponseTime { get; set; }

    /// <summary>
    /// P50 响应时间（毫秒）
    /// </summary>
    public double Percentile50ResponseTime { get; set; }

    /// <summary>
    /// P75 响应时间（毫秒）
    /// </summary>
    public double Percentile75ResponseTime { get; set; }

    /// <summary>
    /// P90 响应时间（毫秒）
    /// </summary>
    public double Percentile90ResponseTime { get; set; }

    /// <summary>
    /// P99 响应时间（毫秒）
    /// </summary>
    public double Percentile99ResponseTime { get; set; }

    /// <summary>
    /// P999 响应时间（毫秒）
    /// </summary>
    public double Percentile999ResponseTime { get; set; }
}
