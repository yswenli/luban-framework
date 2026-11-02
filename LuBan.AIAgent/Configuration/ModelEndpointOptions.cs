/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： ModelEndpointOptions
*版本号： V1.0.0.0
*唯一标识：315398f3-ff8e-437a-b44a-4d7b77b1729d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：模型端点配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：模型端点配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 模型端点配置
/// </summary>
public class ModelEndpointOptions
{
    /// <summary>
    /// Provider 标识（如 deepseek、kimi，大小写不敏感）
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 模型标识（如 deepseek-v4-pro）
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// API 基础 URL（可选，缺省用 Provider 内置地址）
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// API 密钥
    /// </summary>
    public string? ApiKey { get; set; }
}