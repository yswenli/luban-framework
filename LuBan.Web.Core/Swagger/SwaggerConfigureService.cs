/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerConfigureService
*版本号： V1.0.0.0
*唯一标识：81450c9c-daeb-4e11-b892-df8527f8a971
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 14:06:59
*描述：Swagger Doc配置
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 14:06:59
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：Swagger Doc配置
*
*****************************************************************************/

namespace LuBan.Web.Core.Swagger;

#nullable disable warnings

/// <summary>
/// Swagger Doc配置
/// </summary>
public static class SwaggerConfigureService
{
    /// <summary>
    /// 设置配置swagger
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostingOptions"></param>
    public static void AddSwaggerDocGen(this IServiceCollection services, HostingOptions hostingOptions)
    {
        if (hostingOptions.AppOptions.DisableSwagger)
        {
            return;
        }
        ConsoleUtil.WriteLineWithCount("正在加载Swagger API文档", color: ConsoleColor.Green);

        //生成swagger.json文档
        services.AddSwaggerGen(options =>
        {
            var servers = options.SwaggerGeneratorOptions.Servers;

            options.SwaggerGeneratorOptions.Servers.Add(new OpenApiServer
            {
                Url = hostingOptions.Domain,
                Description = "配置的域名地址"
            });

            //带或不带参数启动            
            var docUrl = $"{WebApp.ReadArgs<string>("urls")}";
            var sdkUrl = string.Empty;
            if (docUrl.IsNullOrEmpty())
            {
                docUrl = hostingOptions.AppOptions.Urls[0];
            }
            sdkUrl = docUrl + "/jssdk?documents=";
            docUrl += "/download?documents=";
            //若是带环境参数启动的（一般是在docker中启动时）
            var env = WebApp.ReadArgs<string>("environment");
            if (env.IsNotNullOrEmpty())
            {
                docUrl = $"{hostingOptions.Domain}/download?documents=";
                sdkUrl = $"{hostingOptions.Domain}/jssdk?documents=";
            }

            foreach (var item in SwaggerGroupInfo.Default.Values)
            {
                options.SwaggerDoc(item.GroupName,
                    new OpenApiInfo
                    {
                        Title = item.Title,
                        Version = item.Version,
                        Description = $"{ConfigUtil.GetServiceName()} ，点击<a href='{sdkUrl}{item.GroupName}' target='_blank'>这里</a>下载{item.Title} js sdk",
                        Contact = new OpenApiContact
                        {
                            Email = "yswenli@outlook.com",
                            Name = "Contact",
                            Url = new Uri("https://yswenli.cnblogs.com/"),
                        },
                        License = new OpenApiLicense()
                        {
                            Name = $"⬇️下载 {item.Title}文档",
                            Url = new Uri(docUrl + item.GroupName)
                        }
                    });
            }
            // 注册阻止KeyValuePair<string, StringValues>生成的过滤器
            options.SchemaFilter<ExcludeKeyValuePairSchemaFilter>();
            options.DocumentFilter<RemoveGenericSchemaDocumentFilter>();

            //只加载当前应用程序域中的相关程序集，减少内存占用
            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !string.IsNullOrEmpty(a.FullName))
                .Where(a => 
                    a.FullName.StartsWith("WebApplication1") || 
                    a.FullName.StartsWith("LuBan") ||
                    a.FullName.StartsWith("System")) // 只加载系统核心程序集
                .Distinct();
            
            foreach (var assembly in currentAssemblies)
            {
                if (!hostingOptions.AppOptions.DisableSwagger)
                {
                    try
                    {
                        var xmlPath = assembly.Location.Replace(".dll", ".xml").Replace(".exe", ".xml");
                        if (FileUtil.Exists(xmlPath))
                        {
                            options.IncludeXmlComments(xmlPath, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("swagger加载xml失败", ex);
                    }
                }
            }
            
            options.OrderActionsBy(o => o.ActionDescriptor.DisplayName);



            //Swagger 文档中添加较验            
            var scheme = new OpenApiSecurityScheme()
            {
                Description = "Authorization header. \r\nExample: 'Bearer yswenli'",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Authorization"
                },
                Scheme = "oauth2",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            options.AddSecurityDefinition("Authorization", scheme);
            var requirement = new OpenApiSecurityRequirement
            {
                [scheme] = []
            };
            options.AddSecurityRequirement(requirement);

            //swagger分组
            options.DocInclusionPredicate((docName, apiDescription) =>
            {
                var template = apiDescription.ActionDescriptor.AttributeRouteInfo?.Template ?? string.Empty;
                switch (docName)
                {
                    case "admin":
                        return template.IsNotNullOrEmpty() && template.StartsWith("api/admin/");
                    case "mobile":
                        return template.IsNotNullOrEmpty() && template.StartsWith("api/mobile/");
                    case "internal":
                        return template.IsNotNullOrEmpty() && template.StartsWith("api/internal/");
                    case "open":
                        return template.IsNotNullOrEmpty() && template.StartsWith("api/open/");
                    case "default":
                    default:
                        if (template.IsNullOrEmpty()) return true;
                        else
                        {
                            if (!template.StartsWith("api/admin/")
                            && !template.StartsWith("api/mobile/")
                            && !template.StartsWith("api/internal/")
                            && !template.StartsWith("api/open/"))
                            {
                                return true;
                            }
                        }
                        return false;
                }
            });


            options.CustomSchemaIds(type => type.FullName);

            //添加隐藏的过滤
            options.DocumentFilter<SwaggerApiFilter>();
            options.DocumentFilter<SwaggerLowerCaseFilter>();
            options.OperationFilter<SwaggerFileUploadFilter>();

            //增加接口安全参数
            options.OperationFilter<SwaggerSafeComparisonFilter>();
            options.OperationFilter<SwaggerJwtFilter>();
            //返回控制器和方法名称
            options.CustomOperationIds(apiDesc =>
            {
                var ca = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                if (ca != null)
                {
                    foreach (var meta in ca.EndpointMetadata)
                    {
                        if (meta is HttpMethodMetadata methodMeta)
                        {
                            return $"{ca.ControllerName}_{ca.ActionName}_{string.Join(",", methodMeta.HttpMethods)}";
                        }
                    }

                    return $"{ca.ControllerName}_{ca.ActionName}";
                }

                return "";
            });

        });
        //添加Swagger Gen Newtonsoft Support 支持
        services.AddSwaggerGenNewtonsoftSupport();
        //用于swagger MarkDown生成
        services.AddSwaggerMarkdownDoc();

    }
}
