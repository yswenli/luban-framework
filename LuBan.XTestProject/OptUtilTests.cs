using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace LuBan.Common.Tests;

[TestClass]
public class OptUtilTests
{
    // GenerateSecret tests
    [TestMethod]
    public void GenerateSecret_ShouldReturnValidBase32String()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.IsNotNull(secret);
        Assert.IsTrue(secret.Length > 0);
        Assert.IsTrue(Regex.IsMatch(secret, "^[A-Z2-7]+=*$", RegexOptions.IgnoreCase));
    }

    [TestMethod]
    public void GenerateSecret_WithCustomLength_ShouldReturnValidSecret()
    {
        var secret = OptUtil.GenerateSecret(32);
        Assert.IsNotNull(secret);
        Assert.IsTrue(secret.Length > 0);
        Assert.IsTrue(Regex.IsMatch(secret, "^[A-Z2-7]+=*$", RegexOptions.IgnoreCase));
    }

    [TestMethod]
    public void GenerateSecret_WithInvalidLength_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OptUtil.GenerateSecret(10));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OptUtil.GenerateSecret(100));
    }

    [TestMethod]
    public void GenerateSecret_ShouldGenerateUniqueSecrets()
    {
        var secret1 = OptUtil.GenerateSecret();
        var secret2 = OptUtil.GenerateSecret();
        Assert.AreNotEqual(secret1, secret2);
    }

    // GetTotp tests
    [TestMethod]
    public void GetTotp_ShouldReturn6DigitCode()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret);
        Assert.IsNotNull(code);
        Assert.AreEqual(6, code.Length);
        Assert.IsTrue(Regex.IsMatch(code, "^[0-9]{6}$"));
    }

    [TestMethod]
    public void GetTotp_WithSameSecret_ShouldReturnSameCodeAtSameTime()
    {
        var secret = OptUtil.GenerateSecret();
        var code1 = OptUtil.GetTotp(secret);
        var code2 = OptUtil.GetTotp(secret);
        Assert.AreEqual(code1, code2);
    }

    [TestMethod]
    public void GetTotp_With8Digits_ShouldReturn8DigitCode()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret, 8);
        Assert.AreEqual(8, code.Length);
        Assert.IsTrue(Regex.IsMatch(code, "^[0-9]{8}$"));
    }

    [TestMethod]
    public void GetTotp_WithNullSecret_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => OptUtil.GetTotp(null!));
    }

    [TestMethod]
    public void GetTotp_WithEmptySecret_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => OptUtil.GetTotp(string.Empty));
    }

    [TestMethod]
    public void GetTotp_WithInvalidDigits_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OptUtil.GetTotp(secret, 5));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OptUtil.GetTotp(secret, 9));
    }

    [TestMethod]
    public void GetTotp_WithInvalidSecretFormat_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() => OptUtil.GetTotp("invalid-secret!@#"));
    }

    // ValidateTotp tests
    [TestMethod]
    public void ValidateTotp_WithValidCode_ShouldReturnTrue()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret);
        var result = OptUtil.ValidateTotp(code, secret);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ValidateTotp_WithInvalidCode_ShouldReturnFalse()
    {
        var secret = OptUtil.GenerateSecret();
        var result = OptUtil.ValidateTotp("000000", secret);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateTotp_WithNullCode_ShouldReturnFalse()
    {
        var secret = OptUtil.GenerateSecret();
        var result = OptUtil.ValidateTotp(null!, secret);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateTotp_WithEmptyCode_ShouldReturnFalse()
    {
        var secret = OptUtil.GenerateSecret();
        var result = OptUtil.ValidateTotp(string.Empty, secret);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateTotp_WithNullSecret_ShouldReturnFalse()
    {
        var code = OptUtil.GetTotp(OptUtil.GenerateSecret());
        var result = OptUtil.ValidateTotp(code, null!);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateTotp_WithWrongLengthCode_ShouldReturnFalse()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.IsFalse(OptUtil.ValidateTotp("12345", secret));
        Assert.IsFalse(OptUtil.ValidateTotp("1234567", secret));
    }

    [TestMethod]
    public void ValidateTotp_WithInvalidSecret_ShouldReturnFalse()
    {
        Assert.IsFalse(OptUtil.ValidateTotp("123456", "invalid-secret!@#"));
    }

    [TestMethod]
    public void ValidateTotp_With8Digits_ShouldWork()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret, 8);
        var result = OptUtil.ValidateTotp(code, secret, 8);
        Assert.IsTrue(result);
    }

    // GenerateOtpAuthUri tests
    [TestMethod]
    public void GenerateOtpAuthUri_ShouldReturnValidUriFormat()
    {
        var secret = OptUtil.GenerateSecret();
        var uri = OptUtil.GenerateOtpAuthUri(secret, "test@example.com", "TestApp");
        Assert.IsTrue(uri.StartsWith("otpauth://totp/"));
        Assert.IsTrue(uri.Contains("secret=" + secret));
        Assert.IsTrue(uri.Contains("issuer=TestApp"));
    }

    [TestMethod]
    public void GenerateOtpAuthUri_ShouldEscapeSpecialCharacters()
    {
        var secret = OptUtil.GenerateSecret();
        var uri = OptUtil.GenerateOtpAuthUri(secret, "user+test@example.com", "Test App");
        Assert.IsTrue(uri.Contains("Test%20App"));
        Assert.IsTrue(uri.Contains("user%2Btest"));
    }

    [TestMethod]
    public void GenerateOtpAuthUri_WithNullSecret_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => OptUtil.GenerateOtpAuthUri(null!, "test@example.com", "TestApp"));
    }

    [TestMethod]
    public void GenerateOtpAuthUri_WithNullEmail_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.ThrowsException<ArgumentNullException>(() => OptUtil.GenerateOtpAuthUri(secret, null!, "TestApp"));
    }

    [TestMethod]
    public void GenerateOtpAuthUri_WithNullIssuer_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.ThrowsException<ArgumentNullException>(() => OptUtil.GenerateOtpAuthUri(secret, "test@example.com", null!));
    }

    // GenerateQrCode tests
    [TestMethod]
    public void GenerateQrCode_ShouldReturnPngImage()
    {
        var secret = OptUtil.GenerateSecret();
        var qrCode = OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp");
        Assert.IsNotNull(qrCode);
        Assert.IsTrue(qrCode.Length > 0);
        Assert.AreEqual(0x89, qrCode[0]);
        Assert.AreEqual(0x50, qrCode[1]);
        Assert.AreEqual(0x4E, qrCode[2]);
        Assert.AreEqual(0x47, qrCode[3]);
    }

    [TestMethod]
    public void GenerateQrCode_WithCustomSize_ShouldReturnValidImage()
    {
        var secret = OptUtil.GenerateSecret();
        var qrCode = OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 500, 500);
        Assert.IsNotNull(qrCode);
        Assert.IsTrue(qrCode.Length > 0);
        Assert.AreEqual(0x89, qrCode[0]);
    }

    [TestMethod]
    public void GenerateQrCode_WithInvalidWidth_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 50, 300));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 3000, 300));
    }

    [TestMethod]
    public void GenerateQrCode_WithInvalidHeight_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 300, 50));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 300, 3000));
    }

    [TestMethod]
    public void GenerateQrCode_WithNullParameters_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => OptUtil.GenerateQrCode(null!, "test@example.com", "TestApp"));
        var secret = OptUtil.GenerateSecret();
        Assert.ThrowsException<ArgumentNullException>(() => OptUtil.GenerateQrCode(secret, null!, "TestApp"));
        Assert.ThrowsException<ArgumentNullException>(() => OptUtil.GenerateQrCode(secret, "test@example.com", null!));
    }

    // Integration tests with RandomUtil
    [TestMethod]
    public void RandomUtil_GetWFACode_ShouldWorkWithOptUtil()
    {
        var secret = OptUtil.GenerateSecret();
        var code1 = RandomUtil.GetWFACode(secret);
        var code2 = OptUtil.GetTotp(secret);
        Assert.AreEqual(code1, code2);
    }

    [TestMethod]
    public void RandomUtil_ValideWFACode_ShouldWorkWithOptUtil()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret);
        var result = RandomUtil.ValideWFACode(code, secret);
        Assert.IsTrue(result);
    }
}