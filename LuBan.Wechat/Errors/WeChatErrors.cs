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
}
