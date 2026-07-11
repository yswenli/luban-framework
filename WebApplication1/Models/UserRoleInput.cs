/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Models
*文件名： UserRoleInput
*版本号： V1.0.0.0
*唯一标识：aa1476f8-bd3f-44de-a26b-7f7db419d2b4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/14 10:26:17
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/14 10:26:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace WebApplication1.Models;

/// <summary>
/// 授权用户角色
/// </summary>
public class UserRoleInput
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 角色Id集合
    /// </summary>
    public List<long> RoleIdList { get; set; }
}


/// <summary>
/// 授权角色菜单
/// </summary>
public class RoleMenuInput : BaseIdInput
{
    /// <summary>
    /// 菜单Id集合
    /// </summary>
    public List<long> MenuIdList { get; set; }
}