using System.Text.Json;

using LuBan.Common.Sms;
using LuBan.Common.Sms.Models;
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
        public void AliyunProvider_BuildTpContentGroupKey_Null_ReturnsEmpty()
        {
            Assert.AreEqual("", AliyunSmsProvider.BuildTpContentGroupKey(null));
        }

        [TestMethod]
        public void AliyunProvider_BuildTpContentGroupKey_SameContentDifferentOrder_SameKey()
        {
            var a = new Dictionary<string, string> { { "name", "张三" }, { "code", "1234" } };
            var b = new Dictionary<string, string> { { "code", "1234" }, { "name", "张三" } };

            Assert.AreEqual(AliyunSmsProvider.BuildTpContentGroupKey(a), AliyunSmsProvider.BuildTpContentGroupKey(b));
            Assert.AreEqual("code=1234&name=张三", AliyunSmsProvider.BuildTpContentGroupKey(a));
        }

        [TestMethod]
        public void AliyunProvider_Constructor_MissingAK_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new AliyunSmsProvider(new AliyunSmsSetting { SignName = "特睛彩" }));
        }

        private class FakeSmsProvider : ISmsProvider
        {
            public string ProviderName => "Fake";
            public string LastTemplateCode;
            public SmsRequestResult Result = new SmsRequestResult { Code = 200 };

            public Task<SmsRequestResult> SendTemplateAsync(string templateCode, List<string> mobiles)
            {
                LastTemplateCode = templateCode;
                return Task.FromResult(Result);
            }

            public Task<SmsRequestResult> SendTemplateAsync(string templateCode, List<TemplateMsgInfo> mobileAndMsgs)
            {
                LastTemplateCode = templateCode;
                return Task.FromResult(Result);
            }

            public Task<SmsRequestResult> SendVerifyCodeAsync(string phoneNumber, string verifyCode)
            {
                LastTemplateCode = $"{phoneNumber}:{verifyCode}";
                return Task.FromResult(Result);
            }
        }

        [TestMethod]
        public void SmsSender_RoutesByOptionProvider()
        {
            var aliyunSender = new SmsSender(new SmsOption
            {
                Provider = "aliyun",   // 不区分大小写
                Aliyun = new AliyunSmsSetting { AccessKeyId = "ak", AccessKeySecret = "sk" }
            });
            Assert.IsInstanceOfType(aliyunSender.Provider, typeof(AliyunSmsProvider));

            var ztSender = new SmsSender(new SmsOption
            {
                ZhuTong = new ZhuTongSmsSetting { UserName = "u", Password = "p", Signature = "s", TemplateId = 1 }
            });
            Assert.IsInstanceOfType(ztSender.Provider, typeof(ZhuTongSmsProvider));
        }

        [TestMethod]
        public void SmsSender_RouteInvalidProvider_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new SmsSender(new SmsOption { Provider = "Unknown" }));
        }

        [TestMethod]
        public async Task SmsSender_LongTpId_PassedAsString()
        {
            var fake = new FakeSmsProvider();
            var sender = new SmsSender(fake, new SmsOption());

            await sender.SendTemplaMsgsAsync(123456, new List<string> { "14782301575" });

            Assert.AreEqual("123456", fake.LastTemplateCode);
        }

        [TestMethod]
        public void ZhuTongProvider_EncryptPassword_ReturnsDoubleMD5Lower()
        {
            var result = ZhuTongSmsProvider.EncryptPassword("test", "1234567890");
            Assert.AreEqual(32, result.Length);
            Assert.IsTrue(result.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
        }

        [TestMethod]
        public void ZhuTongProvider_EncryptPassword_Deterministic()
        {
            var r1 = ZhuTongSmsProvider.EncryptPassword("pwd", "tKey");
            var r2 = ZhuTongSmsProvider.EncryptPassword("pwd", "tKey");
            Assert.AreEqual(r1, r2);
        }


        [TestMethod]
        public void Test()
        {
            var smsOption = ConfigUtil.Read<SmsOption>();
            var sender = new SmsSender(smsOption);
            Assert.IsNotNull(sender);
            sender.SendValideCodeAsync("14782301575", "1234").Wait();

        }
    }
}
