using LuBan.Orm;

namespace LuBan.AIAgent.ConsoleApp.Infrastructure;

/// <summary>
/// 数据库初始化器
/// </summary>
public static class DatabaseInitializer
{
    private static int _initialized;

    /// <summary>
    /// 初始化数据库
    /// </summary>
    public static void Initialize()
    {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0) return;
        MigrateLegacyDatabase();
        LuBanOrm.Init();
        EnsureIsCompactedColumn();
        var dbPath = GetDatabasePath();
    }

    /// <summary>
    /// 兜底迁移：ai_session_message 新增 IsCompacted 列
    /// </summary>
    private static void EnsureIsCompactedColumn()
    {
        try
        {
            new Repositories.SessionMessageRepository().Context.Ado
                .ExecuteCommand("ALTER TABLE ai_session_message ADD COLUMN IsCompacted INTEGER NOT NULL DEFAULT 0");
        }
        catch
        {
            // 列已存在（CodeFirst 已迁移或重复执行），忽略
        }
    }

    /// <summary>
    /// 获取数据库路径
    /// </summary>
    public static string GetDatabasePath()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "luban-ai-agent.db");
        return Path.GetFullPath(dbPath);
    }

    private static void MigrateLegacyDatabase()
    {
        var legacy = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ai_sessions.db");
        var current = GetDatabasePath();
        if (File.Exists(legacy) && !File.Exists(current))
        {
            try
            {
                File.Move(legacy, current);
                Console.WriteLine($"数据库已从 {Path.GetFileName(legacy)} 更名为 {Path.GetFileName(current)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库更名失败: {ex.Message}");
            }
        }
    }
}
