/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Web.Core.Attributes
*文件名： IgnoreDataScopePermissionAttribute
*版本号： V1.0.0.0
*唯一标识：86f234e6-c9d3-4449-b62e-16a2c5457a5e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/12 11:40:00
*描述：忽略验证数据权限
*
*=================================================
*修改标记
*修改时间：2024/11/12 11:40:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：忽略验证数据权限
*
*****************************************************************************/

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 忽略验证数据权限
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class IgnoreDataScopePermissionAttribute : Attribute
{

}
