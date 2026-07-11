/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis
*文件名： RedisConnectType
*版本号： V1.0.0.0
*唯一标识：cf052dd9-bc8f-40ea-b305-1a262fa1c764
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/24 11:18:34
*描述：redis连接类型
*
*=================================================
*修改标记
*修改时间：2025/3/24 11:18:34
*修改人： yswenli
*版本号： V1.0.0.0
*描述：redis连接类型
*
*****************************************************************************/
namespace LuBan.Redis;

/// <summary>
/// redis连接类型
/// </summary>
public enum EnumRedisType
{
    /// <summary>
    /// Instance
    /// </summary>
    Instance = 0,
    /// <summary>
    /// Sentinel
    /// </summary>
    Sentinel = 1,
    /// <summary>
    /// Cluster
    /// </summary>
    Cluster = 2
}
