/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：yswenli.Comon.DataProcessing
*文件名： CommonController
*版本号： V1.0.0.0
*唯一标识：df6193ea-6e56-4c51-89f2-b61680366c23
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/15 11:52:22
*描述：数据库配置的通用接口
*
*=====================================================================
*修改标记
*修改时间：2022/7/15 11:52:22
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：数据库配置的通用接口
*
*****************************************************************************/
using System.Web;

namespace LuBan.Web.Core.Controllers;

/// <summary>
/// 通用接口
/// </summary>
[ApiExplorerSettings(GroupName = "default")]
public sealed class CommonController : BaseApiController
{
    /// <summary>
    /// ping健康检查
    /// </summary>
    /// <returns></returns>
    [Route("/"), HttpGet, AllowAnonymous, NoAraParameterFilter]
    public string Ping()
    {
        return $"{_hostingOptions.ServiceName} PONG";
    }

    /// <summary>
    /// 获取swagger资源 🔖
    /// </summary>
    /// <returns></returns>
    [HttpGet("/swagger-resources")]
    public async Task<List<SwaggerNameUrl>> SwaggerGroups()
    {
        if (SessionUser.UserId != LuBanOrmConst.SuperAdminId) throw FriendlyError.Ex(EnumErrorCode.D1016);
        var result = new List<SwaggerNameUrl>
        {
            new () { Name = "admin", Url = "/swagger/admin/swagger.json" },
            new () { Name = "mobile", Url = "/swagger/mobile/swagger.json" },
            new () { Name = "default", Url = "/swagger/default/swagger.json" },
            new () { Name = "open", Url = "/swagger/open/swagger.json" }
        };

        return await Task.FromResult(result);
    }

    /// <summary>
    /// 接口压测🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<StressTestOutput> StressTest([Required, FromBody] StressTestInput input)
    {
        var stopwatch = new Stopwatch();
        var responseTimes = new List<double>();  //响应时间集合
        input.RequestMethod = input.RequestMethod.ToUpper();
        long totalRequests = 0, successfulRequests = 0, failedRequests = 0;

        stopwatch.Start();
        var semaphore = new SemaphoreSlim(input.MaxDegreeOfParallelism!.Value > 0 ? input.MaxDegreeOfParallelism.Value : Environment.ProcessorCount);

        #region 参数构建

        // 构建基础URI（不包括路径和查询参数）
        var baseUriBuilder = new UriBuilder(input.RequestUri);
        var queryString = HttpUtility.ParseQueryString(baseUriBuilder.Query);

        // 替换路径参数到baseUriBuilder.Path
        foreach (var param in input.PathParameters)
        {
            baseUriBuilder.Path = baseUriBuilder.Path.Replace($"{{{param.Key}}}", param.Value, StringComparison.OrdinalIgnoreCase);
        }

        // 构建Query参数
        foreach (var param in input.QueryParameters)
        {
            queryString[param.Key] = param.Value;
        }

        baseUriBuilder.Query = queryString.ToString() ?? string.Empty;
        var fullUri = baseUriBuilder.Uri;
        var baseUri = fullUri.GetLeftPart(UriPartial.Authority);
        var absoulteUri = fullUri.PathAndQuery;
        var httpClient = HttpClientProxy.Create(baseUri);

        #endregion 参数构建

        var tasks = Enumerable.Range(0, input.NumberOfRounds!.Value * input.NumberOfRequests!.Value).Select(async _ =>
        {
            await semaphore.WaitAsync();
            try
            {
                var requestStopwatch = new Stopwatch();
                requestStopwatch.Start();
                using var request = CreateRequestMessage(input, absoulteUri);
                if (!string.Equals(input.RequestMethod, "GET", StringComparison.OrdinalIgnoreCase) && input.RequestParameters.Any())
                {
                    var content = new FormUrlEncodedContent(input.RequestParameters.ToDictionary());
                    request!.Content = content;
                }

                using var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode(); // 抛出错误状态码异常
                requestStopwatch.Stop();
                responseTimes.Add(requestStopwatch.Elapsed.TotalMilliseconds);
                Interlocked.Increment(ref successfulRequests);
            }
            catch// (Exception ex)
            {
                Interlocked.Increment(ref failedRequests);
            }
            finally
            {
                Interlocked.Increment(ref totalRequests);
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        var totalTimeInSeconds = stopwatch.Elapsed.TotalSeconds;
        var qps = totalTimeInSeconds > 0 ? totalRequests / totalTimeInSeconds : 0;
        var orderResponseTimes = responseTimes.OrderBy(t => t).ToList();
        var averageResponseTime = responseTimes.Any() ? responseTimes.Average() : 0;
        var minResponseTime = responseTimes.Any() ? responseTimes.Min() : 0;
        var maxResponseTime = responseTimes.Any() ? responseTimes.Max() : 0;

        return new StressTestOutput
        {
            TotalRequests = totalRequests,
            TotalTimeInSeconds = totalTimeInSeconds,
            SuccessfulRequests = successfulRequests,
            FailedRequests = failedRequests,
            QueriesPerSecond = qps,
            MinResponseTime = minResponseTime,
            MaxResponseTime = maxResponseTime,
            AverageResponseTime = averageResponseTime,
            Percentile10ResponseTime = CalculatePercentile(orderResponseTimes, 0.1),
            Percentile25ResponseTime = CalculatePercentile(orderResponseTimes, 0.25),
            Percentile50ResponseTime = CalculatePercentile(orderResponseTimes, 0.5),
            Percentile75ResponseTime = CalculatePercentile(orderResponseTimes, 0.75),
            Percentile90ResponseTime = CalculatePercentile(orderResponseTimes, 0.9),
            Percentile99ResponseTime = CalculatePercentile(orderResponseTimes, 0.99),
            Percentile999ResponseTime = CalculatePercentile(orderResponseTimes, 0.999)
        };
    }

    /// <summary>
    /// 创建请求消息
    /// </summary>
    /// <param name="input">输入参数</param>
    /// <param name="queryAndPath">/api/xxx?xxx=xxx</param>
    /// <returns></returns>
    private HttpRequestMessage CreateRequestMessage(StressTestInput input, string queryAndPath)
    {
        HttpRequestMessage request = input.RequestMethod switch
        {
            "GET" => new HttpRequestMessage(HttpMethod.Get, queryAndPath),
            "PUT" => new HttpRequestMessage(HttpMethod.Put, queryAndPath),
            "POST" => new HttpRequestMessage(HttpMethod.Post, queryAndPath),
            "DELETE" => new HttpRequestMessage(HttpMethod.Delete, queryAndPath),
            _ => throw FriendlyError.Ex("请求方式异常")
        };

        // 设置请求头
        foreach (var header in input.Headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }
        return request;
    }

    /// <summary>
    /// 计算百分位请求耗时
    /// </summary>
    /// <param name="times">请求耗时列表</param>
    /// <param name="percentile">百分位</param>
    /// <returns></returns>
    private double CalculatePercentile(List<double> times, double percentile)
    {
        if (!times.Any()) return 0;
        var index = (int)Math.Ceiling(percentile * times.Count) - 1;
        return times[index < times.Count ? index : times.Count - 1];
    }
}
