/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： ISwaggerDocGenerator
*版本号： V1.0.0.0
*唯一标识：035f5e05-afe3-455b-9093-5a3b927f25bd
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 11:15:02
*描述：ISwaggerDocGenerator
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 11:15:02
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：ISwaggerDocGenerator
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger.Doc;

/// <summary>
/// ISwaggerDocGenerator
/// </summary>
public interface ISwaggerDocGenerator
{
    /// <summary>
    /// 获取Swagger流文件
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    byte[] GetSwaggerDocContent(string name);
    /// <summary>
    /// 获取js sdk
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    byte[] GetSwaggerJsSdk(string name);
    /// <summary>
    /// 获取Swagger MarkDown源代码
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    string GetSwaggerDoc(string name);
}
