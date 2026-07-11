/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Wechat.CorpProvider
*文件名： WechatCorpSuiteClient
*版本号： V1.0.0.0
*唯一标识：29634b7f-b665-4ffd-b474-9e73842feff3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/13 9:57:04
*描述：企业微信代开发应用模板
*
*=================================================
*修改标记
*修改时间：2024/11/13 9:57:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：企业微信代开发应用模板
*
*****************************************************************************/

using static LuBan.Wechat.Models.WechatCorpOptions;

namespace LuBan.Wechat.CorpProvider;


/// <summary>
/// 企业微信代开发应用模板
/// </summary>
public class WechatCorpSuiteClient : IWechatCorpClient
{
    /// <summary>
    /// 代开发应用模板ticket
    /// </summary>
    public string SuiteTicket
    {
        get
        {
            return MemoryCache.Instance.Get<string>($"WechatCorpSuiteTicket_{SuiteOptions.SuiteId}") ?? "";
        }
        set
        {
            MemoryCache.Instance.Set($"WechatCorpSuiteTicket_{SuiteOptions.SuiteId}", value, TimeSpan.FromSeconds(1750));
        }
    }

    /// <summary>
    /// 代开发应用模板配置
    /// </summary>
    public WechatCorpSuiteOptions SuiteOptions { get; private set; }
    /// <summary>
    /// 企业微信代开发应用模板
    /// </summary>
    public WechatWorkClient Client { get; private set; }

    /// <summary>
    /// 企业微信代开发应用模板
    /// </summary>
    /// <param name="suiteOptions"></param>
    public WechatCorpSuiteClient(WechatCorpSuiteOptions suiteOptions)
    {
        if (suiteOptions.SuiteId.IsNullOrEmpty()) throw new Exception("企业微信代开发应用模板SuiteId不能为空");
        SuiteOptions = suiteOptions;
        Client = new WechatWorkClient(new WechatWorkClientOptions()
        {
            SuiteId = suiteOptions.SuiteId,
            SuiteSecret = suiteOptions.SuiteSecret,
            PushToken = suiteOptions.SuitePushToken,
            PushEncodingAESKey = suiteOptions.SuitePushEncodingAESKey
        });
        Client.Interceptors.Add(new WechatClientInterceptor());
    }

    /// <summary>
    /// 获取企业微信代开发应用模板access_token
    /// </summary>
    /// <param name="suiteAccessToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<AccessToken?> GetAccessToken(string? suiteAccessToken = null)
    {
        //未获取ticket时，直接返回
        if (SuiteTicket.IsNullOrEmpty())
            throw new Exception($"获取企业微信SuiteAccessToken失败:SuiteTicket不能为空");

        var cacheKey = $"WechatCorpSuiteAccessToken_{SuiteOptions.SuiteId}";

        AccessToken? tokenResult = MemoryCache.Instance.Get<AccessToken>(cacheKey);
        if (tokenResult == null || tokenResult.ExpireTime <= DateTime.Now)
        {
            var result = await Client.ExecuteCgibinServiceGetSuiteTokenAsync(new CgibinServiceGetSuiteTokenRequest()
            {
                SuiteTicket = SuiteTicket
            });
            if (result.ErrorCode != 0 || result.SuiteAccessToken.IsNullOrEmpty())
            {
                throw new Exception($"获取企业微信SuiteAccessToken失败:code:{result.ErrorCode},msg:{result.ErrorMessage}");
            }
            tokenResult = new AccessToken()
            {
                Token = result.SuiteAccessToken,
                ExpireTime = DateTime.Now.AddSeconds(result.ExpiresIn)
            };
            MemoryCache.Instance.Set(cacheKey, tokenResult, TimeSpan.FromSeconds(result.ExpiresIn));
        }
        return tokenResult;
    }
}
