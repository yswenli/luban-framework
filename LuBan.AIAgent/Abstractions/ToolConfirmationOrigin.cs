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
*命名空间：LuBan.AIAgent.Abstractions
*文件名： ToolConfirmationOrigin
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/24
*描述：工具确认请求的来源（编排子代理节点），供宿主确认 UI 标注「谁在要权限」
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具确认请求的来源信息。主代理发起的确认没有来源（<c>null</c>）；
/// 编排子代理发起时携带节点标识、角色与描述，供宿主确认 UI 标注来源。
/// </summary>
/// <remarks>
/// 来源<b>仅用于展示</b>，不参与授权判定：授权仍按工具名与路径在会话内共享。
/// 该信息也不会写入工具参数，避免污染工具入参与模型上下文。
/// </remarks>
/// <param name="NodeId">编排节点标识（对应 <c>TaskNode.Id</c>）；主代理为 null。</param>
/// <param name="Role">节点角色名（对应 <c>TaskNode.Role</c>）；未指定时为 null。</param>
/// <param name="Description">节点描述（对应 <c>TaskNode.Description</c>）；无描述时为 null。</param>
public sealed record ToolConfirmationOrigin(
    string? NodeId = null,
    string? Role = null,
    string? Description = null)
{
    /// <summary>
    /// 生成展示用来源文案（如「编排节点 build（角色 coder）：生成构建脚本」）。
    /// </summary>
    /// <returns>来源文案；节点、角色、描述全为空时返回 <c>null</c>。</returns>
    public string? ToDisplayString()
    {
        if (string.IsNullOrWhiteSpace(NodeId)
            && string.IsNullOrWhiteSpace(Role)
            && string.IsNullOrWhiteSpace(Description))
        {
            return null;
        }

        var builder = new StringBuilder("编排节点 ");
        builder.Append(string.IsNullOrWhiteSpace(NodeId) ? "（未命名）" : NodeId);
        if (!string.IsNullOrWhiteSpace(Role))
        {
            builder.Append("（角色 ").Append(Role).Append('）');
        }
        if (!string.IsNullOrWhiteSpace(Description))
        {
            builder.Append("：").Append(Description);
        }
        return builder.ToString();
    }
}