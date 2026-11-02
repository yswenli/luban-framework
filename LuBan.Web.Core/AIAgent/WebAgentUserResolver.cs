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
*命名空间：LuBan.Web.Core.AIAgent
*文件名： WebAgentUserResolver
*版本号： V1.0.0.0
*唯一标识：7f1c9d52-3a4b-4e8c-9f61-2b8d5c0a7e13
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：Web 宿主用户标识解析：优先会话上下文，其次 JWT 的 UserId claim
*
*****************************************************************************/

using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Hosting;

namespace LuBan.Web.Core.AIAgent;

/// <summary>
/// Web 宿主用户标识解析。
/// <para>
/// 会话级 scope 内 <see cref="IUserContext.UserId"/> 已由 <c>SessionRuntimeManager</c>
/// 按会话归属回填；请求级 scope 内该值为 null，此时回退到 JWT 的 <c>UserId</c> claim；
/// 两者都取不到才回落 <c>local</c>（匿名访问或非请求上下文）。
/// </para>
/// <para>
/// 该解析决定会话运行时归属的用户身份，进而隔离本地长期记忆、临时目录与用量统计，
/// 故 Web 宿主必须注册本实现覆盖默认的「恒 <c>local</c>」行为。
/// </para>
/// </summary>
public sealed class WebAgentUserResolver : IAgentUserResolver
{
    /// <inheritdoc />
    public string ResolveUserId(DiContainer services)
    {
        // 会话级 scope：SessionRuntimeManager 已把真实用户写入 DefaultContextState
        if (services.GetService<IUserContext>()?.UserId is { Length: > 0 } userId)
            return userId;

        // 请求级 scope：从当前 JWT 取用户 Id（未登录时为 0）
        var claimUserId = SessionUser.UserId;
        return claimUserId > 0 ? claimUserId.ToString() : "local";
    }
}