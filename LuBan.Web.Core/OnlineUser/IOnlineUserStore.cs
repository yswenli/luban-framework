/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.OnlineUser
*文件名： IOnlineUserStore
*版本号： V1.0.0.0
*唯一标识：0506282b-82ec-4dd9-aede-99d02655c571
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:53:58
*描述：在线用户存储抽象
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:53:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：在线用户存储抽象
*
*****************************************************************************/
namespace LuBan.Web.Core.OnlineUser;

/// <summary>
/// 在线用户存储抽象
/// </summary>
public interface IOnlineUserStore
{
    /// <summary>
    /// 写入或覆盖会话
    /// </summary>
    Task WriteAsync(OnlineUserSession session);

    /// <summary>
    /// 读取会话
    /// </summary>
    Task<OnlineUserSession?> ReadAsync(long tenantId, long userId);

    /// <summary>
    /// 刷新最后活跃时间
    /// </summary>
    Task RefreshAsync(long tenantId, long userId);

    /// <summary>
    /// 移除会话
    /// </summary>
    Task RemoveAsync(long tenantId, long userId);

    /// <summary>
    /// 分页查询
    /// </summary>
    Task<PagedList<OnlineUserSession>> PageAsync(int page, int pageSize, long? tenantId, string? userName = null);
}
