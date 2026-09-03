using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Dysmsapi20170525;
using AlibabaCloud.SDK.Dysmsapi20170525.Models;
using AlibabaCloud.TeaUtil.Models;
using LuBan.Common.Sms.Models;
using Tea;

namespace LuBan.Common.Sms.Providers;

/// <summary>
/// 阿里云短信（官方 SDK）
/// </summary>
public class AliyunSmsProvider : ISmsProvider
{
    readonly AliyunSmsSetting _setting;

    readonly Client _client;

    /// <summary>
    /// 运营商名称
    /// </summary>
    public string ProviderName => "Aliyun";

    /// <summary>
    /// 阿里云短信
    /// </summary>
    /// <param name="setting"></param>
    public AliyunSmsProvider(AliyunSmsSetting setting)
    {
        if (setting == null) throw new ArgumentNullException(nameof(setting));
        if (string.IsNullOrWhiteSpace(setting.AccessKeyId))
            throw new ArgumentException("阿里云短信配置缺少 AccessKeyId", nameof(setting));
        if (string.IsNullOrWhiteSpace(setting.AccessKeySecret))
            throw new ArgumentException("阿里云短信配置缺少 AccessKeySecret", nameof(setting));

        _setting = setting;
        _client = new Client(new Config
        {
            AccessKeyId = setting.AccessKeyId,
            AccessKeySecret = setting.AccessKeySecret,
            Endpoint = setting.Endpoint
        });
    }

    /// <summary>
    /// 构造 SendSms 请求（纯函数，供单测）
    /// </summary>
    internal static SendSmsRequest BuildSendSmsRequest(string templateCode, string signName, List<string> mobiles, string templateParam)
    {
        return new SendSmsRequest
        {
            SignName = signName,
            TemplateCode = templateCode,
            PhoneNumbers = string.Join(",", mobiles),
            TemplateParam = templateParam
        };
    }

    /// <summary>
    /// 阿里云响应映射（纯函数，供单测）
    /// </summary>
    internal static SmsRequestResult MapResult(SendSmsResponseBody body, string templateCode)
    {
        if (body == null)
            return new SmsRequestResult { Code = 500, Msg = "阿里云短信响应为空", TpId = templateCode };

        if (body.Code == "OK")
        {
            return new SmsRequestResult
            {
                Code = 200,
                Msg = body.Message,
                MsgId = body.BizId,
                TpId = templateCode
            };
        }
        return new SmsRequestResult
        {
            Code = 400,
            Msg = $"{body.Code}: {body.Message}",
            TpId = templateCode
        };
    }

    /// <summary>
    /// 模板参数分组键（纯函数，供单测）：null → ""；否则按 key 排序后 key=value 用 &amp; 连接
    /// </summary>
    internal static string BuildTpContentGroupKey(Dictionary<string, string> tpContent)
    {
        return tpContent == null ? "" :
            string.Join("&", tpContent.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));
    }

    /// <summary>
    /// 调 SDK 发送（TeaException 等异常统一转结果，不外抛）
    /// </summary>
    async Task<SmsRequestResult> SendAsync(SendSmsRequest request)
    {
        try
        {
            var resp = await _client.SendSmsWithOptionsAsync(request, new RuntimeOptions { Autoretry = true, MaxAttempts = 3 });
            return MapResult(resp.Body, request.TemplateCode);
        }
        catch (TeaException error)
        {
            var msg = error.Message;
            var recommend = error.DataResult != null && error.DataResult.TryGetValue("Recommend", out var r) ? r?.ToString() : null;
            if (!string.IsNullOrEmpty(recommend)) msg = $"{msg}（诊断：{recommend}）";
            return new SmsRequestResult { Code = 500, Msg = msg, TpId = request.TemplateCode };
        }
        catch (Exception ex)
        {
            return new SmsRequestResult { Code = 500, Msg = ex.Message, TpId = request.TemplateCode };
        }
    }

    /// <summary>
    /// 发送模板短信，不带参数
    /// </summary>
    public async Task<SmsRequestResult> SendTemplateAsync(string templateCode, List<string> mobiles)
    {
        if (mobiles == null || mobiles.Count == 0)
            return new SmsRequestResult { Code = 400, Msg = "无有效接收号码", TpId = templateCode };

        var request = BuildSendSmsRequest(templateCode, _setting.SignName, mobiles, "{}");
        return await SendAsync(request);
    }

    /// <summary>
    /// 发送模板短信，带参数（相同参数合并一批，不同参数逐请求；任一失败即返回）
    /// </summary>
    public async Task<SmsRequestResult> SendTemplateAsync(string templateCode, List<TemplateMsgInfo> mobileAndMsgs)
    {
        if (mobileAndMsgs == null || mobileAndMsgs.Count == 0)
            return new SmsRequestResult { Code = 400, Msg = "无有效接收号码", TpId = templateCode };

        SmsRequestResult last = null!;

        foreach (var group in mobileAndMsgs.GroupBy(m => BuildTpContentGroupKey(m.TpContent)))
        {
            var param = group.First().TpContent == null ? "{}" : group.First().TpContent.ToJson(hasIndentation: false);
            var request = BuildSendSmsRequest(templateCode, _setting.SignName, group.Select(m => m.Mobile).ToList(), param);

            var result = await SendAsync(request);
            if (result.Code != 200) return result;
            last = result;
        }
        return last;
    }

    /// <summary>
    /// 发送手机短信验证码（变量名固定 code，阿里云模板惯例）
    /// </summary>
    public async Task<SmsRequestResult> SendVerifyCodeAsync(string phoneNumber, string verifyCode)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return new SmsRequestResult { Code = 400, Msg = "无有效接收号码", TpId = _setting.TemplateCode };

        var param = new Dictionary<string, string> { { "code", verifyCode } }.ToJson(hasIndentation: false);
        var request = BuildSendSmsRequest(_setting.TemplateCode, _setting.SignName,
            new List<string> { phoneNumber }, param);
        return await SendAsync(request);
    }
}
