namespace LuBan.Common.Sms;

/// <summary>
/// SmsOption 配置解析器，支持外部注册委托（如从 db_config 表读取），
/// 未注册时返回 null，由调用方回退到 appsettings.json。
/// </summary>
public static class SmsConfigResolver
{
    static volatile Func<SmsOption?>? _resolver;

    /// <summary>
    /// 注册配置解析委托（Web.Core 在启动时调用）
    /// </summary>
    public static void Register(Func<SmsOption?> resolver)
    {
        _resolver = resolver;
    }

    /// <summary>
    /// 尝试解析配置，未注册或委托返回 null 则返回 null
    /// </summary>
    internal static SmsOption? Resolve()
    {
        return _resolver?.Invoke();
    }
}