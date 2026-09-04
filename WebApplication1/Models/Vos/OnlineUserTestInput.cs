/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Vos
*文件名： OnlineUserTestInput.cs
*版本号： V1.0.0.0
*唯一标识：7b17612a-0652-4dd2-bb40-4e900676f0c8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：OnlineUserTestInput 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OnlineUserTestInput 类
*
*****************************************************************************/

namespace WebApplication1.Models.Vos;

/// <summary>
/// 写入在线用户会话测试输入
/// </summary>
public class WriteOnlineUserTestInput
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    public string? UserName { get; set; }
}

/// <summary>
/// 分页查询在线用户测试输入
/// </summary>
public class PageOnlineUserTestInput : BasePageInput
{
    /// <summary>
    /// 租户Id（可选，不传则查全部）
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 用户姓名（可选）
    /// </summary>
    public string? UserName { get; set; }
}
