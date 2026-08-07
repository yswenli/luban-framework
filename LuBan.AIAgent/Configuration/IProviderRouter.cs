using Microsoft.Extensions.AI;

/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： IProviderRouter
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：模型提供者路由接口定义
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 模型提供者路由接口
/// </summary>
public interface IProviderRouter
{
    /// <summary>
    /// 创建聊天客户端
    /// </summary>
    /// <param name="providerModel">提供者模型名称，null 表示使用默认选中的模型</param>
    /// <returns>聊天客户端实例</returns>
    IChatClient CreateChatClient(string? providerModel = null);

    /// <summary>
    /// 获取可用的提供者列表
    /// </summary>
    /// <returns>可用提供者信息列表</returns>
    IReadOnlyList<ProviderInfo> GetAvailableProviders();
}

/// <summary>
/// 提供者信息
/// </summary>
/// <param name="Name">提供者标识名称</param>
/// <param name="DisplayName">提供者显示名称</param>
/// <param name="Models">支持的模型列表</param>
public record ProviderInfo(string Name, string DisplayName, string[] Models);
