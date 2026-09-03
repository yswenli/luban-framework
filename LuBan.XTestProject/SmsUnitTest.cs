using System.Text.Json;
using LuBan.Common.Sms;
// Task 4/5 引入 Providers 后取消注释
//using LuBan.Common.Sms.Providers;

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
    }
}
