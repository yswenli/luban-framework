# MFA Utility Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement TOTP-based MFA utility class in LuBan.Common using Otp.NET library

**Architecture:** Single static utility class OptUtil with 5 methods for secret generation, TOTP code generation/validation, and QR code generation. Follows existing utility pattern (PasswordUtil, TokenUtil).

**Tech Stack:** .NET 8.0, Otp.NET, ZXing.SkiaSharp, SkiaSharp, xUnit

---

## File Structure

**Create:**
- `LuBan.Common/OptUtil.cs` - Core TOTP utility class
- `LuBan.XTestProject/OptUtilTests.cs` - Unit tests

**Modify:**
- `LuBan.Common/LuBan.Common.csproj` - Add Otp.NET package reference
- `LuBan.Common/RandomUtil.cs` - Update comments

---

## Task 1: Add Otp.NET NuGet Package

**Files:**
- Modify: `LuBan.Common/LuBan.Common.csproj`

- [ ] **Step 1: Add Otp.NET package reference**

Edit `LuBan.Common/LuBan.Common.csproj`, add after line 33 (Encrypt.Library):

```xml
    <PackageReference Include="Otp.NET" Version="1.10.0" />
```

- [ ] **Step 2: Restore packages**

Run: `dotnet restore LuBan.Common/LuBan.Common.csproj`
Expected: Success

- [ ] **Step 3: Commit**

```bash
git add LuBan.Common/LuBan.Common.csproj
git commit -m "feat: add Otp.NET package reference for TOTP support"
```

---

## Task 2: Create OptUtil Class Skeleton

**Files:**
- Create: `LuBan.Common/OptUtil.cs`

- [ ] **Step 1: Create OptUtil.cs file with header and class skeleton**

Create `LuBan.Common/OptUtil.cs`:

```csharp
/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*文件名： OptUtil
*描述：TOTP双因素认证工具类
*****************************************************************************/
using OtpNet;

namespace LuBan.Common;

/// <summary>
/// TOTP双因素认证工具类
/// </summary>
public static class OptUtil
{
    // Constants
    private const int DefaultSecretLength = 20;
    private const int DefaultCodeDigits = 6;
    private const int DefaultTimeStep = 30;
    private const int MinSecretLength = 16;
    private const int MaxSecretLength = 64;
    private const int MinCodeDigits = 6;
    private const int MaxCodeDigits = 8;
    private const int MinQrSize = 100;
    private const int MaxQrSize = 2000;
    
    // Methods will be implemented in following tasks
}
```

- [ ] **Step 2: Verify file compiles**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build SUCCESS

- [ ] **Step 3: Commit**

```bash
git add LuBan.Common/OptUtil.cs
git commit -m "feat: create OptUtil class skeleton"
```

---

## Task 3: Implement GenerateSecret Method

**Files:**
- Modify: `LuBan.Common/OptUtil.cs`
- Create: `LuBan.XTestProject/OptUtilTests.cs` (partial)

- [ ] **Step 1: Write failing test for GenerateSecret**

Create `LuBan.XTestProject/OptUtilTests.cs`:

```csharp
using Xunit;

namespace LuBan.Common.Tests;

public class OptUtilTests
{
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
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~GenerateSecret" --no-build`
Expected: Tests FAIL (method not implemented)

- [ ] **Step 3: Implement GenerateSecret**

Edit `LuBan.Common/OptUtil.cs`, add method before closing brace:

```csharp
    /// <summary>
    /// Generate secure random secret key for MFA setup
    /// </summary>
    /// <param name="length">Secret length in bytes (default 20 for SHA-1)</param>
    /// <returns>Base32-encoded secret (standard for authenticator apps)</returns>
    public static string GenerateSecret(int length = DefaultSecretLength)
    {
        if (length < MinSecretLength || length > MaxSecretLength)
            throw new ArgumentOutOfRangeException(nameof(length), 
                $"Length must be between {MinSecretLength} and {MaxSecretLength} bytes");
        
        var key = KeyGeneration.GenerateRandomKey(length);
        return Base32Encoding.ToString(key);
    }
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~GenerateSecret"`
Expected: All tests PASS

- [ ] **Step 5: Commit**

```bash
git add LuBan.Common/OptUtil.cs LuBan.XTestProject/OptUtilTests.cs
git commit -m "feat: implement OptUtil.GenerateSecret with tests"
```

---

## Task 4: Implement GetTotp Method

**Files:**
- Modify: `LuBan.Common/OptUtil.cs`
- Modify: `LuBan.XTestProject/OptUtilTests.cs`

- [ ] **Step 1: Write failing tests for GetTotp**

Edit `LuBan.XTestProject/OptUtilTests.cs`, add new test class section before closing brace:

```csharp
    
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
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~GetTotp" --no-build`
Expected: Tests FAIL (method not implemented)

- [ ] **Step 3: Implement GetTotp**

Edit `LuBan.Common/OptUtil.cs`, add method after GenerateSecret:

```csharp
    
    /// <summary>
    /// Generate current TOTP code
    /// </summary>
    /// <param name="secret">Base32-encoded secret</param>
    /// <param name="digits">Code length (default 6)</param>
    /// <returns>6-digit TOTP code</returns>
    public static string GetTotp(string secret, int digits = DefaultCodeDigits)
    {
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentNullException(nameof(secret), "Secret cannot be null or empty");
        
        if (digits < MinCodeDigits || digits > MaxCodeDigits)
            throw new ArgumentOutOfRangeException(nameof(digits), 
                $"Digits must be between {MinCodeDigits} and {MaxCodeDigits}");
        
        try
        {
            var key = Base32Encoding.ToBytes(secret);
            var totp = new Totp(key, step: DefaultTimeStep, totpSize: digits);
            return totp.ComputeTotp();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid secret format. Expected Base32-encoded string.", nameof(secret), ex);
        }
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~GetTotp"`
Expected: All tests PASS

- [ ] **Step 5: Commit**

```bash
git add LuBan.Common/OptUtil.cs LuBan.XTestProject/OptUtilTests.cs
git commit -m "feat: implement OptUtil.GetTotp with tests"
```

---

## Task 5: Implement ValidateTotp Method

**Files:**
- Modify: `LuBan.Common/OptUtil.cs`
- Modify: `LuBan.XTestProject/OptUtilTests.cs`

- [ ] **Step 1: Write failing tests for ValidateTotp**

Edit `LuBan.XTestProject/OptUtilTests.cs`, add after GetTotp tests:

```csharp
    
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
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~ValidateTotp" --no-build`
Expected: Tests FAIL (method not implemented)

- [ ] **Step 3: Implement ValidateTotp**

Edit `LuBan.Common/OptUtil.cs`, add method after GetTotp:

```csharp
    
    /// <summary>
    /// Validate TOTP code with ±1 step tolerance
    /// </summary>
    /// <param name="code">User-provided code</param>
    /// <param name="secret">Base32-encoded secret</param>
    /// <param name="digits">Expected code length (default 6)</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool ValidateTotp(string code, string secret, int digits = DefaultCodeDigits)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(secret))
            return false;
        
        if (code.Length != digits)
            return false;
        
        try
        {
            var key = Base32Encoding.ToBytes(secret);
            var totp = new Totp(key, step: DefaultTimeStep, totpSize: digits);
            long timeStepMatched;
            return totp.VerifyTotp(code, out timeStepMatched, new VerificationWindow(1, 1));
        }
        catch
        {
            return false;
        }
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~ValidateTotp"`
Expected: All tests PASS

- [ ] **Step 5: Commit**

```bash
git add LuBan.Common/OptUtil.cs LuBan.XTestProject/OptUtilTests.cs
git commit -m "feat: implement OptUtil.ValidateTotp with tests"
```

---

## Task 6: Implement GenerateOtpAuthUri Method

**Files:**
- Modify: `LuBan.Common/OptUtil.cs`
- Modify: `LuBan.XTestProject/OptUtilTests.cs`

- [ ] **Step 1: Write failing tests for GenerateOtpAuthUri**

Edit `LuBan.XTestProject/OptUtilTests.cs`, add after ValidateTotp tests:

```csharp
    
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
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~GenerateOtpAuthUri" --no-build`
Expected: Tests FAIL (method not implemented)

- [ ] **Step 3: Implement GenerateOtpAuthUri**

Edit `LuBan.Common/OptUtil.cs`, add method after ValidateTotp:

```csharp
    
    /// <summary>
    /// Generate otpauth:// URI for authenticator app setup
    /// </summary>
    /// <param name="secret">Base32-encoded secret</param>
    /// <param name="email">User email/account identifier</param>
    /// <param name="issuer">Application name</param>
    /// <returns>otpauth://totp/ URI string</returns>
    public static string GenerateOtpAuthUri(string secret, string email, string issuer)
    {
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentNullException(nameof(secret));
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrEmpty(issuer))
            throw new ArgumentNullException(nameof(issuer));
        
        var escapedIssuer = Uri.EscapeDataString(issuer);
        var escapedEmail = Uri.EscapeDataString(email);
        
        return $"otpauth://totp/{escapedIssuer}:{escapedEmail}?secret={secret}&issuer={escapedIssuer}";
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~GenerateOtpAuthUri"`
Expected: All tests PASS

- [ ] **Step 5: Commit**

```bash
git add LuBan.Common/OptUtil.cs LuBan.XTestProject/OptUtilTests.cs
git commit -m "feat: implement OptUtil.GenerateOtpAuthUri with tests"
```

---

## Task 7: Implement GenerateQrCode Method

**Files:**
- Modify: `LuBan.Common/OptUtil.cs`
- Modify: `LuBan.XTestProject/OptUtilTests.cs`

- [ ] **Step 1: Write failing tests for GenerateQrCode**

Edit `LuBan.XTestProject/OptUtilTests.cs`, add after GenerateOtpAuthUri tests:

```csharp
    
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
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~GenerateQrCode" --no-build`
Expected: Tests FAIL (method not implemented)

- [ ] **Step 3: Implement GenerateQrCode**

Edit `LuBan.Common/OptUtil.cs`, add method after GenerateOtpAuthUri:

```csharp
    
    /// <summary>
    /// Generate QR code image for authenticator app setup
    /// </summary>
    /// <param name="secret">Base32-encoded secret</param>
    /// <param name="email">User email/account identifier</param>
    /// <param name="issuer">Application name</param>
    /// <param name="width">QR code width in pixels (default 300)</param>
    /// <param name="height">QR code height in pixels (default 300)</param>
    /// <returns>PNG image as byte array</returns>
    public static byte[] GenerateQrCode(string secret, string email, string issuer, int width = 300, int height = 300)
    {
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentNullException(nameof(secret));
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrEmpty(issuer))
            throw new ArgumentNullException(nameof(issuer));
        
        if (width < MinQrSize || width > MaxQrSize)
            throw new ArgumentOutOfRangeException(nameof(width), 
                $"Width must be between {MinQrSize} and {MaxQrSize}");
        if (height < MinQrSize || height > MaxQrSize)
            throw new ArgumentOutOfRangeException(nameof(height), 
                $"Height must be between {MinQrSize} and {MaxQrSize}");
        
        var uri = GenerateOtpAuthUri(secret, email, issuer);
        
        var writer = new ZXing.SkiaSharp.BarcodeWriter
        {
            Format = ZXing.BarcodeFormat.QR_CODE,
            Options = new ZXing.Common.EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 1
            }
        };
        
        using var bitmap = writer.Write(uri);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var stream = new MemoryStream();
        image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).SaveTo(stream);
        return stream.ToArray();
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~GenerateQrCode"`
Expected: All tests PASS

- [ ] **Step 5: Commit**

```bash
git add LuBan.Common/OptUtil.cs LuBan.XTestProject/OptUtilTests.cs
git commit -m "feat: implement OptUtil.GenerateQrCode with tests"
```

---

## Task 8: Add Integration Tests with RandomUtil

**Files:**
- Modify: `LuBan.XTestProject/OptUtilTests.cs`

- [ ] **Step 1: Write integration tests with RandomUtil**

Edit `LuBan.XTestProject/OptUtilTests.cs`, add at the end before closing brace:

```csharp
    
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
```

- [ ] **Step 2: Run integration tests**

Run: `dotnet test --filter "FullyQualifiedName~RandomUtil"`
Expected: All tests PASS

- [ ] **Step 3: Commit**

```bash
git add LuBan.XTestProject/OptUtilTests.cs
git commit -m "test: add integration tests for OptUtil and RandomUtil"
```

---

## Task 9: Update RandomUtil Comments

**Files:**
- Modify: `LuBan.Common/RandomUtil.cs`

- [ ] **Step 1: Update RandomUtil GetWFACode comment**

Edit `LuBan.Common/RandomUtil.cs`, replace line 122-128:

```csharp
        /// <summary>
        /// 获取动态码
        /// </summary>
        /// <param name="key">Base32-encoded secret key</param>
        /// <param name="size">Code length (default 6)</param>
        /// <returns>Current TOTP code</returns>
        public static string GetWFACode(string key, int size = 6)
        {
            return OptUtil.GetTotp(key, size);
        }
```

- [ ] **Step 2: Update RandomUtil ValideWFACode comment**

Replace line 130-142:

```csharp
        /// <summary>
        /// 校验动态码
        /// </summary>
        /// <param name="valideCode">User-provided TOTP code</param>
        /// <param name="key">Base32-encoded secret key</param>
        /// <param name="size">Expected code length (default 6)</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValideWFACode(string valideCode, string key, int size = 6)
        {
            return OptUtil.ValidateTotp(valideCode, key, size);
        }
```

- [ ] **Step 3: Verify all tests still pass**

Run: `dotnet test`
Expected: All tests PASS

- [ ] **Step 4: Commit**

```bash
git add LuBan.Common/RandomUtil.cs
git commit -m "docs: update RandomUtil comments to reference OptUtil"
```

---

## Task 10: Final Verification and Documentation

**Files:**
- None (verification only)

- [ ] **Step 1: Run all tests**

Run: `dotnet test --verbosity normal`
Expected: All tests PASS

- [ ] **Step 2: Build entire solution**

Run: `dotnet build`
Expected: Build SUCCESS

- [ ] **Step 3: Verify code coverage**

Run: `dotnet test --collect:"XPlat Code Coverage"`
Expected: Coverage report generated, >90% for OptUtil

- [ ] **Step 4: Manual verification - Generate QR code and scan**

Create a temporary test program to verify QR code works:

```csharp
// Temporary verification code (not committed)
var secret = OptUtil.GenerateSecret();
Console.WriteLine($"Secret: {secret}");
Console.WriteLine($"Current code: {OptUtil.GetTotp(secret)}");
var qrCode = OptUtil.GenerateQrCode(secret, "test@example.com", "TestApp");
File.WriteAllBytes("test-qr.png", qrCode);
Console.WriteLine("QR code saved to test-qr.png - scan with Google Authenticator");
```

Scan the generated QR code with Google Authenticator app and verify:
- QR code is readable
- Generated codes match OptUtil.GetTotp()

- [ ] **Step 5: Clean up temporary files**

```bash
Remove-Item test-qr.png -ErrorAction SilentlyContinue
```

- [ ] **Step 6: Final commit if needed**

Check for any uncommitted changes:
```bash
git status
```

If clean, no action needed. If changes exist:
```bash
git add .
git commit -m "chore: final cleanup"
```

---

## Success Criteria Checklist

After completing all tasks, verify:

- [ ] All unit tests pass (`dotnet test`)
- [ ] Integration with RandomUtil works
- [ ] QR codes scan successfully with Google Authenticator
- [ ] TOTP codes validate correctly
- [ ] Code coverage >90%
- [ ] No breaking changes to existing code
- [ ] Follows LuBan.Common utility patterns
- [ ] Documentation is complete

---

## Troubleshooting

**If tests fail:**
1. Check Otp.NET package is installed correctly
2. Verify ZXing.SkiaSharp is referenced
3. Check method signatures match spec
4. Verify error handling matches test expectations

**If QR code doesn't scan:**
1. Verify ZXing.SkiaSharp is generating valid QR codes
2. Check otpauth:// URI format is correct
3. Ensure Base32 secret is properly encoded

**If integration tests fail:**
1. Verify RandomUtil.cs changes are correct
2. Check OptUtil methods have correct signatures
3. Ensure both use same TOTP parameters

---

## References

- Design spec: `docs/superpowers/specs/2026-07-17-mfa-utility-design.md`
- Otp.NET docs: https://github.com/ks6000/Otp.NET
- RFC 6238: https://tools.ietf.org/html/rfc6238
- Google Authenticator URI format: https://github.com/google/google-authenticator/wiki/Key-Uri-Format