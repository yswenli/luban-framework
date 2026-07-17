using Xunit;

namespace LuBan.Common.Tests;

public class OptUtilTests
{
    // GenerateSecret tests
    [Fact]
    public void GenerateSecret_ShouldReturnValidBase32String()
    {
        var secret = OptUtil.GenerateSecret();

        Assert.NotNull(secret);
        Assert.True(secret.Length > 0);
        Assert.Matches("^[A-Z2-7]+$", secret);
    }

    [Fact]
    public void GenerateSecret_WithCustomLength_ShouldReturnValidSecret()
    {
        var secret = OptUtil.GenerateSecret(32);

        Assert.NotNull(secret);
        Assert.True(secret.Length > 0);
        Assert.Matches("^[A-Z2-7]+$", secret);
    }

    [Fact]
    public void GenerateSecret_WithInvalidLength_ShouldThrowException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OptUtil.GenerateSecret(10));
        Assert.Throws<ArgumentOutOfRangeException>(() => OptUtil.GenerateSecret(100));
    }

    [Fact]
    public void GenerateSecret_ShouldGenerateUniqueSecrets()
    {
        var secret1 = OptUtil.GenerateSecret();
        var secret2 = OptUtil.GenerateSecret();

        Assert.NotEqual(secret1, secret2);
    }

    // GetTotp tests
    [Fact]
    public void GetTotp_ShouldReturn6DigitCode()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret);

        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        Assert.Matches("^[0-9]{6}$", code);
    }

    [Fact]
    public void GetTotp_WithSameSecret_ShouldReturnSameCodeAtSameTime()
    {
        var secret = OptUtil.GenerateSecret();
        var code1 = OptUtil.GetTotp(secret);
        var code2 = OptUtil.GetTotp(secret);

        Assert.Equal(code1, code2);
    }

    [Fact]
    public void GetTotp_With8Digits_ShouldReturn8DigitCode()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret, 8);

        Assert.Equal(8, code.Length);
        Assert.Matches("^[0-9]{8}$", code);
    }

    [Fact]
    public void GetTotp_WithNullSecret_ShouldThrowException()
    {
        Assert.Throws<ArgumentNullException>(() => OptUtil.GetTotp(null!));
    }

    [Fact]
    public void GetTotp_WithEmptySecret_ShouldThrowException()
    {
        Assert.Throws<ArgumentNullException>(() => OptUtil.GetTotp(string.Empty));
    }

    [Fact]
    public void GetTotp_WithInvalidDigits_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.Throws<ArgumentOutOfRangeException>(() => OptUtil.GetTotp(secret, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => OptUtil.GetTotp(secret, 9));
    }

    [Fact]
    public void GetTotp_WithInvalidSecretFormat_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => OptUtil.GetTotp("invalid-secret!@#"));
    }

    // ValidateTotp tests
    [Fact]
    public void ValidateTotp_WithValidCode_ShouldReturnTrue()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret);
        var result = OptUtil.ValidateTotp(code, secret);

        Assert.True(result);
    }

    [Fact]
    public void ValidateTotp_WithInvalidCode_ShouldReturnFalse()
    {
        var secret = OptUtil.GenerateSecret();
        var result = OptUtil.ValidateTotp("000000", secret);

        Assert.False(result);
    }

    [Fact]
    public void ValidateTotp_WithNullCode_ShouldReturnFalse()
    {
        var secret = OptUtil.GenerateSecret();
        var result = OptUtil.ValidateTotp(null!, secret);

        Assert.False(result);
    }

    [Fact]
    public void ValidateTotp_WithEmptyCode_ShouldReturnFalse()
    {
        var secret = OptUtil.GenerateSecret();
        var result = OptUtil.ValidateTotp(string.Empty, secret);

        Assert.False(result);
    }

    [Fact]
    public void ValidateTotp_WithNullSecret_ShouldReturnFalse()
    {
        var code = OptUtil.GetTotp(OptUtil.GenerateSecret());
        var result = OptUtil.ValidateTotp(code, null!);

        Assert.False(result);
    }

    [Fact]
    public void ValidateTotp_WithWrongLengthCode_ShouldReturnFalse()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.False(OptUtil.ValidateTotp("12345", secret));  // 5 digits
        Assert.False(OptUtil.ValidateTotp("1234567", secret)); // 7 digits
    }

    [Fact]
    public void ValidateTotp_WithInvalidSecret_ShouldReturnFalse()
    {
        Assert.False(OptUtil.ValidateTotp("123456", "invalid-secret!@#"));
    }

    [Fact]
    public void ValidateTotp_With8Digits_ShouldWork()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret, 8);
        var result = OptUtil.ValidateTotp(code, secret, 8);

        Assert.True(result);
    }

    // GenerateOtpAuthUri tests
    [Fact]
    public void GenerateOtpAuthUri_ShouldReturnValidUriFormat()
    {
        var secret = OptUtil.GenerateSecret();
        var uri = OptUtil.GenerateOtpAuthUri(secret, "test@example.com", "TestApp");

        Assert.StartsWith("otpauth://totp/", uri);
        Assert.Contains("secret=" + secret, uri);
        Assert.Contains("issuer=TestApp", uri);
    }

    [Fact]
    public void GenerateOtpAuthUri_ShouldEscapeSpecialCharacters()
    {
        var secret = OptUtil.GenerateSecret();
        var uri = OptUtil.GenerateOtpAuthUri(secret, "user+test@example.com", "Test App");

        Assert.Contains("Test%20App", uri);
        Assert.Contains("user%2Btest", uri);
    }

    [Fact]
    public void GenerateOtpAuthUri_WithNullSecret_ShouldThrowException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            OptUtil.GenerateOtpAuthUri(null!, "test@example.com", "TestApp"));
    }

    [Fact]
    public void GenerateOtpAuthUri_WithNullEmail_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.Throws<ArgumentNullException>(() =>
            OptUtil.GenerateOtpAuthUri(secret, null!, "TestApp"));
    }

    [Fact]
    public void GenerateOtpAuthUri_WithNullIssuer_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.Throws<ArgumentNullException>(() =>
            OptUtil.GenerateOtpAuthUri(secret, "test@example.com", null!));
    }

    // GenerateQrCode tests
    [Fact]
    public void GenerateQrCode_ShouldReturnPngImage()
    {
        var secret = OptUtil.GenerateSecret();
        var qrCode = OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp");

        Assert.NotNull(qrCode);
        Assert.True(qrCode.Length > 0);
        // PNG magic number: 89 50 4E 47 (0x89, 'P', 'N', 'G')
        Assert.Equal(0x89, qrCode[0]);
        Assert.Equal(0x50, qrCode[1]); // 'P'
        Assert.Equal(0x4E, qrCode[2]); // 'N'
        Assert.Equal(0x47, qrCode[3]); // 'G'
    }

    [Fact]
    public void GenerateQrCode_WithCustomSize_ShouldReturnValidImage()
    {
        var secret = OptUtil.GenerateSecret();
        var qrCode = OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 500, 500);

        Assert.NotNull(qrCode);
        Assert.True(qrCode.Length > 0);
        Assert.Equal(0x89, qrCode[0]); // PNG magic number
    }

    [Fact]
    public void GenerateQrCode_WithInvalidWidth_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 50, 300));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 3000, 300));
    }

    [Fact]
    public void GenerateQrCode_WithInvalidHeight_ShouldThrowException()
    {
        var secret = OptUtil.GenerateSecret();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 300, 50));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp", 300, 3000));
    }

    [Fact]
    public void GenerateQrCode_WithNullParameters_ShouldThrowException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            OptUtil.GenerateQrCode(null!, "test@example.com", "TestApp"));

        var secret = OptUtil.GenerateSecret();
        Assert.Throws<ArgumentNullException>(() =>
            OptUtil.GenerateQrCode(secret, null!, "TestApp"));
        Assert.Throws<ArgumentNullException>(() =>
            OptUtil.GenerateQrCode(secret, "test@example.com", null!));
    }

    // Integration tests with RandomUtil
    [Fact]
    public void RandomUtil_GetWFACode_ShouldWorkWithOptUtil()
    {
        var secret = OptUtil.GenerateSecret();
        var code1 = RandomUtil.GetWFACode(secret);
        var code2 = OptUtil.GetTotp(secret);

        Assert.Equal(code1, code2);
    }

    [Fact]
    public void RandomUtil_ValideWFACode_ShouldWorkWithOptUtil()
    {
        var secret = OptUtil.GenerateSecret();
        var code = OptUtil.GetTotp(secret);
        var result = RandomUtil.ValideWFACode(code, secret);

        Assert.True(result);
    }
}