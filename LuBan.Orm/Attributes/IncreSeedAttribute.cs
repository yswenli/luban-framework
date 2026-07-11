/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Attributes
*文件名： IncreSeedAttribute
*版本号： V1.0.0.0
*唯一标识：c62cb182-38b9-4539-807d-55f2baa8587f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 11:22:17
*描述：增量种子特性
*
*=================================================
*修改标记
*修改时间：2023/12/4 11:22:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：增量种子特性
*
*****************************************************************************/
namespace LuBan.Orm.Attributes;

/// <summary>
/// 增量种子特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class IncreSeedAttribute : Attribute
{
}
