/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： EnvironmentArgsUtil
*版本号： V1.0.0.0
*唯一标识：54511e75-d323-4039-bcba-0c3ecfaf778d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/12 16:03:57
*描述：环境参数工具类
*
*=================================================
*修改标记
*修改时间：2024/8/12 16:03:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：环境参数工具类
*
*****************************************************************************/
namespace System;

/// <summary>
/// 环境参数工具类
/// </summary>
public static class EnvironmentArgsUtil
{
    /// <summary>
    /// 获取环境命令行参数
    /// </summary>
    /// <returns></returns>
    public static List<string> GetArgs()
    {
        var args = Environment.GetCommandLineArgs();
        if (args == null || args.Length < 1)
        {
            return new List<string>();
        }
        return args.ToList();
    }

    /// <summary>
    /// 获取环境命令行参数
    /// </summary>
    /// <param name="argName"></param>
    /// <returns></returns>
    public static string GetArgValue(this string argName)
    {
        if (argName.IsNullOrEmpty()) return argName;
        bool exists = false;
        foreach (var arg in GetArgs())
        {
            if (arg.Equals(argName, true))
            {
                exists = true;
                continue;
            }
            if (exists)
            {
                return arg;
            }
        }
        return string.Empty;
    }

    /// <summary>
    ///  获取环境变量字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> GetVariables()
    {
        var result = new Dictionary<string, string>();
        var variables = Environment.GetEnvironmentVariables();
        if (variables == null || variables.Count == 0)
        {
            return result;
        }
        foreach (DictionaryEntry item in variables)
        {
            if (item.Key != null)
                result.TryAdd(item.Key.ToString() ?? "", item.Value?.ToString() ?? "");
        }
        return result;
    }

    /// <summary>
    /// 获取环境变量值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetVariable(this string key)
    {
        if (key.IsNullOrEmpty()) return key;
        return GetVariables().TryGetValue(key, out var value) ? value : string.Empty;
    }
}
