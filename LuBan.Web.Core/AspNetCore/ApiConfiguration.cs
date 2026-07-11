/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： ApiConfiguration
*版本号： V1.0.0.0
*唯一标识：17948d22-5c33-4ce6-9eb4-87bddc7d4bb4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/7 13:41:12
*描述：接口页面相关服务项配置
*
*=================================================
*修改标记
*修改时间：2024/8/7 13:41:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：接口页面相关服务项配置
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 接口页面相关服务项配置
/// </summary>
public static class ApiConfiguration
{
    /// <summary>
    /// 加载接口页面相关服务项
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostingOptions"></param>
    public static void SetControllers(this IServiceCollection services, HostingOptions hostingOptions)
    {
        ConsoleUtil.WriteLineWithCount("正在配置基于aspnetcore框架的参数", color: ConsoleColor.Green);
        services.AddControllers(options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressConsumesConstraintForFormFileParameters = true;
            options.SuppressInferBindingSourcesForParameters = true;
            options.SuppressModelStateInvalidFilter = true;
            //options.ClientErrorMapping[404].Link = "https://yswenli.cnblogs.com/404";
        })
        .AddJsonOptions(options =>
        {
            // 避免过度转义非ASCII字符（如中文、特殊符号），同时保持基本安全
            options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(
                UnicodeRanges.BasicLatin,
                UnicodeRanges.CjkUnifiedIdeographs,
                UnicodeRanges.CjkSymbolsandPunctuation
            );

            //格式化输出内容
            options.JsonSerializerOptions.WriteIndented = true;
            //命名规则
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = null;

            //允许字符串与数值类型互转（如 "25" → int，123 → string）
            options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            //允许空值与非空类型的兼容（可选）
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            // 允许JSON末尾有多余逗号
            options.JsonSerializerOptions.AllowTrailingCommas = true;

            //自定义输出的时间格式
            options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());

            //枚举类型处理：支持字符串与数值互转
            //options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            //引用循环处理：避免循环引用导致的序列化失败
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        //支持razor页面
        services.AddRazorPages();

        //支持signalr hub
        var signalROptions = hostingOptions.AppOptions.SignalROptions;
        if (hostingOptions.AppOptions.EnableSignalR && signalROptions != null)
        {
            services.AddSignalR((option) =>
            {
                option.HandshakeTimeout = TimeSpan.FromSeconds(signalROptions.HandshakeTimeout);
                option.KeepAliveInterval = TimeSpan.FromSeconds(signalROptions.KeepAliveInterval);
                option.ClientTimeoutInterval = TimeSpan.FromSeconds(signalROptions.FreeTimeout);
                option.MaximumParallelInvocationsPerClient = signalROptions.ParallelCount;
                option.MaximumReceiveMessageSize = signalROptions.MaximumReceiveMessageSize;
            });
        }

        //注入accessor
        services.AddHttpContextAccessor();

        //mvc框架级统一配置
        services.AddMvc(option =>
        {
            //全局统一返回值
            option.Filters.Add<ApiResultConvertionAttribute>();
            //全局日志
            option.Filters.Add<ApiLogAttribute>();
            //接口请求参数检查
            option.Filters.Add<InputArgsValidateActionFilterAttribute>();
            //启用get数组支持,例如：?a[]=1&a[]=2&a[]=3;但是一般的集合还是建议从httppost的FromBody处理
            option.ValueProviderFactories.Add(new JQueryQueryStringValueProviderFactory());
            //启用默认请求谓词和参数的处理
            option.Conventions.Add(new DefaultMethodConvention(services));
        });
    }

    /// <summary>
    /// 配置跨域
    /// </summary>
    /// <param name="services"></param>
    public static void SetDefaultCors(this IServiceCollection services)
    {
        ConsoleUtil.WriteLineWithCount("正在设置跨域", color: ConsoleColor.Green);
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("x-elsa-workflow-instance-id");
                    //.AllowCredentials();
                });
        });
    }
}
