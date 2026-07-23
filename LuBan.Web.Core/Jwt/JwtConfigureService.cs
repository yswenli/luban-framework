/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.JWT
*文件名： JwtConfigureService
*版本号： V1.0.0.0
*唯一标识：e59171c1-bc7b-4bb0-8ecb-52c6cf9a468f
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 13:40:46
*描述：Startup 中配置jwt
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 13:40:46
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：Startup 中配置jwt
*
*****************************************************************************/
namespace LuBan.Web.Core.Jwt;

/// <summary>
/// Startup 中配置jwt
/// </summary>
public static class JwtConfigureService
{
    /// <summary>
    /// 配置jwt
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="hostingOptions"></param>
    public static void AddJwt(this IServiceCollection services, IConfiguration configuration, HostingOptions hostingOptions)
    {
        ConsoleUtil.WriteLineWithCount("正在初始化jwt校验", color: ConsoleColor.Green);
        var jwtConfig = hostingOptions.AppOptions.JwtAuthConfig;

        if (string.IsNullOrWhiteSpace(jwtConfig.Secret))
            throw new InvalidOperationException("JWT Secret 未配置，请在 appsettings.json 中设置 tokenManagement.Secret");

        byte[] keyBytes;
        var secret = jwtConfig.Secret;
        if (secret.StartsWith("base64:", StringComparison.OrdinalIgnoreCase))
            secret = secret["base64:".Length..];
        keyBytes = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = true;
            x.SaveToken = false;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
            x.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Logger.Warn("JWT 验证失败", context.Exception, context.Request.GetJwtToken());
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.GetJwtToken();
                    return Task.CompletedTask;
                }
            };
        });

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationMiddlewareResultHandler>();
    }
}
