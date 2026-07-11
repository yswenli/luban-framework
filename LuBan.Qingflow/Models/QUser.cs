/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： QUser
*版本号： V1.0.0.0
*唯一标识：aae50a6a-9ad8-4d95-aaea-9b887e9fb7a2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/25 13:30:47
*描述：
*
*=================================================
*修改标记
*修改时间：2025/3/25 13:30:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;

/// <summary>
/// 轻流用户信息
/// </summary>
[DataContract]
public class QUser
{
    /// <summary>
    /// 区域代码
    /// </summary>
    [DataMember(Name = "areaCode")]
    public string AreaCode { get; set; }

    /// <summary>
    /// 是否活跃
    /// </summary>
    [DataMember(Name = "beingActive")]
    public bool BeingActive { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    [DataMember(Name = "beingDisabled")]
    public bool BeingDisabled { get; set; }

    /// <summary>
    /// 自定义部门
    /// </summary>
    [DataMember(Name = "customDepartment")]
    public List<int> CustomDepartment { get; set; }

    /// <summary>
    /// 自定义角色
    /// </summary>
    [DataMember(Name = "customRole")]
    public List<int> CustomRole { get; set; }

    /// <summary>
    /// 部门
    /// </summary>
    [DataMember(Name = "department")]
    public List<int> Department { get; set; }

    /// <summary>
    /// 电子邮件
    /// </summary>
    [DataMember(Name = "email")]
    public string Email { get; set; }

    /// <summary>
    /// 头像图片
    /// </summary>
    [DataMember(Name = "headImg")]
    public string HeadImg { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [DataMember(Name = "mobileNum")]
    public string MobileNum { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary>
    /// 选项ID
    /// </summary>
    [DataMember(Name = "optionId")]
    public int OptionId { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    [DataMember(Name = "role")]
    public List<int> Role { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [DataMember(Name = "userId")]
    public string UserId { get; set; }
}
