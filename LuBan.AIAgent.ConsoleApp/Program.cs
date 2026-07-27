/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp
*文件名： Program
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：程序入口
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.ConsoleApp.Infrastructure;
using LuBan.AIAgent.ConsoleApp.Services;
using LuBan.AIAgent.Sessions;
using LuBan.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.AIAgent.ConsoleApp;

/// <summary>
/// 程序入口
/// </summary>
class Program
{
    /// <summary>
    /// 程序主入口
    /// </summary>
    static async Task Main(string[] args)
    {
        ConsoleUtil.PrintName();

        // 初始化数据库
        DatabaseInitializer.Initialize();

        var configuration = BuildConfiguration(args);
        using var serviceProvider = BuildServiceProvider(configuration);

        var appService = serviceProvider.GetRequiredService<ConsoleAppService>();
        await appService.RunAsync();
    }

    /// <summary>
    /// 构建配置
    /// </summary>
    private static IConfiguration BuildConfiguration(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        return builder.Build();
    }

    /// <summary>
    /// 构建服务提供者
    /// </summary>
    private static ServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        // 配置
        services.AddSingleton<IConfiguration>(configuration);

        // 配置管理器
        var configPath = ConfigManager.GetDefaultConfigPath();
        var configManager = new ConfigManager(configPath);
        configManager.Load();
        services.AddSingleton(configManager);

        // 注册 LuBan Agent 服务（包含 Skills, Rules, MCP）
        services.AddLuBanAgent(configuration);

        // 注册 Session 管理器
        services.AddSingleton<ISessionManager, SessionManager>();

        // 注册 ConsoleAppService
        services.AddSingleton<ConsoleAppService>();

        return services.BuildServiceProvider();
    }
}