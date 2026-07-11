/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Models
*文件名： DbOrgInput
*版本号： V1.0.0.0
*唯一标识：c216a081-8803-4fa6-b10f-76da95205a2d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/14 11:06:44
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/14 11:06:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace WebApplication1.Models;


public class OrgInput : BaseIdInput
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 机构类型
    /// </summary>
    public string Type { get; set; }
}

public class AddOrgInput : DbOrg
{

}

public class UpdateOrgInput : AddOrgInput
{
}

public class DeleteOrgInput : BaseIdInput
{
}
