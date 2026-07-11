/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： ApiResultConvertion
*版本号： V1.0.0.0
*唯一标识：261f9445-8d20-4bf0-b701-3782ef66a8fb
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/1 17:33:17
*描述：接口返回值统一处理转换
*
*=================================================
*修改标记
*修改时间：2024/8/1 17:33:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：接口返回值统一处理转换
*
*****************************************************************************/

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 不进行全局ApiResult转换
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class NoApiResultConvertAtrribute : Attribute
{

}
