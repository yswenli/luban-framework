using LuBan.Logging.Serialization;

namespace LuBan.Logging;

/// <summary>
/// IServiceCollection 扩展方法。
/// </summary>
public static class LuBanLoggingServiceExtensions
{
    /// <summary>
    /// 创建 LuBan STJ 序列化器委托，用于注入到 static Logger。
    /// </summary>
    /// <returns>序列化委托。</returns>
    public static Func<object, string> CreateLuBanSerializer()
    {
        return obj => LuBanJsonSerializer.Serialize(obj);
    }
}
