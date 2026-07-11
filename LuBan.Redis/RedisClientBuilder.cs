/****************************************************************************
*Copyright @ 2023-2024 riverland All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：riverland
*命名空间：LuBan.Redis
*文件名： RedisClientBuilder
*版本号： V1.0.0.0
*唯一标识：0e4e869f-8cfa-498c-838e-0a0a6d978bfe
*当前的用户域：riverland
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 15:18:08
*描述：redis封装类创建者
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 15:18:08
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：redis封装类创建者
*
*****************************************************************************/
namespace LuBan.Redis;

using System.Collections.Concurrent;

/// <summary>
/// redis封装类创建者
/// </summary>
public static class RedisClientBuilder
{
    static ConcurrentDictionary<string, RedisClient> _cache;

    /// <summary>
    /// RedisClient容器
    /// </summary>
    static RedisClientBuilder()
    {
        _cache = new ConcurrentDictionary<string, RedisClient>();
    }

    /// <summary>
    /// 根据指定配置产生一个新的实例
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static RedisClient Build(RedisOptions config)
    {
        return _cache.GetOrAdd(config.Masters, _ =>
        {
            return new RedisClient(config);
        });
    }

}
