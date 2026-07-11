/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerGroupConfig
*版本号： V1.0.0.0
*唯一标识：1861fee3-ca77-4cf4-94cf-e1633e47890b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/23 17:13:15
*描述：Swagger分组配置
*
*=================================================
*修改标记
*修改时间：2024/9/23 17:13:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Swagger分组配置
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;

/// <summary>
/// Swagger分组配置
/// </summary>
public class SwaggerGroupInfo
{
    /// <summary>
    /// 分组名称
    /// </summary>
    public string GroupName { get; set; }
    /// <summary>
    /// 分组标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 默认分组
    /// </summary>
    public static Dictionary<string, SwaggerGroupInfo> Default
    {
        get
        {
            return new SwaggerGroupInfo().Default();
        }
    }
}
