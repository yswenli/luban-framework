/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： ProviderConfig
*版本号： V1.0.0.0
*唯一标识：26d9fbbb-1243-41ee-b227-90941fa2d7ca
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：模型提供方配置
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：模型提供方配置
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Provider 配置项
/// </summary>
public class ProviderConfig
{
    /// <summary>
    /// Provider 名称（如 openai, azure, deepseek）
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// API 密钥
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// API 基础 URL（可选，用于自定义端点）
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Provider 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 支持的模型列表（运行时填充，不保存到配置文件）
    /// </summary>
    public List<string> SupportedModels { get; set; } = new();

    /// <summary>
    /// 用户自定义的模型列表（持久化保存）
    /// </summary>
    public List<string> CustomModels { get; set; } = new();
}