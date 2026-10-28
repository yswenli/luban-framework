/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.AIAgent
*文件名： LuBanAgentStartupOptions
*版本号： V1.0.0.0
*唯一标识：fde87925-9837-49e3-bcd9-0dc36a3efc6e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：LuBanAgent 配置节的校验用扁平模型
*
*****************************************************************************/

namespace LuBan.Web.Core.AIAgent;

/// <summary>
/// LuBanAgent 配置节的校验用扁平模型（仅结构与 appsettings 一致，不代表运行期绑定类型）。
/// </summary>
public sealed class LuBanAgentStartupOptions
{
    /// <summary>
    /// Agent 工作目录
    /// </summary>
    public string? WorkspaceRoot { get; set; }

    /// <summary>
    /// 默认模型（格式 provider:model）；可省略，省略时取 Models 第一项
    /// </summary>
    public string? DefaultModel { get; set; }

    /// <summary>
    /// 模型接入点列表；每项是一个 provider:model 组合
    /// </summary>
    public List<LuBanAgentModelsOptions>? Models { get; set; }
}

/// <summary>
/// 模型接入点配置
/// </summary>
public sealed class LuBanAgentModelsOptions
{
    /// <summary>
    /// Provider 标识
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 模型标识
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 访问密钥
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// 接口地址（可选，缺省用 Provider 内置地址）
    /// </summary>
    public string? BaseUrl { get; set; }
}