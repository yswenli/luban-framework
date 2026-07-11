/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：
*文件名： 
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：开放接口身份表
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：开放接口身份表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 开放接口身份表
/// </summary>
[SugarTable("db_open_access", "开放接口身份表")]
[SysTable]
public class DbOpenAccess : EntityBase
{
    /// <summary>
    /// 身份标识
    /// </summary>
    [SugarColumn(ColumnDescription = "身份标识", Length = 128)]
    [Required, MaxLength(128)]
    public string AccessKey { get; set; }

    /// <summary>
    /// 密钥
    /// </summary>
    [SugarColumn(ColumnDescription = "密钥", Length = 256)]
    [Required, MaxLength(256)]
    public string AccessSecret { get; set; }

    /// <summary>
    /// 绑定租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定租户Id")]
    public long BindTenantId { get; set; }

    /// <summary>
    /// 绑定租户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(BindTenantId)), JsonIgnore]
    public DbTenant BindTenant { get; set; }

    /// <summary>
    /// 绑定用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定用户Id")]
    public long BindUserId { get; set; }

    /// <summary>
    /// 绑定用户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(BindUserId)), JsonIgnore]
    public DbUser BindUser { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启用")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// RefreshToken
    /// </summary>
    [SugarColumn(ColumnDescription = "RefreshToken", Length = 256)]
    public string? RefreshToken { get; set; }
}