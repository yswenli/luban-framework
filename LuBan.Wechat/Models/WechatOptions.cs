/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： WechatOptions
*版本号： V1.0.0.0
*唯一标识：bc77b3a6-7b97-41d4-a6fe-0637722eb188
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 10:49:04
*描述：微信相关配置选项
*
*=================================================
*修改标记
*修改时间：2023/12/5 10:49:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信相关配置选项
*
*****************************************************************************/
namespace LuBan.Wechat.Models;

/// <summary>
/// 微信相关配置选项
/// </summary>
public sealed class WechatOptions
{
    #region 公众号
    /// <summary>
    /// 微信公众号的应用ID
    /// </summary>
    public string WechatAppId { get; set; }

    /// <summary>
    /// 微信公众号的应用密钥
    /// </summary>
    public string WechatAppSecret { get; set; }
    /// <summary>
    /// 是否启用回调代理，
    /// 启用代理后，回调地址将会被代理到【LuBan微信消息中转服务】平台上
    /// </summary>
    public bool? EnableCallbackProxy { get; set; } = false;
    /// <summary>
    /// 跳转回调代理的域名
    /// </summary>
    public string CallbackProxyDomain { get; set; } = "https://uat-wcforward.luban.com/";
    #endregion

    #region 微信小程序
    /// <summary>
    /// 微信小程序的应用ID
    /// </summary>
    public string WxOpenAppId { get; set; }

    /// <summary>
    /// 微信小程序的应用密钥
    /// </summary>
    public string WxOpenAppSecret { get; set; }
    #endregion

    #region 企业微信
    /// <summary>
    /// 企业微信相关配置选项
    /// </summary>
    public WechatCorpOptions WechatCorpOptions { get; set; }
    #endregion
}
/// <summary>
/// 企业微信类型
/// </summary>
public enum EnumWechatCorpType
{
    /// <summary>
    /// 自建应用
    /// </summary>
    SelfApp = 0,
    /// <summary>
    /// 开发者
    /// </summary>
    Developers = 1,
    /// <summary>
    /// 智慧硬件
    /// </summary>
    SmartHardware = 2
}

/// <summary>
/// 企业微信配置
/// </summary>
public sealed class WechatCorpOptions
{
    /// <summary>
    /// 企业微信类型
    /// </summary>
    public EnumWechatCorpType Type { get; set; }

    /// <summary>
    /// 自建应用配置
    /// </summary>
    public WechatCorpSelfAppOptions? SelfAppOptions { get; set; }
    /// <summary>
    /// 代开发者配置
    /// </summary>
    public WechatCorpDevelopersOptions? DevelopersOptions { get; set; }
    /// <summary>
    /// 智慧硬件配置
    /// </summary>
    public WechatCorpSmartHardwareOptions? SmartHardwareOptions { get; set; }


    /// <summary>
    /// 企业微信自建应用
    /// </summary>
    public class WechatCorpSelfAppOptions
    {
        /// <summary>
        /// 企业微信
        /// </summary>
        public string CorpId { get; set; }
        /// <summary>
        /// 企业微信应用的id,仅限企业内部开发时使用。
        /// </summary>
        public int? AgentId { get; set; }
        /// <summary>
        /// 企业微信应用的密码,仅限企业内部开发时使用。
        /// </summary>
        public string? AgentSecret { get; set; }

        /// <summary>
        /// 获取或设置企业微信服务器推送的token
        /// </summary>
        public string PushToken { get; set; }
        /// <summary>
        /// 获取或设置企业微信服务器推送的aeskey
        /// </summary>
        public string PushEncodingAESKey { get; set; }
    }

    /// <summary>
    /// 企业微信应用模板配置
    /// </summary>
    public class WechatCorpSuiteOptions
    {
        /// <summary>
        /// 获取或设置企业微信第三方应用的 SuiteId。仅限第三方应用开发或服务商代开发时使用。
        /// </summary>
        public string? SuiteId { get; set; }
        /// <summary>
        /// 获取或设置企业微信第三方应用的 SuiteSecret。仅限第三方应用开发或服务商代开发时使用。
        /// </summary>
        public string? SuiteSecret { get; set; }

        /// <summary>
        /// 获取或设置企业微信服务器推送的token
        /// </summary>
        public string SuitePushToken { get; set; }
        /// <summary>
        /// 获取或设置企业微信服务器推送的aeskey
        /// </summary>
        public string SuitePushEncodingAESKey { get; set; }
    }


    /// <summary>
    /// 企业微信开发者配置
    /// </summary>
    public class WechatCorpDevelopersOptions
    {
        /// <summary>
        /// 企业微信
        /// </summary>
        public string CorpId { get; set; }
        /// <summary>
        /// 获取或设置企业微信服务商 Secret。仅限第三方应用开发或服务商代开发时使用。
        /// </summary>
        public string? ProviderSecret { get; set; }

        /// <summary>
        /// 获取或设置企业微信服务器推送的token
        /// </summary>
        public string PushToken { get; set; }
        /// <summary>
        /// 获取或设置企业微信服务器推送的aeskey
        /// </summary>
        public string PushEncodingAESKey { get; set; }

        /// <summary>
        /// 获取或设置企业微信第三方应用配置
        /// </summary>
        public List<WechatCorpSuiteOptions> SuiteOptions { get; set; }
    }

    /// <summary>
    /// 企业微信硬件配置
    /// </summary>
    public class WechatCorpSmartHardwareOptions
    {
        /// <summary>
        /// 企业微信
        /// </summary>
        public string CorpId { get; set; }
        /// <summary>
        /// 获取或设置企业微信硬件型号的 ModelId。仅限智慧硬件开发时使用。
        /// </summary>
        public string? ModelId { get; set; }

        /// <summary>
        /// 获取或设置企业微信硬件型号的 ModelSecret。仅限智慧硬件开发时使用。
        /// </summary>
        public string? ModelSecret { get; set; }

        /// <summary>
        /// 获取或设置企业微信服务器推送的token
        /// </summary>
        public string PushToken { get; set; }
        /// <summary>
        /// 获取或设置企业微信服务器推送的aeskey
        /// </summary>
        public string PushEncodingAESKey { get; set; }
    }
}
