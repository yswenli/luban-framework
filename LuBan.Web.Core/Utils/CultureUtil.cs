/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Utils
*文件名： CultureUtil
*版本号： V1.0.0.0
*唯一标识：77ac95ad-9681-4214-b624-1a16b1ef1ed8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/21 16:10:26
*描述：全局文化信息工具类
*
*=================================================
*修改标记
*修改时间：2025/7/21 16:10:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：全局文化信息工具类
*
*****************************************************************************/
namespace LuBan.Web.Core.Utils;

/// <summary>
/// 全局文化信息工具类
/// </summary>
public static class CultureUtil
{
    /// <summary>
    /// 指定应用程序的默认文化信息
    /// </summary>
    /// <param name="services"></param>
    /// <param name="cultureName"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void SetCurrentCulture(this IServiceCollection services, string cultureName = "zh-CN")
    {
        try
        {
            ConsoleUtil.WriteLineWithCount("指定应用程序的默认文化信息:zh-CN", color: ConsoleColor.Green);

            var cultureInfo = new CultureInfo(cultureName)
            {
                DateTimeFormat =
                {
                    DateSeparator = "-",
                    TimeSeparator = ":",
                    LongDatePattern = "yyyy-MM-dd hh:mm:ss",
                    ShortDatePattern = "yyyy-MM-dd",
                    FullDateTimePattern = "yyyy-MM-dd HH:mm:ss",
                    LongTimePattern = "HH:mm:ss"
                }
            };
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(cultureInfo);
                options.SupportedCultures = [cultureInfo];
                options.SupportedUICultures = [cultureInfo];
            });
        }
        catch (Exception ex)
        {
            Logger.Warn("CultureUtil.SetCurrentCulture", new InvalidOperationException($"Failed to set culture to '{cultureName}'.", ex));
        }
    }


}
