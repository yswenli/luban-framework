/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow
*文件名： FlowExtensions
*版本号： V1.0.0.0
*唯一标识：1f9c60ad-39cb-4e08-9c16-0c4f8da086de
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30
*描述：ASP.NET Core 快速集成扩展
*
*=================================================
*修改标记
*修改时间：2025/10/30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ASP.NET Core 快速集成扩展
*
*****************************************************************************/
using Microsoft.Extensions.Hosting;

namespace LuBan.ApprovalFlow;

/// <summary>
/// 审批流预热加载服务：在应用启动后自动加载配置的流程定义文件。
/// </summary>
internal class ApprovalFlowWarmupService : IHostedService
{
    private readonly FlowBuilder _builder;
    private readonly ApprovalFlowOptions _options;

    public ApprovalFlowWarmupService(FlowBuilder builder, ApprovalFlowOptions options)
    {
        _builder = builder;
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var toLoad = new List<string>();

        // 目录批量加载
        var dataDir = _options.DataDir;
        if (!string.IsNullOrWhiteSpace(dataDir) && Directory.Exists(dataDir))
        {
            foreach (var f in Directory.EnumerateFiles(dataDir, "*.json", SearchOption.TopDirectoryOnly))
            {
                toLoad.Add(f);
            }
        }

        if (toLoad.Count > 0)
        {
            foreach (var path in toLoad.Distinct())
            {
                _ = _builder.CreateExecutorFromJsonFileAsync(path);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// 审批流的依赖注入与启动预热加载扩展。
/// </summary>
public static class FlowExtensions
{
    /// <summary>
    /// 注册审批流服务。支持在启动时从配置中加载一个或多个 JSON 流程定义。
    /// 支持键：
    /// - LuBan.ApprovalFlow:dataFile（单文件路径）
    /// - LuBan.ApprovalFlow:dataFiles（字符串数组，多文件路径）
    /// - LuBan.ApprovalFlow:dataDir（目录，自动加载其中的 *.json）
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">应用配置。</param>
    /// <returns>服务集合。</returns>
    public static IServiceCollection AddApprovalFlow(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<FlowBuilder>();
        services.AddSingleton(ApprovalFlowOptions.Default);
        services.AddHostedService<ApprovalFlowWarmupService>();

        return services;
    }
}