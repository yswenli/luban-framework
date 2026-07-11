/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Utils
*文件名： EnvironmentUtil
*版本号： V1.0.0.0
*唯一标识：28a8880c-5168-47a7-8147-311464000434
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/3 9:34:19
*描述：环境变量
*
*=================================================
*修改标记
*修改时间：2025/4/3 9:34:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：环境变量
*
*****************************************************************************/
namespace System;

/// <summary>
/// 环境变量,
/// 优先级：启动参数 > 环境变量 > 配置文件
/// </summary>
public static class EnvironmentUtil
{
    /// <summary>
    /// 环境变量
    /// </summary>
    public static string EnvironmentString { get; set; }

    /// <summary>
    /// 获取环境，
    /// 优先级：启动参数 > 环境变量 > 配置文件
    /// </summary>
    /// <returns></returns>
    public static string GetEnvironment()
    {
        //从启动参数中读取
        if (EnvironmentString.IsNullOrEmpty())
        {
            //从dotnet环境变量中读取
            var env = EnvironmentVars.DotNetEnvironment;
            if (env.IsNotNullOrEmpty())
            {
                EnvironmentString = env;
            }
            else
            {
                //从aspnetcore环境变量中读取
                env = EnvironmentVars.AspnetcoreEnvironment;
                if (env.IsNotNullOrEmpty())
                {
                    EnvironmentString = env;
                }
                else
                {
                    //从配置文件中读取
                    env = NacosConfigUtil.GetEnvironment();
                    if (env.IsNotNullOrEmpty())
                    {
                        EnvironmentString = env;
                    }
                }
            }
        }
        return EnvironmentString;
    }

    /// <summary>
    /// 设置环境，从启动参数中解析环境
    /// </summary>
    /// <param name="args">参数示例 --environment env</param>
    public static void SetEnvironment(string[]? args)
    {
        if (args == null || args.Length < 2) return;
        EnvironmentString = ArgsUtil.Read<string>(args, "environment") ?? "";
    }
}
