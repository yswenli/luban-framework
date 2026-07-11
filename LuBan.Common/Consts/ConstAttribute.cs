/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Consts
*文件名： ConstAttribute
*版本号： V1.0.0.0
*唯一标识：7cb4db26-9963-499e-8f49-d455515327bd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 15:21:35
*描述：常量特性
*
*=================================================
*修改标记
*修改时间：2023/12/4 15:21:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：常量特性
*
*****************************************************************************/
namespace LuBan.Common.Consts;

/// <summary>
/// 常量特性
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ConstAttribute : Attribute
{
    /// <summary>
    /// 常量值
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 常量特性
    /// </summary>
    /// <param name="name"></param>
    public ConstAttribute(string name)
    {
        Name = name;
    }
}
