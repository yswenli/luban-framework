/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Attributes
*文件名： IncreTableAttribute
*版本号： V1.0.0.0
*唯一标识：ebc4f9dc-490a-47a8-aeb8-475375ce42a3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 19:29:45
*描述：增量表特性
*
*=================================================
*修改标记
*修改时间：2023/12/1 19:29:45
*修改人： yswenli
*版本号： V1.0.0.0
*描述：增量表特性
*
*****************************************************************************/


namespace LuBan.Orm.Attributes
{
    /// <summary>
    /// 增量表特性
    /// </summary>    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class IncreTableAttribute : Attribute
    {

    }
}
