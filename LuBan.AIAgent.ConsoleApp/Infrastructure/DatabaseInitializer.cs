/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Infrastructure
*文件名： DatabaseInitializer
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：数据库初始化
*
*****************************************************************************/
using LuBan.Orm;

namespace LuBan.AIAgent.ConsoleApp.Infrastructure;

/// <summary>
/// 数据库初始化器
/// </summary>
public static class DatabaseInitializer
{
    private static bool _initialized;

    /// <summary>
    /// 初始化数据库
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        LuBanOrm.Init();

        var dbPath = GetDatabasePath();
        Console.WriteLine($"数据库已初始化: {dbPath}");
    }

    /// <summary>
    /// 获取数据库路径
    /// </summary>
    public static string GetDatabasePath()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ai_sessions.db");
        return Path.GetFullPath(dbPath);
    }
}
