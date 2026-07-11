/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Wechat
*文件名： WechatTenpayClient
*版本号： V1.0.0.0
*唯一标识：fb7cb0bf-74ea-4ead-98a5-c821f2518ffd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/14 10:56:30
*描述：微信接口客户端工具工厂
*
*=================================================
*修改标记
*修改时间：2023/12/14 10:56:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信接口客户端工具工厂
*
*****************************************************************************/

namespace LuBan.Wechat;

/// <summary>
/// 微信接口客户端工具工厂
/// </summary>
public static class WechatClientFactory
{
    static ConcurrentDictionary<EnumWechatType, ICommonClient> _cache;

    /// <summary>
    /// 微信接口客户端工具工厂
    /// </summary>
    static WechatClientFactory()
    {
        _cache = new ConcurrentDictionary<EnumWechatType, ICommonClient>();
    }

    /// <summary>
    /// 读取配置并返回对应的客户端
    /// </summary>
    /// <param name="wechatType"></param>
    /// <returns></returns>
    public static ICommonClient Create(EnumWechatType wechatType = EnumWechatType.Api)
    {
        return _cache.GetOrAdd(wechatType, (key) =>
        {
            var wechatOption = ConfigUtil.Read<WechatOptions>();
            ICommonClient commonClient;
            switch (wechatType)
            {
                case EnumWechatType.Pay:
                    var _wechatPayOptions = ConfigUtil.Read<WechatPayOptions>();
                    if (_wechatPayOptions == null) throw new Exception("请配置微信支付参数");
                    var cerFilePath = WebApp.WebHostEnvironment.ContentRootPath + _wechatPayOptions.MerchantCertificatePrivateKey;
                    var tenpayClientOptions = new WechatTenpayClientOptions()
                    {
                        MerchantId = _wechatPayOptions.MerchantId,
                        MerchantV3Secret = _wechatPayOptions.MerchantV3Secret,
                        MerchantCertificateSerialNumber = _wechatPayOptions.MerchantCertificateSerialNumber,
                        MerchantCertificatePrivateKey = File.Exists(cerFilePath) ? File.ReadAllText(cerFilePath) : "",
                        PlatformCertificateManager = new InMemoryCertificateManager(),
                        Endpoint = WechatTenpayEndpoints.DEFAULT
                    };
                    commonClient = new WechatTenpayClient(tenpayClientOptions);
                    break;

                case EnumWechatType.App:
                    if (wechatOption == null) throw new Exception("请配置微信App参数");
                    commonClient = new WechatApiClient(new WechatApiClientOptions
                    {
                        AppId = wechatOption.WxOpenAppId,
                        AppSecret = wechatOption.WxOpenAppSecret,
                        Endpoint = WechatApiEndpoints.REGION_SHANGHAI
                    });
                    break;
                case EnumWechatType.Corp:
                    if (wechatOption == null) throw new Exception("请配置企微参数");                    
                    commonClient = new WechatCorpClient(wechatOption);
                    break;
                default:
                    if (wechatOption == null) throw new Exception("请配置微信参数");
                    commonClient = new WechatApiClient(new WechatApiClientOptions
                    {
                        AppId = wechatOption.WechatAppId,
                        AppSecret = wechatOption.WechatAppSecret,
                        Endpoint = WechatApiEndpoints.REGION_SHANGHAI
                    });
                    break;
            }
            commonClient.Interceptors.Add(new WechatClientInterceptor());
            return commonClient;
        });
    }

    /// <summary>
    /// 创建微信支付端
    /// </summary>
    /// <param name="wechatPayOptions"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ICommonClient CreateWechatTenpayClient(WechatPayOptions wechatPayOptions)
    {
        return _cache.GetOrAdd(EnumWechatType.Pay, (key) =>
        {
            if (wechatPayOptions == null) throw new Exception("请配置微信支付参数");
            var cerFilePath = WebApp.WebHostEnvironment.ContentRootPath + wechatPayOptions.MerchantCertificatePrivateKey;
            var tenpayClientOptions = new WechatTenpayClientOptions()
            {
                MerchantId = wechatPayOptions.MerchantId,
                MerchantV3Secret = wechatPayOptions.MerchantV3Secret,
                MerchantCertificateSerialNumber = wechatPayOptions.MerchantCertificateSerialNumber,
                MerchantCertificatePrivateKey = File.Exists(cerFilePath) ? File.ReadAllText(cerFilePath) : "",
                PlatformCertificateManager = new InMemoryCertificateManager()
            };
            return new WechatTenpayClient(tenpayClientOptions);
        });

    }

    /// <summary>
    /// 创建微信服务号客户端
    /// </summary>
    /// <param name="wechatOption"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ICommonClient CreateWechatApiClient(WechatOptions wechatOption)
    {
        return _cache.GetOrAdd(EnumWechatType.Api, (key) =>
        {
            if (wechatOption == null) throw new Exception("请配置微信参数");
            return new WechatApiClient(new WechatApiClientOptions
            {
                AppId = wechatOption.WechatAppId,
                AppSecret = wechatOption.WechatAppSecret
            });
        });
    }
    /// <summary>
    /// 创建微信app客户端
    /// </summary>
    /// <param name="wechatOption"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ICommonClient CreateWechatAppClient(WechatOptions wechatOption)
    {
        return _cache.GetOrAdd(EnumWechatType.App, (key) =>
        {
            if (wechatOption == null) throw new Exception("请配置微信参数");
            return new WechatApiClient(new WechatApiClientOptions
            {
                AppId = wechatOption.WxOpenAppId,
                AppSecret = wechatOption.WxOpenAppSecret
            });
        });

    }

    /// <summary>
    /// 创建企业微信客户端
    /// </summary>
    /// <param name="wechatOption"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ICommonClient CreateWechatCorpClient(WechatOptions wechatOption)
    {
        return _cache.GetOrAdd(EnumWechatType.App, (key) =>
        {
            if (wechatOption == null) throw new Exception("请配置微信参数");
            return new WechatCorpClient(wechatOption);
        });

    }
}
