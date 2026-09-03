using System.Text.Json;
using LuBan.Common.Sms;
using LuBan.Common.Sms.Providers;

namespace LuBan.XTestProject
{
    /// <summary>
    /// 短信纯逻辑测试：不发真实短信、不依赖网络与 AK
    /// </summary>
    [TestClass]
    public class SmsUnitTest
    {
        [TestMethod]
        public void SmsOption_DefaultProvider_IsZhuTong()
        {
            var option = new SmsOption();

            Assert.AreEqual("ZhuTong", option.Provider);
            Assert.IsNull(option.Aliyun);
        }

        [TestMethod]
        public void SmsOption_LegacyJsonWithoutNewFields_BindsAsZhuTong()
        {
            var json = @"{""ZhuTong"":{""UserName"":""u"",""Password"":""p"",""TemplateId"":123,""Signature"":""s""}}";

            var option = JsonSerializer.Deserialize<SmsOption>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(option);
            // STJ 反序列化会运行属性初始化器：JSON 缺 Provider 字段时保留默认值 "ZhuTong"（向后兼容的关键）
            Assert.AreEqual("ZhuTong", option.Provider);
            Assert.IsNotNull(option.ZhuTong);
            Assert.AreEqual(123, option.ZhuTong.TemplateId);
        }

        [TestMethod]
        public void AliyunProvider_BuildVerifyCodeRequest_HasCodeParam()
        {
            var setting = new AliyunSmsSetting
            {
                AccessKeyId = "ak",
                AccessKeySecret = "sk",
                SignName = "特睛彩",
                TemplateCode = "SMS_499015208"
            };

            var request = AliyunSmsProvider.BuildSendSmsRequest("SMS_499015208", setting.SignName,
                new List<string> { "14782301575" }, """{"code":"1234"}""");

            Assert.AreEqual("14782301575", request.PhoneNumbers);
            Assert.AreEqual("特睛彩", request.SignName);
            Assert.AreEqual("SMS_499015208", request.TemplateCode);
            Assert.AreEqual("""{"code":"1234"}""", request.TemplateParam);
        }

        [TestMethod]
        public void AliyunProvider_MapResult_OK_Returns200()
        {
            var body = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsResponseBody
            {
                Code = "OK",
                Message = "ok",
                BizId = "770301417791481665",
                RequestId = "B0BA3C82-xxxx"
            };

            var result = AliyunSmsProvider.MapResult(body, "SMS_499015208");

            Assert.AreEqual(200, result.Code);
            Assert.AreEqual("ok", result.Msg);
            Assert.AreEqual("770301417791481665", result.MsgId);
            Assert.AreEqual("SMS_499015208", result.TpId);
        }

        [TestMethod]
        public void AliyunProvider_MapResult_NotOK_Returns400()
        {
            var body = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsResponseBody
            {
                Code = "isv.BUSINESS_LIMIT_CONTROL",
                Message = "触发分钟级流控"
            };

            var result = AliyunSmsProvider.MapResult(body, "SMS_499015208");

            Assert.AreEqual(400, result.Code);
            Assert.AreEqual("isv.BUSINESS_LIMIT_CONTROL: 触发分钟级流控", result.Msg);
        }

        [TestMethod]
        public void AliyunProvider_Constructor_MissingAK_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new AliyunSmsProvider(new AliyunSmsSetting { SignName = "特睛彩" }));
        }
    }
}
