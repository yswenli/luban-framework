/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Wechat
*文件名： WechatCorpClient
*版本号： V1.0.0.0
*唯一标识：e488617e-eca4-4516-89c3-5f5e436f7195
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/11 11:36:02
*描述：企业微信客户端
*
*=================================================
*修改标记
*修改时间：2024/11/11 11:36:02
*修改人： yswenli
*版本号： V1.0.0.0
*描述：企业微信客户端
*
*****************************************************************************/
namespace LuBan.Wechat;

/// <summary>
/// 企业微信客户端
/// </summary>
public class WechatCorpClient : ICommonClient, IDisposable, IWechatCorpClient
{
    /// <summary>
    /// 企业微信服务商代开发客户端，自建应用时为当前业务处理客户端
    /// </summary>
    public WechatWorkClient Client { get; set; }

    /// <summary>
    /// 企业微信服务商代开发模板或智慧硬件的客户端，自建应用时为当前为空
    /// </summary>
    public List<WechatCorpSuiteClient> SuiteClients { get; set; }

    /// <summary>
    /// 企业微信自建应用客户端配置
    /// </summary>
    public WechatOptions WechatOptions { get; set; }


    HttpInterceptorCollection _interceptors;

    /// <summary>
    /// Interceptors
    /// </summary>
    public HttpInterceptorCollection Interceptors
    {
        get
        {
            return _interceptors;
        }
        set
        {
            _interceptors = value;
        }
    }

    /// <summary>
    /// JsonSerializer
    /// </summary>
    public IJsonSerializer JsonSerializer => throw new NotImplementedException();

    /// <summary>
    /// FormUrlEncodedSerializer
    /// </summary>
    public IFormUrlEncodedSerializer FormUrlEncodedSerializer => throw new NotImplementedException();

    /// <summary>
    /// 企业微信客户端
    /// </summary>
    /// <param name="wechatOptions"></param>
    public WechatCorpClient(WechatOptions wechatOptions)
    {
        WechatOptions = wechatOptions;
        if (WechatOptions == null) throw new Exception("请配置企业微信");
        if (WechatOptions.WechatCorpOptions.Type == EnumWechatCorpType.SelfApp)
        {
            var options = WechatOptions.WechatCorpOptions.SelfAppOptions;
            if (options == null
                || options.CorpId.IsNullOrEmpty()
                || options.AgentId == null
                || options.AgentSecret.IsNullOrEmpty()) throw new Exception("请配置企业微信自建应用");
            Client = WechatWorkClientBuilder.Create(new WechatWorkClientOptions()
            {
                CorpId = options.CorpId,
                AgentId = options.AgentId,
                AgentSecret = options.AgentSecret,
                PushEncodingAESKey = options.PushEncodingAESKey,
                PushToken = options.PushToken
            }).Build();
        }
        else if (WechatOptions.WechatCorpOptions.Type == EnumWechatCorpType.Developers)
        {
            var options = WechatOptions.WechatCorpOptions.DevelopersOptions;
            if (options == null
                || options.CorpId.IsNullOrEmpty()
                || options.ProviderSecret.IsNullOrEmpty()) throw new Exception("请配置企业微信开发者应用");
            Client = WechatWorkClientBuilder.Create(new WechatWorkClientOptions()
            {
                CorpId = options.CorpId,
                ProviderSecret = options.ProviderSecret,
                PushEncodingAESKey = options.PushEncodingAESKey,
                PushToken = options.PushToken
            }).Build();

            var suiteOptions = options.SuiteOptions;
            if (suiteOptions != null)
            {
                SuiteClients = [];
                foreach (var suiteOption in suiteOptions)
                {
                    var suiteClient = new WechatCorpSuiteClient(suiteOption);
                    SuiteClients.Add(suiteClient);
                }
            }
        }
        else
        {
            var options = wechatOptions.WechatCorpOptions.SmartHardwareOptions;
            if (options == null) throw new Exception("请配置企业微信智能硬件应用");
            Client = WechatWorkClientBuilder.Create(new WechatWorkClientOptions()
            {
                CorpId = options.CorpId,
                PushEncodingAESKey = options.PushEncodingAESKey,
                PushToken = options.PushToken,
                ModelId = options.ModelId,
                ModelSecret = options.ModelSecret

            }).Build();
        }
        Interceptors = Client.Interceptors;
        Client.Interceptors.Add(new WechatClientInterceptor());
    }


    /// <summary>
    /// 获取企业微信AccessToken，仅限自建应用、第三方应用和服务商代开发应用
    /// </summary>
    /// <param name="suiteAccessToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<AccessToken?> GetAccessToken(string? suiteAccessToken = null)
    {
        if (WechatOptions == null) throw new Exception("请配置企业微信");

        AccessToken? tokenResult = null;

        if (WechatOptions.WechatCorpOptions.Type == EnumWechatCorpType.SelfApp)
        {
            tokenResult = MemoryCache.Instance.Get<AccessToken>("WechatCorpAccessToken");
            if (tokenResult == null || tokenResult.ExpireTime <= DateTime.Now)
            {
                var result = await Client.ExecuteCgibinGetTokenAsync(new CgibinGetTokenRequest());
                if (result.ErrorCode != 0 || result.AccessToken.IsNullOrEmpty())
                {
                    throw new Exception($"获取企业微信AccessToken失败:code:{result.ErrorCode},msg:{result.ErrorMessage}");
                }
                tokenResult = new AccessToken()
                {
                    Token = result.AccessToken,
                    ExpireTime = DateTime.Now.AddSeconds(result.ExpiresIn)
                };
                MemoryCache.Instance.Set("WechatCorpAccessToken", tokenResult, TimeSpan.FromSeconds(result.ExpiresIn));
                return tokenResult;
            }
        }
        else if (WechatOptions.WechatCorpOptions.Type == EnumWechatCorpType.Developers)
        {
            tokenResult = MemoryCache.Instance.Get<AccessToken>("ProviderAccessToken");
            if (tokenResult == null || tokenResult.ExpireTime <= DateTime.Now)
            {
                var result = await Client.ExecuteCgibinServiceGetProviderTokenAsync(new CgibinServiceGetProviderTokenRequest());
                if (result.ErrorCode != 0 || result.ProviderAccessToken.IsNullOrEmpty())
                {
                    throw new Exception($"获取企业微信ProviderAccessToken失败:code:{result.ErrorCode},msg:{result.ErrorMessage}");
                }
                tokenResult = new AccessToken()
                {
                    Token = result.ProviderAccessToken,
                    ExpireTime = DateTime.Now.AddSeconds(result.ExpiresIn)
                };
                MemoryCache.Instance.Set("ProviderAccessToken", tokenResult, TimeSpan.FromSeconds(result.ExpiresIn));
                return tokenResult;
            }
        }
        else
        {
            throw new Exception("企业微信类型配置错误");
        }
        return tokenResult;
    }

    public void Configure(Action<CommonClientSettings> configure)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

