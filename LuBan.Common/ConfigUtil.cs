/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： ConfigUtil
*版本号： V1.0.0.0
*唯一标识：d342e350-ad94-408a-863d-1d39ea79d160
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/4/27 9:10:42
*描述：appsettings.json配置读取工具类
*
*=====================================================================
*修改标记
*修改时间：2021/4/27 9:10:42
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：appsettings.json配置读取工具类
*
*****************************************************************************/

namespace LuBan.Common;

/// <summary>
/// appsettings.json配置读取工具类
/// </summary>
public static class ConfigUtil
{
    static IConfiguration _configuration;


    /// <summary>
    /// 设置全局读写配置工具
    /// </summary>
    /// <param name="configuration"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void InitConfigUtil(this IConfiguration configuration)
    {
        ConsoleUtil.WriteLineWithCount("正在加载HostingOptions配置", color: ConsoleColor.Green);
        _configuration = configuration ?? throw new NotImplementedException("传入的配置对象不能为空");
    }

    /// <summary>
    /// GetRoot
    /// </summary>
    /// <param name="configFileName"></param>
    /// <returns></returns>
    static IConfiguration GetRoot(string configFileName)
    {
        if (_configuration == null)
        {
            var builder = new ConfigurationBuilder().AddJsonFile(configFileName, true, true);
            return builder.Build();
        }
        return _configuration;
    }

    /// <summary>
    /// 读取配置
    /// </summary>
    /// <param name="sectionName"></param>
    /// <param name="configFileName"></param>
    /// <returns></returns>
    public static string? Read(string sectionName, string configFileName = "")
    {
        if (configFileName.IsNullOrEmpty())
        {
            configFileName = GetConfigFileNameByEnvironment();
        }
        var configuration = GetRoot(configFileName);
        if (sectionName.IndexOf(":") > -1)
        {
            return configuration.GetSection(sectionName).Value;
        }
        return configuration[sectionName];
    }

    /// <summary>
    /// 根据环境获取配置文件名称
    /// </summary>
    /// <returns></returns>
    private static string GetConfigFileNameByEnvironment()
    {
        var configFileName = "appsettings.json";
        var envArg = "--environment".GetArgValue();
        if (envArg.IsNotNullOrEmpty())
        {
            configFileName = $"appsettings.{envArg}.json";
        }
        return configFileName;
    }

    /// <summary>
    /// 读取配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sectionName"></param>
    /// <param name="configFileName"></param>
    /// <returns></returns>
    public static T? Read<T>(string sectionName, string configFileName = "")
    {
        if (configFileName.IsNullOrEmpty())
        {
            configFileName = GetConfigFileNameByEnvironment();
        }
        var configuration = GetRoot(configFileName);
        if (sectionName.IsNullOrEmpty())
        {
            return configuration.Get<T>();
        }
        else
        {
            return configuration.GetSection(sectionName).Get<T>();
        }

    }
    /// <summary>
    /// 读取配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? Read<T>() => Read<T>(typeof(T).Name);

    /// <summary>
    /// 获取服务名
    /// </summary>
    /// <returns></returns>
    public static string GetServiceName()
    {
        return Read("HostingOptions:ServiceName") ?? "";
    }
    /// <summary>
    /// 获取配置的域名地址
    /// </summary>
    /// <returns></returns>
    public static string GetDomain()
    {
        return Read("HostingOptions:Domain") ?? "";
    }

    /// <summary>
    /// 读取nacos本地配置
    /// </summary>
    /// <returns></returns>
    public static NacosConfig? GetNacosConfigFromFile()
    {
        return Read<NacosConfig>("HostingOptions:NacosConfig");
    }
}
