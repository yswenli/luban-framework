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

    [TestMethod]
    public void Combined_Line_Applies_Ordering()
        => Assert.AreEqual(
            "guid <GUID> ip <IP> hex <HEX> num <N> q <S>",
            LogSignatureNormalizer.Normalize("guid 12345678-1234-1234-1234-1234567890ab ip 10.0.0.1 hex 0xFF num 42 q \"a1b2\""));

    [TestMethod]
    public void Hex_Is_Not_Consumed_By_Number()
        => Assert.AreEqual("a <HEX> b <N>", LogSignatureNormalizer.Normalize("a 0x1F b 42"));

    [TestMethod]
    public void Quoted_Digits_Are_Single_Substitution()
        => Assert.AreEqual("v <S>", LogSignatureNormalizer.Normalize("v \"a1b2\""));

    [TestMethod]
    public void NonPositive_MaxChars_Falls_Back_To_Default()
    {
        Assert.AreEqual(500, LogSignatureNormalizer.Normalize(new string('a', 1000), 0).Length);
        Assert.AreEqual(500, LogSignatureNormalizer.Normalize(new string('a', 1000), -5).Length);
    }

    [TestMethod]
    public void Whitespace_Only_Returns_Empty()
        => Assert.AreEqual(string.Empty, LogSignatureNormalizer.Normalize("   \t  "));
}
