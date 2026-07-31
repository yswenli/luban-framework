/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
    /// API 基础 URL
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// API 密钥
    /// </summary>
    public string? ApiKey { get; set; }
}