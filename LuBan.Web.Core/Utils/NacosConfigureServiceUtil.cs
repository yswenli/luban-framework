/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Utils
*文件名： NacosUtil
*版本号： V1.0.0.0
*唯一标识：a1e0bc71-dbd4-4928-be01-cadb25b413fd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/7/20 13:17:28
*描述：nacos集成工具类
*
*=================================================
*修改标记
*修改时间：2023/7/20 13:17:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：nacos集成工具类
*
*****************************************************************************/


using Nacos.AspNetCore.V2;

namespace LuBan.Web.Core.Utils;

/// <summary>
/// nacos集成工具类
/// </summary>
public static class NacosConfigureServiceUtil
{
    /// <summary>
    /// 是否可用
    /// </summary>
    public static bool Enable { get; set; } = false;

    /// <summary>
    /// aspnetcore中初始化nacos config center
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="hostingOptions"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void AddNacosConfigCenterService(this IServiceCollection services, IConfiguration configuration, HostingOptions hostingOptions)
    {
        var nacosConfig = configuration.GetSection("NacosConfig").Get<NacosConfig>();
        if (nacosConfig == null)
        {
            return;
        }
        ConsoleUtil.WriteLineWithCount("正在集成Nacos配置", color: ConsoleColor.Green);
        Enable = true;

        services.AddNacosAspNet(configuration, "nacos");
        services.AddNacosV2Config((n) =>
        {
            n.ConfigUseRpc = false;
            n.NamingUseRpc = false;
            n.ServerAddresses = nacosConfig.ServerAddresses;
            n.ListenInterval = 5000;
            n.Namespace = nacosConfig.Namespace;
        });
        var serviceProvider = services.BuildServiceProvider();
        var configSvc = serviceProvider.GetService<INacosConfigService>();
        if (configSvc == null) throw new NotImplementedException($"nacos config center 连接初始化失败:" + nacosConfig.ToJson());

        if (nacosConfig.Listeners != null && nacosConfig.Listeners.Count > 0)
        {
            foreach (var item in nacosConfig.Listeners)
            {
                //初始化数据
                var content = configSvc.GetConfig(item.DataId, item.Group, 10 * 1000).GetAwaiter().GetResult();
                if (content.IsNotNullOrEmpty())
                    NacosConfigUtil.Set(nacosConfig.Namespace, item.Group, item.DataId, content);
                else
                    throw new NotImplementedException("从nacos config center中读取配置失败:" + nacosConfig.ToJson());


                if (hostingOptions.AppOptions.EnableNacosListener)
                {
                    //初始化配置变化监听
                    configSvc.AddListener(item.DataId, item.Group, new NacosConfigListener(nacosConfig.Namespace, item.DataId, item.Group));

                }
            }
        }
        else
        {
            throw new NotImplementedException("读取nacos服务器中的配置失败:" + nacosConfig.ToJson());
        }
    }


    /// <summary>
    /// 测试专用，初始化nacos config center
    /// </summary>
    [Obsolete("测试专用")]
    public static void InitForTest()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        AddNacosConfigCenterService(serviceCollection, configuration, HostingOptions.Default);
    }

    //

}

/// <summary>
/// 配置变化监听
/// </summary>
public class NacosConfigListener : IListener
{
    string _nameSpace, _dataId, _group;

    /// <summary>
    /// 配置变化监听
    /// </summary>
    /// <param name="nameSpace"></param>
    /// <param name="group"></param>
    /// <param name="dataId"></param>
    public NacosConfigListener(string nameSpace, string group, string dataId)
    {
        _nameSpace = nameSpace;
        _dataId = dataId;
        _group = group;
    }
    /// <summary>
    /// 接收配置方法
    /// </summary>
    /// <param name="configInfo"></param>
    public void ReceiveConfigInfo(string configInfo)
    {
        NacosConfigUtil.Set(_nameSpace, _dataId, _group, configInfo);
    }
}
