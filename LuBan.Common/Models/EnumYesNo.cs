/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Models
*文件名： EnumYesNo
*版本号： V1.0.0.0
*唯一标识：ac2e1b33-988d-4bde-804e-65efe2fb0622
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:19:21
*描述：是否枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:19:21
*修改人： yswenli
*版本号： V1.0.0.0
*描述：是否枚举
*
*****************************************************************************/
namespace LuBan.Common.Models;

/// <summary>
/// 是否枚举
/// </summary>
[Description("是否枚举")]
public enum EnumYesNo
{
    /// <summary>
    /// 是
    /// </summary>
    [Description("是")]
    Y = 1,

    /// <summary>
    /// 否
    /// </summary>
    [Description("否")]
    N = 2
}
