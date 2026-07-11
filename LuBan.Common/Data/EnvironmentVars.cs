/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Consts
*文件名： EnvironmentConst
*版本号： V1.0.0.0
*唯一标识：b20f959c-54f3-4028-9613-20fe865784da
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/1/21 17:44:24
*描述：环境常量
*
*=================================================
*修改标记
*修改时间：2025/1/21 17:44:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：环境常量
*
*****************************************************************************/
namespace LuBan.Common.Data;

/// <summary>
/// 环境常量
/// </summary>
public class EnvironmentVars
{
    public const string Dev = "dev";

    public const string Uat = "uat";

    public const string Gray = "gray";

    public const string Prd = "prd";

    /// <summary>
    /// 当前环境
    /// </summary>
    public static string DotNetEnvironment
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (env.IsNullOrEmpty()) return Dev;
            switch (env)
            {
                case "Development":
                    return Dev;
                case "Staging":
                    return Uat;
                case "Production":
                    return Prd;
                default:
                    return env;

            }
        }
    }


    /// <summary>
    /// 当前环境
    /// </summary>
    public static string AspnetcoreEnvironment
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env.IsNullOrEmpty()) return Dev;
            switch (env)
            {
                case "Development":
                    return Dev;
                case "Staging":
                    return Uat;
                case "Production":
                    return Prd;
                default:
                    return env;

            }
        }
    }

    /// <summary>
    /// 获取环境变量集合
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string?>? GetEnvironmentVariables()
    {
        var vars = Environment.GetEnvironmentVariables();
        if (vars == null) return null;
        var result = new Dictionary<string, string?>();
        foreach (DictionaryEntry item in vars)
        {
            if (item.Key == null || item.Key.ToString().IsNullOrEmpty())
            {
                continue;
            }
            result.Add(item.Key.ToString()!, item.Value?.ToString());
        }
        return result;
    }
}

