/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerGroupConfigExtension
*版本号： V1.0.0.0
*唯一标识：e750c2ce-ccc0-4b51-90cb-c65ba36a1d06
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/24 18:07:46
*描述：
*
*=================================================
*修改标记
*修改时间：2025/9/24 18:07:46
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;


/// <summary>
/// Swagger分组配置
/// </summary>
public static class SwaggerGroupConfigExtension
{
    static Dictionary<string, SwaggerGroupInfo> _dic = new Dictionary<string, SwaggerGroupInfo>();

    /// <summary>
    /// Swagger分组配置
    /// </summary>
    static SwaggerGroupConfigExtension()
    {
        _dic.Add("default", new SwaggerGroupInfo() { GroupName = "default", Title = "Default-未分组接口", Version = "1.0" });
        _dic.Add("admin", new SwaggerGroupInfo() { GroupName = "admin", Title = "Admin-管理端接口", Version = "1.0" });
        _dic.Add("mobile", new SwaggerGroupInfo() { GroupName = "mobile", Title = "Mobile-移动端接口", Version = "1.0" });
        _dic.Add("internal", new SwaggerGroupInfo() { GroupName = "internal", Title = "Internal-内部服务接口", Version = "1.0" });
        _dic.Add("open", new SwaggerGroupInfo() { GroupName = "open", Title = "Open-开放服务接口", Version = "1.0" });
    }

    /// <summary>
    /// 获取默认分组
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static Dictionary<string, SwaggerGroupInfo> Default(this SwaggerGroupInfo config)
    {
        lock (_dic)
        {
            return new Dictionary<string, SwaggerGroupInfo>(_dic);
        }
    }
}
