/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore.Extentions
*文件名： ServiceCacheExtention
*版本号： V1.0.0.0
*唯一标识：7c9c50ea-d5d9-44cf-b045-4c2dcfeca03e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/20 15:59:43
*描述：业务缓存扩展类
*
*=================================================
*修改标记
*修改时间：2025/11/20 15:59:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：业务缓存扩展类
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore.Extentions;

/// <summary>
/// 业务缓存扩展类
/// </summary>
public static class ServiceCacheExtention
{
    /// <summary>
    /// 根据HostingOptions配置注入缓存单例
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddServiceCache(this IServiceCollection services)
    {
        ConsoleUtil.WriteLineWithCount("正在初始化缓存", color: ConsoleColor.Green);
        var options = ConfigUtil.Read<HostingOptions>()!;
        if (options.EnableRedisCache)
        {
            try
            {
                services.AddSingleton<IServiceCache>(RedisCache.Instance);
            }
            catch (Exception ex)
            {
                Logger.Error("请检查Redis配置是否正确", ex);
            }
        }
        else
        {
            services.AddSingleton<IServiceCache>(MemoryCache.Instance);
        }
        return services;
    }

    /// <summary>
    /// 系统缓存
    /// </summary>
    /// <returns></returns>
    public static IServiceCache ServiceCache
    {
        get
        {
            return DiContainer.Instance.GetRequiredService<IServiceCache>();
        }
    }
}
