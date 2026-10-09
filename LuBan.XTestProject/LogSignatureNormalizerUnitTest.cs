using LuBan.AIAgent.LogAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class LogSignatureNormalizerUnitTest
{
    [TestMethod]
    public void Replaces_Guid()
        => Assert.AreEqual(
            "id <GUID> done",
            LogSignatureNormalizer.Normalize("id 12345678-1234-1234-1234-1234567890ab done"));

    [TestMethod]
    public void Replaces_Ip_Then_Numbers()
        => Assert.AreEqual(
            "connect <IP> port <N>",
            LogSignatureNormalizer.Normalize("connect 192.168.1.10 port 8080"));

    [TestMethod]
    public void Replaces_Hex()
        => Assert.AreEqual("ptr <HEX>", LogSignatureNormalizer.Normalize("ptr 0xDEADBEEF"));

    [TestMethod]
    public void Replaces_Quoted_Strings()
        => Assert.AreEqual(
            "user <S> said <S>",
            LogSignatureNormalizer.Normalize("user \"alice\" said 'hello world'"));

    [TestMethod]
    public void Collapses_Whitespace_And_Trims()
        => Assert.AreEqual("a b c", LogSignatureNormalizer.Normalize("  a\t b\r\n c  "));

    [TestMethod]
    public void Truncates_To_MaxChars()
        => Assert.AreEqual(500, LogSignatureNormalizer.Normalize(new string('a', 1000)).Length);
}
