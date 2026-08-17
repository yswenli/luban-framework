namespace LuBan.Wechat.Errors;

using LuBan.Common.Errors;

public static class WeChatErrors
{
    public static readonly ErrorDescriptor TokenFailed = new(81001, "通过code获取微信token失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor AccessTokenFailed = new(81002, "获取微信accessToken失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor ProfileFailed = new(81003, "获取微信昵称头像失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor NotFollowed = new(81004, "未关注微信公众号", ErrorCategory.Business);
    public static readonly ErrorDescriptor SendFailed = new(81005, "发送微信消息失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor TemplateSendFailed = new(81006, "发送微信模板消息失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor PrepayFailed = new(81007, "微信预付下单失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor OrderInfoFailed = new(81008, "获取微信订单信息失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor ConfigMissing = new(81009, "微信配置缺失", ErrorCategory.System);
    public static readonly ErrorDescriptor SuiteTokenFailed = new(81010, "获取企业微信SuiteAccessToken失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor CorpJsApiTicketFailed = new(81011, "获取企业微信JsApiTicket失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor LocationFailed = new(81012, "腾讯位置服务调用失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor CallbackFailed = new(81013, "微信回调处理失败", ErrorCategory.Business);
}
