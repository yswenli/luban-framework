/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： NacosConfigUtil
*版本号： V1.0.0.0
*唯一标识：e7cfba95-5a1d-4944-9fbe-672c50960bc6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/7/20 11:36:44
*描述：Nacos配置工具类
*
*=================================================
*修改标记
*修改时间：2023/7/20 11:36:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Nacos配置工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// Nacos配置工具类
/// </summary>
[DataContract]
public class NacosConfig
{
    /// <summary>
    /// Listeners
    /// </summary>
    [DataMember(Name = "Listeners")]
    public IList<Listener> Listeners { get; set; }

    /// <summary>
    /// Namespace, dev uat pro
    /// </summary>
    [DataMember(Name = "Namespace")]
    public string Namespace { get; set; }
    /// <summary>
    /// nacos服务器地址
    /// </summary>
    [DataMember(Name = "ServerAddresses")]
    public List<string> ServerAddresses { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    [DataMember(Name = "UserName")]
    public string UserName { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    [DataMember(Name = "Password")]
    public string Password { get; set; }
    /// <summary>
    /// rpc
    /// </summary>
    [DataMember(Name = "ConfigUseRpc")]
    public bool ConfigUseRpc { get; set; } = false;
    /// <summary>
    /// rpc
    /// </summary>
    [DataMember(Name = "NamingUseRpc")]
    public bool NamingUseRpc { get; set; } = false;
}
/// <summary>
/// nacos监听相关
/// </summary>
[DataContract]
public class Listener
{

    [DataMember(Name = "Optional")]
    public bool Optional { get; set; }

    [DataMember(Name = "DataId")]
    public string DataId { get; set; }

    [DataMember(Name = "Group")]
    public string Group { get; set; }
}

/// <summary>
/// Nacos配置工具类
/// </summary>
public static class NacosConfigUtil
{
    static MemoryCache _cache;

    //namaspace用于环境，在运行中一般不变
    static string _nameSpace;


    /// <summary>
    /// 是否可用
    /// </summary>
    public static bool Enable { get; set; } = false;

    /// <summary>
    /// Nacos配置工具类
    /// </summary>
    static NacosConfigUtil()
    {
        _cache = MemoryCache.Instance;
    }

    /// <summary>
    /// GetRoot
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    static IConfigurationRoot GetRoot(string content)
    {
        if (content.IsNullOrEmpty()) throw new ArgumentNullException("content");
        var stream = content.ToStream();
        return new ConfigurationBuilder().AddJsonStream(stream!).Build();
    }


    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="nameSpace"></param>
    /// <param name="groupName"></param>
    /// <param name="dataId"></param>
    /// <param name="content"></param>
    public static void Set(string nameSpace, string groupName, string dataId, string content)
    {
        using (var locker = LockerBuilder.Default.Create("NacosConfigUtil.Set"))
        {
            _nameSpace = nameSpace;
            if (!_cache.ContainsKey(nameSpace))
            {
                _cache.Set(nameSpace, new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>());
            }
            var keyValuePairs = _cache.Get<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>>(nameSpace);
            if (!keyValuePairs!.TryGetValue(groupName, out ConcurrentDictionary<string, string>? value))
            {
                value = new ConcurrentDictionary<string, string>();
                keyValuePairs[groupName] = value;
            }
            value[dataId] = content;
            Enable = true;
        }
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="dataId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="Exception"></exception>
    public static string GetContent(string groupName, string dataId)
    {
        if (dataId.IsNullOrEmpty())
        {
            throw new ArgumentException("dataId不能为空");
        }
        using var locker = LockerBuilder.Default.Create("NacosConfigUtil.GetContent");
        if (!_cache.ContainsKey(_nameSpace))
        {
            throw new NotImplementedException("当前配置还未初始化");
        }
        var keyValuePairs = _cache.Get<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>>(_nameSpace);
        if (keyValuePairs == null || !keyValuePairs.TryGetValue(groupName, out ConcurrentDictionary<string, string>? value))
        {
            return string.Empty;
        }
        return value[dataId];
    }

    /// <summary>
    /// 获取内容列表
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="Exception"></exception>
    public static List<string> GetContents(string groupName = "DEFAULT_GROUP")
    {
        using var locker = LockerBuilder.Default.Create("NacosConfigUtil.GetContents");
        if (!_cache.ContainsKey(_nameSpace))
        {
            throw new NotImplementedException("当前配置还未初始化");
        }
        var keyValuePairs = _cache.Get<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>>(_nameSpace);
        if (keyValuePairs == null || !keyValuePairs.ContainsKey(groupName))
        {
            return [];
        }
        var result = new List<string>();
        var localNacosConfig = ConfigUtil.GetNacosConfigFromFile();
        if (localNacosConfig == null) return result;
        foreach (var item in localNacosConfig.Listeners)
        {
            if (keyValuePairs[groupName].ContainsKey(item.DataId))
            {
                var content = keyValuePairs[groupName][item.DataId];
                if (content.IsNotNullOrEmpty())
                {
                    result.Add(content);
                }
            }
        }
        return result;
    }



    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="groupName"></param>
    /// <param name="dataId"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static T? GetConfig<T>(string groupName, string dataId, string sectionName)
    {
        if (sectionName.IsNullOrEmpty()) return default;
        if (dataId.IsNotNullOrEmpty())
        {
            var content = GetContent(groupName, dataId);
            return GetRoot(content).GetSection(sectionName).Get<T>();
        }
        else
        {
            var contents = GetContents(groupName);
            if (contents == null || contents.Count < 1) return default;
            foreach (var content in contents)
            {
                if (content.IsNullOrEmpty() || content.IndexOf($"\"{sectionName}\"") == -1) continue;
                var val = GetRoot(content).GetSection(sectionName).Get<T?>();
                if (val == null) continue;
                return val;
            }
        }
        return default(T?);
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="dataId"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static string? GetConfig(string groupName, string dataId, string sectionName)
    {
        return GetConfig<string>(groupName, dataId, sectionName);
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static List<T> GetConfigs<T>(string sectionName)
    {
        var result = new List<T>();
        var contents = GetContents();
        if (contents == null || contents.Count < 1) return [];
        foreach (var content in contents)
        {
            var val = GetRoot(content).GetSection(sectionName).Get<T?>();
            if (val == null) continue;
            result.Add(val);
        }
        return result;
    }



    /// <summary>
    /// 读取配置字符串
    /// </summary>
    /// <param name="dataId"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static string? Read(string dataId, string sectionName)
    {
        return GetConfig("DEFAULT_GROUP", dataId, sectionName);
    }

    /// <summary>
    /// 读取配置字符串
    /// </summary>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static string? Read(string sectionName)
    {
        if (Enable)
            return Read(string.Empty, sectionName);
        else
            return ConfigUtil.Read(sectionName);
    }

    /// <summary>
    /// 读取配置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataId"></param>
    /// <param name="sectionName"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public static T? Read<T>(string dataId, string sectionName, string groupName = "DEFAULT_GROUP")
    {
        return GetConfig<T>(groupName, dataId, sectionName);
    }

    /// <summary>
    /// 读取配置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static T? Read<T>(string sectionName)
    {
        if (Enable)
            return Read<T>(string.Empty, sectionName);
        else
            return ConfigUtil.Read<T>(sectionName);
    }

    /// <summary>
    /// 读取配置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? Read<T>() => Read<T>(typeof(T).Name);

    /// <summary>
    /// 是否启用调试
    /// </summary>
    public static bool EnabelDebug
    {
        get
        {
            return Read<bool>("HostingOptions:EnabelDebug");
        }
    }

    /// <summary>
    /// 获取环境变量
    /// </summary>
    /// <returns></returns>
    public static string? GetEnvironment()
    {
        return Read("HostingOptions:Environment");
    }

}
