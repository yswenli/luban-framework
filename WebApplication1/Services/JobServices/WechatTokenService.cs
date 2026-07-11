using LuBan.Service;
using LuBan.Wechat;
using LuBan.Wechat.Models;

using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace WebApplication1.Services.JobServices;


/// <summary>
/// 微信token刷新服务，默认一分钟一次
/// </summary>
[JobInfo<WechatTokenService>("微信token刷新服务")]
public class WechatTokenService : BaseJobService
{
    const string WechatCacheKey = $"{CacheConst.KeySystem}wechat_access_token";
    readonly WechatOptions _wechatOptions;

    /// <summary>
    /// 微信token刷新服务，默认一分钟一次
    /// </summary>
    public WechatTokenService() : base(1000)
    {
        var options = ConfigUtil.Read<WechatOptions>();
        if (options == null) throw new Exception("未配置微信参数");
        _wechatOptions = options;
    }

    /// <summary>
    /// 获取微信公众号accesstoken
    /// </summary>
    void SetAccessToken()
    {
        try
        {
            if (_wechatOptions == null
                || _wechatOptions.WechatAppId.IsNullOrEmpty()
                || _wechatOptions.WechatAppSecret.IsNullOrEmpty()) return;

            var tokenResult = "";
            ServiceCache.Get<string>(WechatCacheKey);
            if (tokenResult.IsNullOrEmpty())
            {
                var client = WechatClientFactory.Create(EnumWechatType.Api) as WechatApiClient;
                if (client == null) return;
                var result = client.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest()
                {
                    AppId = _wechatOptions.WechatAppId,
                    AppSecret = _wechatOptions.WechatAppSecret
                }).GetAwaiter().GetResult();
                if (result.ErrorCode != 0 || result.AccessToken.IsNullOrEmpty())
                {
                    Logger.Error("获取微信AccessToken失败", $"code:{result.ErrorCode},msg:{result.ErrorMessage}");
                    return;
                }
                tokenResult = result.AccessToken;
                ServiceCache.Set(WechatCacheKey, tokenResult, TimeSpan.FromSeconds(result.ExpiresIn));
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }
    /// <summary>
    /// 获取微信小程序accesstoken
    /// </summary>
    void SetOpenAccessToken()
    {
        try
        {
            if (_wechatOptions == null
                || _wechatOptions.WxOpenAppId.IsNullOrEmpty()
                || _wechatOptions.WxOpenAppSecret.IsNullOrEmpty()) return;

            var tokenResult = "";//CacheService.Instance.Get<string>(WechatOpenCacheKey);
            if (tokenResult.IsNullOrEmpty())
            {
                var client = WechatClientFactory.Create(EnumWechatType.App) as WechatApiClient;
                if (client == null) return;
                var result = client.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest()
                {
                    AppId = _wechatOptions.WxOpenAppId,
                    AppSecret = _wechatOptions.WxOpenAppSecret
                }).GetAwaiter().GetResult();
                if (result.ErrorCode != 0 || result.AccessToken.IsNullOrEmpty())
                {
                    Logger.Error("获取微信小程序AccessToken失败", $"code:{result.ErrorCode},msg:{result.ErrorMessage}");
                    return;
                }
                tokenResult = result.AccessToken;
                //CacheService.Instance.Set(WechatOpenCacheKey, tokenResult, TimeSpan.FromSeconds(result.ExpiresIn));
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    /// <summary>
    /// 获取微信企业号accesstoken
    /// </summary>
    void SetCorpAccessToken()
    {
        try
        {
            if (_wechatOptions == null) return;
            var wechatCorpOptions = _wechatOptions.WechatCorpOptions;
            if (wechatCorpOptions == null) return;

            if (wechatCorpOptions.Type == EnumWechatCorpType.SelfApp
                && (wechatCorpOptions.SelfAppOptions == null
                || wechatCorpOptions.SelfAppOptions.CorpId.IsNullOrEmpty()
                || wechatCorpOptions.SelfAppOptions.AgentId == null)) return;

            if (wechatCorpOptions.Type == EnumWechatCorpType.Developers
                && (wechatCorpOptions.DevelopersOptions == null
                || wechatCorpOptions.DevelopersOptions.CorpId.IsNullOrEmpty()
                || wechatCorpOptions.DevelopersOptions.ProviderSecret.IsNullOrEmpty())) return;
            var accessToken = "";


            var _wechatCorpClient = (WechatCorpClient)WechatClientFactory.Create(EnumWechatType.Corp);
            accessToken = _wechatCorpClient.GetAccessToken().GetAwaiter().GetResult()?.Token;
            if (accessToken.IsNullOrEmpty())
            {
                throw FriendlyError.Ex("企业微信服务商获取AccessToken失败");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        return;
    }


    /// <summary>
    /// 执行业务
    /// </summary>
    public override void Run()
    {
        SetAccessToken();
        SetOpenAccessToken();
        SetCorpAccessToken();
    }
}
