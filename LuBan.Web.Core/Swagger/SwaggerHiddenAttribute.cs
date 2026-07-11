/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerHiddenAttribute
*版本号： V1.0.0.0
*唯一标识：0732cf00-88f2-4354-9ec1-c2dbc718e0bf
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 14:07:23
*描述：隐藏swagger接口特性标识
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 14:07:23
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：隐藏swagger接口特性标识
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger
{
    /// <summary>
    /// 隐藏swagger接口特性标识
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SwaggerHiddenAttribute : Attribute
    {

    }
}
