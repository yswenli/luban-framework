/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit
*文件名： EMailOptions
*版本号： V1.0.0.0
*唯一标识：6629eecc-652a-45aa-add1-f386f42569da
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/14 16:43:29
*描述：
*
*=================================================
*修改标记
*修改时间：2024/8/14 16:43:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.EMailKit;

/// <summary>
/// 邮件客户端配置
/// </summary>
public class EMailClientConfig
{
    /// <summary>
    /// 客户端类型
    /// </summary>
    public EnumClientType ClientType { get; set; }

    /// <summary>
    /// 主机
    /// </summary>
    public string Host { get; set; }
    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// 是否使用ssl
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
}

/// <summary>
/// 邮件选项
/// </summary>
public class EMailOptions
{
    /// <summary>
    /// 邮件客户端配置列表
    /// </summary>
    public List<EMailClientConfig> EMailClientConfigs { get; set; } = new List<EMailClientConfig>();
}
