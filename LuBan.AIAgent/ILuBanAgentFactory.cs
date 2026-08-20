/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： ILuBanAgentFactory
*版本号： V1.0.0.0
*唯一标识：5ecf6fa5-aa2a-4957-8be1-bddf447ca821
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:21:20
*描述：LuBan Agent 工厂接口
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:21:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan Agent 工厂接口
*
*****************************************************************************/
namespace LuBan.AIAgent;

/// <summary>
/// LuBan Agent 工厂接口
/// </summary>
public interface ILuBanAgentFactory
{
    /// <summary>
    /// 创建 Agent 实例
    /// </summary>
    /// <param name="modelName">模型名称，格式 "provider:model"</param>
    /// <param name="systemPrompt">自定义系统提示词</param>
    /// <param name="toolGroups">指定启用的工具组，null 表示全部启用</param>
    /// <param name="useSessionHistory">是否启用 Session 历史</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>LuBanAgent 实例</returns>
    Task<LuBanAgent> CreateAsync(
        string? modelName = null,
        string? systemPrompt = null,
        IEnumerable<string>? toolGroups = null,
        bool useSessionHistory = false,
        CancellationToken cancellationToken = default);
}