/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Sms
*文件名： SmsConfigResolver
*版本号： V1.0.0.0
*唯一标识：b8e3f1a2-c4d6-4f8a-9e1b-3d5c7a9f2e6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/09/08 10:00:00
*描述：SmsOption 配置解析委托，支持外部注册（如 db_config 表读取），
*       未注册时返回 null，由调用方回退到默认配置源。
*
*=================================================
*修改标记
*修改时间：2026/09/08 10:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：创建
*
*****************************************************************************/
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