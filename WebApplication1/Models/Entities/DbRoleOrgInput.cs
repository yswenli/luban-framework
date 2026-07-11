/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Models
*文件名： DbRoleOrgInput
*版本号： V1.0.0.0
*唯一标识：1ff1a38a-cbf3-40ae-b9b8-fb3779251d12
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/14 11:09:44
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/14 11:09:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace WebApplication1.Models.Entities;


/// <summary>
/// 授权角色机构
/// </summary>
public class RoleOrgInput : BaseIdInput
{
    /// <summary>
    /// 数据范围
    /// </summary>
    public int DataScope { get; set; }

    /// <summary>
    /// 机构Id集合
    /// </summary>
    public List<long> OrgIdList { get; set; }
}