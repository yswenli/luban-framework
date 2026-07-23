/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Orm
*文件名： MapsterExtension
*版本号： V1.0.0.0
*唯一标识：2d21b78b-a477-4f62-95d6-100e7f93ca15
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/4 16:06:57
*描述：Mapster扩展
*
*=================================================
*修改标记
*修改时间：2024/11/4 16:06:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Mapster扩展
*
*****************************************************************************/

namespace LuBan.Orm;

/// <summary>
/// Mapster扩展,
/// https://www.cnblogs.com/qiqigou/p/13696669.html
/// </summary>
public static class MapsterExtension
{
    /// <summary>
    /// Mapster全局添加对象继承IRegister映射
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns></returns>
    public static IServiceCollection AddObjectMapper(this IServiceCollection services)
    {
        ConsoleUtil.WriteLineWithCount("正在注册Mapster的全局对象映射", color: ConsoleColor.Green);
        // 获取全局映射配置
        var config = TypeAdapterConfig.GlobalSettings;

        // 扫描所有继承IRegister 接口的对象映射配置
        var assemblies = RuntimeUtil.GetAssemblies();
        if (assemblies != null && assemblies.Count > 0) config.Scan([.. assemblies]);

        // 配置默认全局映射（忽略大小写敏感）
        config.Default
              .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
              .PreserveReference(true);

        // 配置支持依赖注入
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }



    /// <summary>
    /// 从源中转换为目标
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static TDestination ConvertTo<TDestination>(this object source)
    {
        return source.Adapt<TDestination>();
    }


    /// <summary>
    /// 从目标Model中填充源Model，
    /// 含从源Model中填充目标实体
    /// </summary>
    /// <typeparam name="SModel"></typeparam>
    /// <typeparam name="DModel"></typeparam>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    public static SModel FillFrom<SModel, DModel>(this SModel source, DModel destination, bool ignoreNull = true)
    {
        var config = new TypeAdapterConfig();
        config.ForType<SModel, DModel>()
            .IgnoreNullValues(ignoreNull);
        return destination.Adapt(source, config);
    }

    /// <summary>
    /// 从目标Entity中填充源Entity
    /// </summary>
    /// <typeparam name="SEntity"></typeparam>
    /// <typeparam name="DEntity"></typeparam>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="ignoreNull"></param>
    /// <param name="ignoreCreateUser"></param>
    /// <returns></returns>
    public static SEntity FillFromEntity<SEntity, DEntity>(this SEntity source,
        DEntity destination,
        bool ignoreNull = true,
        bool ignoreCreateUser = true)
        where SEntity : EntityBase
        where DEntity : EntityBase
    {
        var config = new TypeAdapterConfig();
        config.ForType<SEntity, DEntity>()
            .IgnoreNullValues(ignoreNull)
            .IgnoreIf((a, b) => ignoreCreateUser,
            (e) => e.Id,
            (e) => e.CreateTime,
            (e) => e.CreateUserId!,
            (e) => e.CreateUserName!);
        return destination.Adapt(source, config);
    }
}
