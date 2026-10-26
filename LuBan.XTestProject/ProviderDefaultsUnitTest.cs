using LuBan.AIAgent.Configuration.Providers;

namespace LuBan.XTestProject;

[TestClass]
public class ProviderDefaultsUnitTest
{
    [TestMethod]
    public void GetDefaultBaseUrl_KnownProvider_ReturnsUrl()
    {
        Assert.AreEqual("https://api.deepseek.com/v1/", ProviderDefaults.GetDefaultBaseUrl("deepseek"));
    }

    [TestMethod]
    public void GetDefaultBaseUrl_UnknownProvider_ReturnsNull()
    {
        Assert.IsNull(ProviderDefaults.GetDefaultBaseUrl("no-such-provider"));
    }

    [TestMethod]
    public void GetDefaultModels_KnownProvider_NotEmpty()
    {
        Assert.IsNotEmpty(ProviderDefaults.GetDefaultModels("openai").ToList());
    }

    [TestMethod]
    public void GetDefaultBaseUrl_IsCaseInsensitive()
    {
        Assert.AreEqual(ProviderDefaults.GetDefaultBaseUrl("openai"), ProviderDefaults.GetDefaultBaseUrl("OpenAI"));
    }

    [TestMethod]
    public void ProviderIds_ContainsOpenAi()
    {
        CollectionAssert.Contains(ProviderDefaults.ProviderIds.ToList(), "openai");
    }
}