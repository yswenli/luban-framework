/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Models
*文件名： UserInput
*版本号： V1.0.0.0
*唯一标识：5f25c6a1-077b-4007-9390-810b60e5186d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/14 10:21:16
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/14 10:21:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace WebApplication1.Models;


public class UserInput : BaseIdInput
{
    /// <summary>
    /// 状态
    /// </summary>
    public EnumEnableStatus Status { get; set; }
}

public class PageUserInput : BasePageInput
{
    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string RealName { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// 查询时所选机构Id
    /// </summary>
    public long OrgId { get; set; }
}

public class AddUserInput : DbUser
{
    /// <summary>
    /// 角色集合
    /// </summary>
    public List<long> RoleIdList { get; set; }

    /// <summary>
    /// 扩展机构集合
    /// </summary>
    public List<DbUserExtOrg> ExtOrgIdList { get; set; }

    public EnumUserAuditStatus? AuditStatus { get; set; } = EnumUserAuditStatus.Waiting;
    public string? Nation { get; set; }
    public EnumCardType CardType { get; set; }
    public string? IdCardNum { get; set; }
    public EnumCultureLevel CultureLevel { get; set; }
    public string? PoliticalOutlook { get; set; }
    public string? College { get; set; }
    [Phone]
    public string? OfficePhone { get; set; }

    public string? EmergencyContact { get; set; }
    [Phone]
    public string? EmergencyPhone { get; set; }

    /// <summary>
    /// 紧急联系人地址
    /// </summary>
    public string? EmergencyAddress { get; set; }

    /// <summary>
    /// 学生证/医师执业证书
    /// </summary>
    public string? Introduction { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    public string? JobNum { get; set; }

    /// <summary>
    /// 职级
    /// </summary>
    public string? PosLevel { get; set; }
    /// <summary>
    /// 擅长领域
    /// </summary>
    public string? Expertise { get; set; }

    /// <summary>
    /// 医院级别
    /// </summary>
    public string? OfficeZone { get; set; }

    /// <summary>
    /// 专业背景（科室，部门）
    /// </summary>
    public string? Office { get; set; }

    /// <summary>
    /// 入学年份
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? JoinDate { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public string? City { get; set; }
}

public class UpdateUserInput : AddUserInput
{

}

public class PageUserOutput : UpdateUserInput
{

}



public class DeleteUserInput : BaseIdInput
{
    /// <summary>
    /// 机构Id
    /// </summary>
    public long OrgId { get; set; }
}



public class ResetPwdUserInput : BaseIdInput
{
}

public class ChangePwdInput
{
    /// <summary>
    /// 当前密码
    /// </summary>
    [Required(ErrorMessage = "当前密码不能为空")]
    public string PasswordOld { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "密码需要大于5个字符")]
    public string PasswordNew { get; set; }
}