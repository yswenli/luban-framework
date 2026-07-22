/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： SysUser
*版本号： V1.0.0.0
*唯一标识：997be648-d25a-41a4-b326-d2b0397de626
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:19:56
*描述：系统用户表
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:19:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统用户表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;


/// <summary>
/// 系统用户表
/// </summary>
[SugarTable("db_user", "系统用户表")]
[SysTable]
public class DbUser : EntityeDataScoreBase
{
    /// <summary>
    /// 账号
    /// </summary>
    [SugarColumn(ColumnDescription = "账号", Length = 32)]
    [Required(ErrorMessage = "账号不能为空"), MaxLength(32, ErrorMessage = "账号长度不能超过32")]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [SugarColumn(ColumnDescription = "密码", Length = 512)]
    [MaxLength(512)]
    [System.Text.Json.Serialization.JsonIgnore]
    [JsonIgnore]
    public string Password { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "真实姓名", Length = 32)]
    [MaxLength(32, ErrorMessage = "真实姓名长度不能超过32")]
    public string RealName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称", Length = 32)]
    [MaxLength(32, ErrorMessage = "昵称长度不能超过32")]
    public string? NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [SugarColumn(ColumnDescription = "头像", Length = 512)]
    [MaxLength(512, ErrorMessage = "头像长度不能超过512")]
    public string? Avatar { get; set; }

    /// <summary>
    /// 性别-男_1、女_2
    /// </summary>
    [SugarColumn(ColumnDescription = "性别")]
    public EnumGender Sex { get; set; } = EnumGender.Male;

    /// <summary>
    /// 出生日期
    /// </summary>
    [SugarColumn(ColumnDescription = "出生日期")]
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [SugarColumn(ColumnDescription = "手机号码", Length = 16)]
    [MaxLength(16, ErrorMessage = "手机号码长度不能超过16")]
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [SugarColumn(ColumnDescription = "邮箱", Length = 64)]
    [MaxLength(64, ErrorMessage = "邮箱长度不能超过64")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [SugarColumn(ColumnDescription = "地址", Length = 256)]
    [MaxLength(256, ErrorMessage = "地址长度不能超过256")]
    public string? Address { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public EnumEnableStatus Status { get; set; } = EnumEnableStatus.Enable;

    /// <summary>
    /// 直属机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "直属机构Id")]
    public long OrgId { get; set; }

    /// <summary>
    /// 直属机构
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrgId))]
    public DbOrg Organization { get; set; }

    /// <summary>
    /// 直属主管Id
    /// </summary>
    [SugarColumn(ColumnDescription = "直属主管Id")]
    public long? ManagerUserId { get; set; }

    /// <summary>
    /// 直属主管
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ManagerUserId))]
    public DbUser ManagerUser { get; set; }

    /// <summary>
    /// 职位Id
    /// </summary>
    [SugarColumn(ColumnDescription = "职位Id")]
    public long PosId { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PosId))]
    public DbPos Position { get; set; }


    /// <summary>
    /// 角色映射列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(DbUserRole.UserId), nameof(Id))]
    public List<DbUserRole> UserRoles { get; set; }

    /// <summary>
    /// 最新登录Ip
    /// </summary>
    [SugarColumn(ColumnDescription = "最新登录Ip", Length = 256)]
    [MaxLength(256, ErrorMessage = "最新登录Ip长度不能超过256")]
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// 最新登录地点
    /// </summary>
    [SugarColumn(ColumnDescription = "最新登录地点", Length = 256)]
    [MaxLength(256, ErrorMessage = "最新登录地点长度不能超过256")]
    public string? LastLoginAddress { get; set; }

    /// <summary>
    /// 最新登录时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最新登录时间")]
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 最新登录设备
    /// </summary>
    [SugarColumn(ColumnDescription = "最新登录设备", Length = 2048)]
    [MaxLength(2048, ErrorMessage = "最新登录设备长度不能超过2048")]
    public string? LastLoginDevice { get; set; }

    /// <summary>
    /// 电子签名
    /// </summary>
    [SugarColumn(ColumnDescription = "电子签名", Length = 512)]
    [MaxLength(512, ErrorMessage = "电子签名长度不能超过512")]
    public string? Signature { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256)]
    [MaxLength(256, ErrorMessage = "备注长度不能超过256")]
    public string? Remark { get; set; }
}
