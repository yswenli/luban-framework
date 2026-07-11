/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Attributes
*文件名： LogTableAttribute
*版本号： V1.0.0.0
*唯一标识：78c65531-d20a-422b-b975-ce39a08c85ae
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 19:31:17
*描述：日志表特性
*
*=================================================
*修改标记
*修改时间：2023/12/1 19:31:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志表特性
*
*****************************************************************************/
namespace LuBan.Orm.Attributes
{
    /// <summary>
    /// 日志表特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class LogTableAttribute : Attribute
    {
    }
}
