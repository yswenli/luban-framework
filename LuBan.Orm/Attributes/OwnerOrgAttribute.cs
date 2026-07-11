/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Attributes
*文件名： CustomAttribute
*版本号： V1.0.0.0
*唯一标识：17ac920f-0788-4eee-9480-8b747d3201cd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/31 11:53:25
*描述：所属机构数据权限
*
*=================================================
*修改标记
*修改时间：2024/7/31 11:53:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：所属机构数据权限
*
*****************************************************************************/
namespace LuBan.Orm.Attributes;


/// <summary>
/// 所属机构数据权限
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class OwnerOrgAttribute : Attribute
{
}
