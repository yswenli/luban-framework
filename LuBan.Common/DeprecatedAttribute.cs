/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： DeprecatedAttribute
*版本号： V1.0.0.0
*唯一标识：66e46bb8-9865-4dff-8700-66fc356da9ba
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/4/29 13:38:49
*描述：
*
*=====================================================================
*修改标记
*修改时间：2021/4/29 13:38:49
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 标记不建议使用的程序元素
/// </summary>
[Serializable]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
[ComVisible(true)]
public sealed class DeprecatedAttribute : Attribute
{
    /// <summary>
    /// 标记不建议使用的程序元素
    /// </summary>
    public DeprecatedAttribute()
    {

    }
    /// <summary>
    /// 描述可选的变通方法的文本字符串
    /// </summary>
    /// <param name="message"></param>
    public DeprecatedAttribute(string message)
    {

    }


}
