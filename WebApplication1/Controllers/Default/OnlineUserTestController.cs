/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Default
*文件名： OnlineUserTestController.cs
*版本号： V1.0.0.0
*唯一标识：0eca0e0e-b7cc-49a3-8dd8-cc4d82fc6d3a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：OnlineUserTestController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OnlineUserTestController 控制器
*
*****************************************************************************/

using LuBan.Web.Core.OnlineUser;

using WebApplication1.Models.Vos;

namespace WebApplication1.Controllers.Default;

/// <summary>
/// 在线用户核心操作测试接口（免登录）
/// </summary>
[Route("api/test/onlineuser")]
[AllowAnonymous, AllowAccess]
public class OnlineUserTestController : BaseApiController
{
    /// <summary>
    /// 写入在线用户会话
    /// </summary>
    [DisplayName("写入会话"), HttpPost("write")]
    public async Task<object> WriteAsync([FromBody] WriteOnlineUserTestInput input)
    {
        var session = new OnlineUserSession
        {
            UserId = input.UserId,
            TenantId = input.TenantId,
            UserName = input.UserName ?? "",
            LoginTime = DateTime.UtcNow,
            LastActiveTime = DateTime.UtcNow,
            Ip = "127.0.0.1",
            Device = "TestDevice"
        };
        await OnlineUserStoreProvider.Store.WriteAsync(session);
        
        // 写入后立即读取验证
        var readSession = await OnlineUserStoreProvider.Store.ReadAsync(input.TenantId, input.UserId);
        
        return new { 
            Message = "写入成功", 
            WrittenSession = session,
            ReadAfterWrite = readSession,
            ReadSuccess = readSession != null
        };
    }

    /// <summary>
    /// 读取在线用户会话
    /// </summary>
    [DisplayName("读取会话"), HttpGet("read")]
    public async Task<object> ReadAsync([FromQuery] long tenantId, [FromQuery] long userId)
    {
        var session = await OnlineUserStoreProvider.Store.ReadAsync(tenantId, userId);
        return new { Session = session, Found = session != null };
    }

    /// <summary>
    /// 刷新在线用户活跃时间
    /// </summary>
    [DisplayName("刷新活跃时间"), HttpPost("refresh")]
    public async Task<object> RefreshAsync([FromQuery] long tenantId, [FromQuery] long userId)
    {
        await OnlineUserStoreProvider.Store.RefreshAsync(tenantId, userId);
        return new { Message = "刷新成功", TenantId = tenantId, UserId = userId };
    }

    /// <summary>
    /// 移除在线用户会话
    /// </summary>
    [DisplayName("移除会话"), HttpPost("remove")]
    public async Task<object> RemoveAsync([FromQuery] long tenantId, [FromQuery] long userId)
    {
        await OnlineUserStoreProvider.Store.RemoveAsync(tenantId, userId);
        return new { Message = "移除成功", TenantId = tenantId, UserId = userId };
    }

    /// <summary>
    /// 分页查询在线用户
    /// </summary>
    [DisplayName("分页查询"), HttpPost("page")]
    public async Task<object> PageAsync([FromBody] PageOnlineUserTestInput input)
    {
        var result = await OnlineUserStoreProvider.Store.PageAsync(
            input.Page, input.PageSize, input.TenantId, input.UserName);
        return new { Total = result.Total, Items = result.Items };
    }
}
