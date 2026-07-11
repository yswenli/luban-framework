/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： AppOptions
*版本号： V1.0.0.0
*唯一标识：624e2ec3-984e-4e0f-8b64-e99ac4342eef
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/11 13:57:22
*描述：服务器配置
*
*=====================================================================
*修改标记
*修改时间：2024/8/9 13:57:22
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：服务器配置
*
*****************************************************************************/

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 服务器配置
/// </summary>
public sealed class AppOptions
{
    /// <summary>
    /// api接口地址
    /// </summary>
    public string[] Urls { get; set; }

    /// <summary>
    /// 是否启用静态目录
    /// </summary>
    public bool UserStaticPath { get; set; } = false;

    /// <summary>
    /// 静态目录列表
    /// </summary>
    public string[] StaticPaths { get; set; }

    /// <summary>
    /// 停用swagger
    /// </summary>
    public bool DisableSwagger { get; set; } = false;

    /// <summary>
    /// jwt相关配置
    /// </summary>
    public JwtAuthConfig JwtAuthConfig { get; set; }

    /// <summary>
    /// 最大请求长度，
    /// 默认50M
    /// </summary>
    public long MaxRequestSize { get; set; } = 5242880;

    /// <summary>
    /// 是否隐藏console日志
    /// </summary>
    public bool HideConsoleLog { get; set; } = false;

    /// <summary>
    /// 启动地址
    /// </summary>
    public string StartPath { get; set; } = "/swagger";

    /// <summary>
    /// 禁用安全参数校验
    /// </summary>
    public bool DisableSafeComparisonFilter { get; set; } = false;

    /// <summary>
    /// 安全校验的缓存时长(单位：分钟)
    /// </summary>
    public int SafeComparisonExpired { get; set; } = 5;
    /// <summary>
    /// 启用nacos变更监听
    /// </summary>
    public bool EnableNacosListener { get; set; } = true;
    /// <summary>
    /// 是否启用大文件的分段获取
    /// </summary>
    public bool EnablePartialRequest { get; set; } = true;

    /// <summary>
    /// 默认全局验证码
    /// </summary>
    public string GloabVerifyCode { get; set; } = "9365";

    /// <summary>
    /// 是否启用SignalR
    /// </summary>
    public bool EnableSignalR { get; set; } = false;
    /// <summary>
    /// SignalR相关配置
    /// </summary>
    public SignalROptions? SignalROptions { get; set; }

    /// <summary>
    /// 在线用户配置
    /// </summary>
    public OnlineUserOptions OnlineUser { get; set; } = new();
}

/// <summary>
/// 在线用户配置
/// </summary>
public sealed class OnlineUserOptions
{
    /// <summary>
    /// 是否启用在线用户管理
    /// </summary>
    public bool Enabled { get; set; } = false;
}
