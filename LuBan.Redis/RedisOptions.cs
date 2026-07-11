/****************************************************************************
*Copyright @ 2023-2024 riverland All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：riverland
*命名空间：LuBan.Redis
*文件名： RedisOptions
*版本号： V1.0.0.0
*唯一标识：dc7b295f-4d45-4942-8a99-932d4ce086ba
*当前的用户域：riverland
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 15:21:02
*描述：redis配置类
*
*=====================================================================
*修改标记
*修改时间：2024/8/9 15:21:02
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：redis配置类
*
*****************************************************************************/

namespace LuBan.Redis;

/// <summary>
/// redis配置类
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// 连接类型
    /// </summary>
    public EnumRedisType Type { get; set; }

    /// <summary>
    /// 主Redis库，亦可是sentinel服务器地址
    /// </summary>
    public string Masters
    {
        get; set;
    }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password
    {
        get; set;
    }

    /// <summary>
    /// 从redis库
    /// </summary>
    public string Slaves
    {
        get; set;
    }

    /// <summary>
    /// 哨兵模式下服务名称
    /// </summary>
    public string ServiceName
    {
        get; set;
    } = "mymaster";

    /// <summary>
    /// 非集群模式下可以指定读写db
    /// </summary>
    public int DefaultDatabase
    {
        get; set;
    } = 0;

    /// <summary>
    /// 管理员模式
    /// </summary>
    public bool AllowAdmin
    {
        get; set;
    } = true;

    /// <summary>
    /// 连接保持(s)
    /// </summary>
    public int KeepAlive
    {
        get; set;
    } = 180;

    /// <summary>
    /// 连接超时(ms)
    /// </summary>
    public int ConnectTimeout
    {
        get; set;
    } = 10 * 1000;

    /// <summary>
    /// 重连次数
    /// </summary>
    public int ConnectRetry
    {
        get; set;
    } = 1;

    /// <summary>
    /// 任务忙重试次数
    /// 1-10000之间的整数
    /// </summary>
    public int BusyRetry
    {
        get; set;
    } = 3;

    /// <summary>
    /// 重试等待时长(ms)
    /// </summary>
    public int BusyRetryWaitMS
    {
        get; set;
    } = 1000;


    /// <summary>
    /// 命令超时时间 (ms)
    /// </summary>
    public int CommandTimeout
    {
        get; set;
    } = 60000;

    /// <summary>
    /// 扩展
    /// 有一些redis因为禁用了某些命令需要添加如下部分
    /// $CLIENT=,$CLUSTER=,$CONFIG=,$ECHO=,$INFO=,$PING=
    /// </summary>
    public string Extention
    {
        get; set;
    }

    /// <summary>
    /// 从Json中读取配置
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static RedisOptions Parse(string json)
    {
        return SerializeUtil.Deserialize<RedisOptions>(json) ?? throw new Exception("Redis配置解析失败");
    }

    /// <summary>
    /// 将stackexchange.redis的连接字符串转换成配置对象
    /// </summary>
    /// <param name="seConnectStr"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static RedisOptions ParseByStackExchangeConnectStr(string seConnectStr)
    {
        if (seConnectStr.IsNullOrEmpty()) throw new Exception("StackExchangeRedis连接字符串不能为空");
        var arr = seConnectStr.Split([","], StringSplitOptions.RemoveEmptyEntries);
        if (arr == null || arr.Length < 1) throw new Exception("StackExchangeRedis连接字符串格式有误");
        RedisOptions redisOptions = new();
        foreach (var item in arr)
        {
            if (item.IsNullOrEmpty()) continue;
            var sarr = item.Split("=", StringSplitOptions.RemoveEmptyEntries);
            if (sarr == null || sarr.Length < 1) continue;
            if (sarr.Length == 1)
            {
                redisOptions.Masters = sarr[0];
            }
            else
            {
                switch (sarr[0].ToLower())
                {
                    case "password":
                        redisOptions.Password = sarr[1];
                        break;
                    case "connecttimeout":
                        redisOptions.ConnectTimeout = int.Parse(sarr[1]);
                        break;
                    case "connectretry":
                        redisOptions.ConnectRetry = int.Parse(sarr[1]);
                        break;
                    case "synctimeout":
                        redisOptions.CommandTimeout = int.Parse(sarr[1]);
                        break;
                }
            }
        }
        return redisOptions;
    }
}
