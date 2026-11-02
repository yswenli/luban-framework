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
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： IAgentModelSelection
*版本号： V1.0.0.0
*唯一标识：22f0ebc6-0cae-48f3-9e1b-c74eb33a3302
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：模型选择契约：当前会话使用的模型限定名
*
*****************************************************************************/
namespace LuBan.AIAgent.Hosting;

/// <summary>当前选定的模型（宿主读取；产品可用自己的配置实现覆盖）。</summary>
public interface IAgentModelSelection
{
    /// <summary>Provider 标识（如 <c>openai</c>）。</summary>
    string? ProviderId { get; }

    /// <summary>模型标识（如 <c>gpt-4.1</c>）。</summary>
    string? ModelId { get; }

    /// <summary>限定名 <c>provider:model</c>；未选择时为空字符串。</summary>
    string QualifiedModelName { get; }
}