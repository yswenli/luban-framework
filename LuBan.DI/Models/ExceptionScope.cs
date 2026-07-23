/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Models
*文件名： ExceptionScope
*版本号： V1.0.0.0
*唯一标识：e48dee5e-2a6e-4e64-be8d-14f00ea2c603
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/22 14:07:05
*描述：作用域共享的异常信息
*
*=================================================
*修改标记
*修改时间：2023/12/22 14:07:05
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作用域共享的异常信息
*
*****************************************************************************/
namespace LuBan.DI.Models;

/// <summary>
/// 作用域共享的异常信息
/// </summary>
public class ExceptionScope : IScoped
{
    /// <summary>
    /// 异常
    /// </summary>
    public Exception? Exception { get; set; }
}
