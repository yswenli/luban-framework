/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Extensions
*文件名： ServiceCollectionExtensions
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：服务注册扩展方法
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：服务注册扩展方法
*
*****************************************************************************/
namespace LuBan.Reporting.Extensions;

/// <summary>
/// 服务注册扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 LuBan.Reporting 服务
    /// </summary>
    public static IServiceCollection AddLuBanReporting(this IServiceCollection services)
    {
        ConsoleUtil.WriteLineWithCount("注册动态报表服务", color: ConsoleColor.Green);

        // 注册 Lua 脚本引擎（Singleton，全局复用）
        services.AddSingleton<LuaScriptEngine>();

        // 注册动态报表服务
        services.AddScoped<DynamicReportService>();
        services.AddScoped<ReportConfigService>();
        services.AddScoped<DynamicReportRepository>();

        return services;
    }
}
