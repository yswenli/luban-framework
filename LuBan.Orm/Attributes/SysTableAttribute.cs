/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Attributes
*文件名： SysTableAttribute
*版本号： V1.0.0.0
*唯一标识：6d1e7bc9-ec7f-490e-b7b7-eaa5c80772c4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 19:30:42
*描述：系统表特性
*
*=================================================
*修改标记
*修改时间：2023/12/1 19:30:42
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统表特性
*
*****************************************************************************/
namespace LuBan.Orm.Attributes
{
    /// <summary>
    /// 系统表特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class SysTableAttribute : Attribute
    {
    }
}
