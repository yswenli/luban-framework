/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Wechat.CorpProvider
*文件名： WechatCorpAppClient
*版本号： V1.0.0.0
*唯一标识：40ad1a76-722b-4de6-9c57-99a437c7c1c3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/13 10:03:16
*描述：企业微信代开发授权应用客户端
*
*=================================================
*修改标记
*修改时间：2024/11/13 10:03:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：企业微信代开发授权应用客户端
*
*****************************************************************************/
namespace LuBan.Wechat.CorpProvider;

/// <summary>
/// 企业微信代开发授权应用客户端
/// </summary>
public class WechatCorpAppClient : IWechatCorpClient
{
    /// <summary>
    /// 代开发应用模板配置
    /// </summary>
    public WechatCorpAppOptions AppOptions { get; private set; }
    /// <summary>
    /// 企业微信代开发应用模板
    /// </summary>
    public WechatWorkClient Client { get; private set; }

    /// <summary>
    /// 企业微信代开发授权应用
    /// </summary>
    /// <param name="appOptions"></param>
    public WechatCorpAppClient(WechatCorpAppOptions appOptions)
    {
        AppOptions = appOptions;
        Client = new WechatWorkClient(new WechatWorkClientOptions()
        {
            CorpId = appOptions.CorpId,
            AgentId = appOptions.AgentId,
            AgentSecret = appOptions.PermanentCode
        });
        Client.Interceptors.Add(new WechatClientInterceptor());
    }

    /// <summary>
    /// 获取企业微信授权应用AccessToken
    /// </summary>
    /// <param name="suiteAccessToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<AccessToken?> GetAccessToken(string? suiteAccessToken = null)
    {
        if (suiteAccessToken.IsNullOrEmpty()) throw new Exception("获取企业微信授权应用AccessToken时，必须传入有效的suiteAccessToken");

        AccessToken? tokenResult = null;

        var cacheKey = $"WechatCorpAppAccessToken_{AppOptions.CorpId}_{AppOptions.AgentId}";
        tokenResult = MemoryCache.Instance.Get<AccessToken>(cacheKey);
        if (tokenResult == null || tokenResult.ExpireTime <= DateTime.Now)
        {
            //代开发应使用的也是自建应用的接口，参数根据具体实调整即可；
            var result = await Client.ExecuteCgibinGetTokenAsync(new CgibinGetTokenRequest());

            if (!result.IsSuccessful())
            {
                throw new Exception($"获取企业微信授权应用AccessToken失败:code:{result.ErrorCode},msg:{result.ErrorMessage}");
            }
            tokenResult = new AccessToken()
            {
                Token = result.AccessToken,
                ExpireTime = DateTime.Now.AddSeconds(result.ExpiresIn)
            };
            MemoryCache.Instance.Set(cacheKey, tokenResult, TimeSpan.FromSeconds(result.ExpiresIn));
        }
        return tokenResult;
    }

    /// <summary>
    /// 获取企业微信授权应用CorpJsApiTicket
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public async Task<JsApiTicket> GetCorpJsApiTicket(string accessToken)
    {
        var cacheKey = $"WechatCorpAppCorpJsApiTicket_{AppOptions.CorpId}_{AppOptions.AgentId}";

        var jsApiTicket = MemoryCache.Instance.Get<JsApiTicket>(cacheKey);
        if (jsApiTicket == null || jsApiTicket.ExpireTime <= DateTime.Now)
        {
            var request = new CgibinGetJsapiTicketRequest()
            {
                AccessToken = accessToken
            };
            var result = await Client.ExecuteCgibinGetJsapiTicketAsync(request);
            if (!result.IsSuccessful())
                throw new Exception($"获取企业微信授权应用CorpJsApiTicket失败:code:{result.ErrorCode},msg:{result.ErrorMessage}");
            jsApiTicket = new JsApiTicket()
            {
                Ticket = result.Ticket,
                ExpireTime = DateTime.Now.AddSeconds(result.ExpiresIn)
            };
            MemoryCache.Instance.Set(cacheKey, jsApiTicket, TimeSpan.FromSeconds(result.ExpiresIn));
        }
        return jsApiTicket;
    }

    /// <summary>
    /// 获取企业微信授权应用AgentJsApiTicket
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<JsApiTicket> GetAgentJsApiTicket(string accessToken)
    {
        var cacheKey = $"WechatCorpAgentCorpJsApiTicket_{AppOptions.CorpId}_{AppOptions.AgentId}";
        var jsApiTicket = MemoryCache.Instance.Get<JsApiTicket>(cacheKey);
        if (jsApiTicket == null || jsApiTicket.ExpireTime <= DateTime.Now)
        {
            var request = new CgibinTicketGetRequest()
            {
                AccessToken = accessToken,
                Type = "agent_config"
            };
            var result = await Client.ExecuteCgibinTicketGetAsync(request);
            if (!result.IsSuccessful())
                throw new Exception($"获取企业微信授权应用AgentJsApiTicket失败:code:{result.ErrorCode},msg:{result.ErrorMessage}");
            jsApiTicket = new JsApiTicket()
            {
                Ticket = result.Ticket,
                ExpireTime = DateTime.Now.AddSeconds(result.ExpiresIn)
            };
            MemoryCache.Instance.Set(cacheKey, jsApiTicket, TimeSpan.FromSeconds(result.ExpiresIn));
        }
        return jsApiTicket;
    }
}
