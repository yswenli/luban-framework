/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Models
*文件名： DbRoleInput
*版本号： V1.0.0.0
*唯一标识：eef298bb-7a78-4ab8-a45a-a4b525ea2aa8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/14 11:10:58
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/14 11:10:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace WebApplication1.Models;


public class RoleInput : BaseIdInput
{
    /// <summary>
    /// 状态
    /// </summary>
    public virtual EnumEnableStatus Status { get; set; }
}

public class PageRoleInput : BasePageInput
{
    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    public virtual string Code { get; set; }
}

public class AddRoleInput : DbRole
{
    /// <summary>
    /// 菜单Id集合
    /// </summary>
    public List<long> MenuIdList { get; set; }
}

public class UpdateRoleInput : AddRoleInput
{
}

public class DeleteRoleInput : BaseIdInput
{
}



/// <summary>
/// 角色列表输出参数
/// </summary>
public class RoleOutput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; }
}