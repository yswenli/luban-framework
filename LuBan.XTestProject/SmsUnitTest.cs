/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.XTestProject
*文件名： SmsUnitTest.cs
*版本号： V1.0.0.0
*唯一标识：d7ef651e-7b11-4ea2-a6da-331bd7dc5d03
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/3 11:35:04
*描述：SmsUnitTest 类
*
*=================================================
*修改标记
*修改时间：2026/9/3 11:35:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SmsUnitTest 类
*
*****************************************************************************/

using System.Text.Json;

using LuBan.Common.Consts;
using LuBan.Common.Errors;
using LuBan.Common.Sms;
using LuBan.Common.Sms.Models;
using LuBan.Common.Sms.Providers;
using LuBan.Web.Core.Models;
using LuBan.Web.Core.Utils;

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
        public void SmsOption_VerifyCodeExpireMinutes_DefaultIs5()
        {
            var option = new SmsOption();
            Assert.AreEqual(5, option.VerifyCodeExpireMinutes);
        }

        [TestMethod]
        public void SmsOption_VerifyCodeLength_DefaultIs4()
        {
            var option = new SmsOption();
            Assert.AreEqual(4, option.VerifyCodeLength);
        }

        [TestMethod]
        public void SmsOption_LegacyJson_WithoutVerifyCodeLength_DefaultIs4()
        {
            var json = @"{""Provider"":""ZhuTong"",""ZhuTong"":{""UserName"":""u"",""Password"":""p"",""TemplateId"":123,""Signature"":""s""}}";

            var option = JsonSerializer.Deserialize<SmsOption>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(option);
            Assert.AreEqual(4, option.VerifyCodeLength);
        }

        [TestMethod]
        public void SmsOption_WithVerifyCodeLength_DeserializesCorrectly()
        {
            var json = @"{""VerifyCodeLength"":6,""ZhuTong"":{""UserName"":""u"",""Password"":""p"",""TemplateId"":123,""Signature"":""s""}}";

            var option = JsonSerializer.Deserialize<SmsOption>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(option);
            Assert.AreEqual(6, option.VerifyCodeLength);
        }

        [TestMethod]
        public void SmsOption_LegacyJson_WithoutVerifyCodeExpireMinutes_DefaultIs5()
        {
            var json = @"{""Provider"":""ZhuTong"",""ZhuTong"":{""UserName"":""u"",""Password"":""p"",""TemplateId"":123,""Signature"":""s""}}";

            var option = JsonSerializer.Deserialize<SmsOption>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(option);
            Assert.AreEqual(5, option.VerifyCodeExpireMinutes);
        }

        [TestMethod]
        public void SmsOption_WithVerifyCodeExpireMinutes_DeserializesCorrectly()
        {
            var json = @"{""VerifyCodeExpireMinutes"":10,""ZhuTong"":{""UserName"":""u"",""Password"":""p"",""TemplateId"":123,""Signature"":""s""}}";

            var option = JsonSerializer.Deserialize<SmsOption>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(option);
            Assert.AreEqual(10, option.VerifyCodeExpireMinutes);
        }

        [TestMethod]
        public void SmsConfigResolver_NotRegistered_ReturnsNull()
        {
            var result = SmsConfigResolver.Resolve();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SmsConfigResolver_Registered_ReturnsConfig()
        {
            var expected = new SmsOption { Provider = "Aliyun", VerifyCodeExpireMinutes = 3 };
            SmsConfigResolver.Register(() => expected);
            try
            {
                var result = SmsConfigResolver.Resolve();
                Assert.IsNotNull(result);
                Assert.AreEqual("Aliyun", result.Provider);
                Assert.AreEqual(3, result.VerifyCodeExpireMinutes);
            }
            finally
            {
                // 恢复为未注册状态，避免影响其他测试
                SmsConfigResolver.Register(() => null!);
            }
        }

        [TestMethod]
        public void SmsSender_DefaultConstructor_WithResolverRegistered_UsesDbConfig()
        {
            var dbOption = new SmsOption
            {
                Provider = "ZhuTong",
                ZhuTong = new ZhuTongSmsSetting { UserName = "db", Password = "dbpwd", Signature = "dbSign", TemplateId = 99 },
                VerifyCodeExpireMinutes = 7
            };
            SmsConfigResolver.Register(() => dbOption);
            try
            {
                var sender = new SmsSender();
                Assert.IsNotNull(sender);
                Assert.AreSame(dbOption, sender.Option);
                Assert.AreEqual(7, sender.Option.VerifyCodeExpireMinutes);
                Assert.AreEqual("db", sender.Option.ZhuTong.UserName);
            }
            finally
            {
                SmsConfigResolver.Register(() => null!);
            }
        }

        [TestMethod]
        public void ValidatePhoneVerifyCode_PhoneEmpty_Throws()
        {
            try
            {
                SmsExtention.ValidatePhoneVerifyCode("", "1234");
                Assert.Fail("应该抛出异常");
            }
            catch (FriendlyException ex)
            {
                Assert.AreEqual(FrameworkErrors.Common.PhoneEmpty.Code, ex.Error.Code);
            }
        }

        [TestMethod]
        public void ValidatePhoneVerifyCode_NoCache_ThrowsCaptchaError()
        {
            var phone = "13800138001";
            MemoryCache.Instance.Delete(CacheConst.KeyPhoneVerCode + phone);
            try
            {
                SmsExtention.ValidatePhoneVerifyCode(phone, "1234");
                Assert.Fail("应该抛出异常");
            }
            catch (FriendlyException ex)
            {
                Assert.AreEqual(FrameworkErrors.Common.CaptchaError.Code, ex.Error.Code);
            }
        }

        [TestMethod]
        public void ValidatePhoneVerifyCode_AlreadyUsed_Throws()
        {
            var phone = "13800138003";
            var key = CacheConst.KeyPhoneVerCode + phone;
            MemoryCache.Instance.Set(key, new PhoneVerifyCodeInfo
            {
                Code = "5678",
                CreateTime = DateTime.Now,
                IsUsed = true,
                ExpireMinutes = 5
            }, TimeSpan.FromMinutes(5));

            try
            {
                try
                {
                    SmsExtention.ValidatePhoneVerifyCode(phone, "5678");
                    Assert.Fail("应该抛出异常");
                }
                catch (FriendlyException ex)
                {
                    Assert.AreEqual(FrameworkErrors.Common.SmsVerifyCodeUsed.Code, ex.Error.Code);
                }
            }
            finally
            {
                MemoryCache.Instance.Delete(key);
            }
        }

        [TestMethod]
        public void ValidatePhoneVerifyCode_Expired_ThrowsCaptchaError()
        {
            var phone = "13800138005";
            var key = CacheConst.KeyPhoneVerCode + phone;
            MemoryCache.Instance.Set(key, new PhoneVerifyCodeInfo
            {
                Code = "5678",
                CreateTime = DateTime.Now.AddMinutes(-6),
                IsUsed = false,
                ExpireMinutes = 5
            }, TimeSpan.FromMinutes(5));

            try
            {
                try
                {
                    SmsExtention.ValidatePhoneVerifyCode(phone, "5678");
                    Assert.Fail("应该抛出异常");
                }
                catch (FriendlyException ex)
                {
                    Assert.AreEqual(FrameworkErrors.Common.CaptchaError.Code, ex.Error.Code);
                }
            }
            finally
            {
                MemoryCache.Instance.Delete(key);
            }
        }

        [TestMethod]
        public void ValidatePhoneVerifyCode_WrongCode_ThrowsCaptchaError()
        {
            var phone = "13800138004";
            var key = CacheConst.KeyPhoneVerCode + phone;
            MemoryCache.Instance.Set(key, new PhoneVerifyCodeInfo
            {
                Code = "5678",
                CreateTime = DateTime.Now,
                IsUsed = false,
                ExpireMinutes = 5
            }, TimeSpan.FromMinutes(5));

            try
            {
                try
                {
                    SmsExtention.ValidatePhoneVerifyCode(phone, "9999");
                    Assert.Fail("应该抛出异常");
                }
                catch (FriendlyException ex)
                {
                    Assert.AreEqual(FrameworkErrors.Common.CaptchaError.Code, ex.Error.Code);
                }
            }
            finally
            {
                MemoryCache.Instance.Delete(key);
            }
        }

        [TestMethod]
        public void ValidatePhoneVerifyCode_CodeMatches_MarksUsed()
        {
            var phone = "13800138002";
            var key = CacheConst.KeyPhoneVerCode + phone;
            MemoryCache.Instance.Set(key, new PhoneVerifyCodeInfo
            {
                Code = "5678",
                CreateTime = DateTime.Now,
                IsUsed = false,
                ExpireMinutes = 5
            }, TimeSpan.FromMinutes(5));

            try
            {
                SmsExtention.ValidatePhoneVerifyCode(phone, "5678");
                var cached = MemoryCache.Instance.Get<PhoneVerifyCodeInfo>(key);
                Assert.IsNotNull(cached);
                Assert.IsTrue(cached.IsUsed);
            }
            finally
            {
                MemoryCache.Instance.Delete(key);
            }
        }

        [TestMethod]
        public void SendVerifyCodeAsync_PhoneEmpty_Throws()
        {
            try
            {
                SmsExtention.SendVerifyCodeAsync("").GetAwaiter().GetResult();
                Assert.Fail("应该抛出异常");
            }
            catch (FriendlyException ex)
            {
                Assert.AreEqual(FrameworkErrors.Common.PhoneEmpty.Code, ex.Error.Code);
            }
        }

        [TestMethod]
        public void SendVerifyCodeAsync_NoCache_GeneratesAndStoresCode()
        {
            SmsConfigResolver.Register(() => new SmsOption
            {
                Provider = "ZhuTong",
                ZhuTong = new ZhuTongSmsSetting { UserName = "test", Password = "test", Signature = "test", TemplateId = 1 },
                VerifyCodeExpireMinutes = 5
            });
            try
            {
                var phone = "13800138010";
                var key = CacheConst.KeyPhoneVerCode + phone;
                MemoryCache.Instance.Delete(key);

                try
                {
                    SmsExtention.SendVerifyCodeAsync(phone).GetAwaiter().GetResult();
                }
                catch
                {
                    // SMS 发送在测试环境失败（假凭据），忽略
                }

                var cached = MemoryCache.Instance.Get<PhoneVerifyCodeInfo>(key);
                Assert.IsNotNull(cached, "验证码应已缓存");
                Assert.IsFalse(cached.IsUsed);
                Assert.IsTrue(cached.Code.Length == 4, "应为4位数字验证码");
                MemoryCache.Instance.Delete(key);
            }
            finally
            {
                SmsConfigResolver.Register(() => null!);
            }
        }

        [TestMethod]
        public void SendVerifyCodeAsync_CustomLength_GeneratesCodeOfConfiguredLength()
        {
            SmsConfigResolver.Register(() => new SmsOption
            {
                Provider = "ZhuTong",
                ZhuTong = new ZhuTongSmsSetting { UserName = "test", Password = "test", Signature = "test", TemplateId = 1 },
                VerifyCodeExpireMinutes = 5,
                VerifyCodeLength = 6
            });
            try
            {
                var phone = "13800138020";
                var key = CacheConst.KeyPhoneVerCode + phone;
                MemoryCache.Instance.Delete(key);

                try
                {
                    SmsExtention.SendVerifyCodeAsync(phone).GetAwaiter().GetResult();
                }
                catch
                {
                    // SMS 发送在测试环境失败（假凭据），忽略
                }

                var cached = MemoryCache.Instance.Get<PhoneVerifyCodeInfo>(key);
                Assert.IsNotNull(cached, "验证码应已缓存");
                Assert.IsTrue(cached.Code.Length == 6, "应为6位数字验证码");
                MemoryCache.Instance.Delete(key);
            }
            finally
            {
                SmsConfigResolver.Register(() => null!);
            }
        }

        [TestMethod]
        public void SendVerifyCodeAsync_ExistingUnusedNotExpired_ReusesCode()
        {
            SmsConfigResolver.Register(() => new SmsOption
            {
                Provider = "ZhuTong",
                ZhuTong = new ZhuTongSmsSetting { UserName = "test", Password = "test", Signature = "test", TemplateId = 1 },
                VerifyCodeExpireMinutes = 5
            });
            try
            {
                var phone = "13800138011";
                var key = CacheConst.KeyPhoneVerCode + phone;
                var existingCode = "1234";
                MemoryCache.Instance.Set(key, new PhoneVerifyCodeInfo
                {
                    Code = existingCode,
                    CreateTime = DateTime.Now,
                    IsUsed = false,
                    ExpireMinutes = 5
                }, TimeSpan.FromMinutes(5));

                try
                {
                    SmsExtention.SendVerifyCodeAsync(phone).GetAwaiter().GetResult();
                }
                catch
                {
                    // SMS 发送在测试环境失败（假凭据），忽略
                }

                var cached = MemoryCache.Instance.Get<PhoneVerifyCodeInfo>(key);
                Assert.IsNotNull(cached);
                Assert.AreEqual(existingCode, cached.Code, "应复用已有验证码");
                Assert.IsFalse(cached.IsUsed);
                MemoryCache.Instance.Delete(key);
            }
            finally
            {
                SmsConfigResolver.Register(() => null!);
            }
        }

        [TestMethod]
        public void SendVerifyCodeAsync_ExistingUsed_GeneratesNewCode()
        {
            SmsConfigResolver.Register(() => new SmsOption
            {
                Provider = "ZhuTong",
                ZhuTong = new ZhuTongSmsSetting { UserName = "test", Password = "test", Signature = "test", TemplateId = 1 },
                VerifyCodeExpireMinutes = 5
            });
            try
            {
                var phone = "13800138012";
                var key = CacheConst.KeyPhoneVerCode + phone;
                MemoryCache.Instance.Set(key, new PhoneVerifyCodeInfo
                {
                    Code = "1111",
                    CreateTime = DateTime.Now,
                    IsUsed = true,
                    ExpireMinutes = 5
                }, TimeSpan.FromMinutes(5));

                try
                {
                    SmsExtention.SendVerifyCodeAsync(phone).GetAwaiter().GetResult();
                }
                catch
                {
                    // SMS 发送在测试环境失败（假凭据），忽略
                }

                var cached = MemoryCache.Instance.Get<PhoneVerifyCodeInfo>(key);
                Assert.IsNotNull(cached);
                Assert.AreNotEqual("1111", cached.Code, "已使用的验证码应被替换为新码");
                Assert.IsFalse(cached.IsUsed);
                MemoryCache.Instance.Delete(key);
            }
            finally
            {
                SmsConfigResolver.Register(() => null!);
            }
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
