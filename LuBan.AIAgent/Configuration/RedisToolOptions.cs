/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： RedisToolOptions
*版本号： V1.0.0.0
*唯一标识：843c8775-02ce-496d-9313-e31d77164954
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：Redis 工具配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Redis 工具配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Redis 工具配置
/// </summary>
public class RedisToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Redis 主机
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Redis 端口
    /// </summary>
    public int Port { get; set; } = 6379;

    /// <summary>
    /// Redis 密码
    /// </summary>
    public string? Password { get; set; }
}