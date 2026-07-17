# MFA Utility Design Specification

**Date:** 2026-07-17  
**Author:** Design generated through brainstorming session  
**Status:** Approved by user

---

## Overview

Implement TOTP (Time-based One-Time Password) based Multi-Factor Authentication utility in LuBan.Common using Otp.NET library, following existing utility class patterns.

## Requirements

### Core Requirements

1. **TOTP Support Only** - Implement TOTP (not HOTP) for compatibility with Google Authenticator, Microsoft Authenticator, etc.
2. **QR Code Generation** - Generate QR codes for easy authenticator app setup
3. **Secret Key Generation** - Generate secure random secrets for MFA enrollment
4. **Standard Defaults** - Use Google Authenticator standard: SHA-1, 6 digits, 30-second time step
5. **Time Tolerance** - Support ±1 step tolerance (90-second window) for clock drift

### Non-Goals

- HOTP (counter-based) support
- Custom hash algorithms (SHA-256, SHA-512)
- Custom time step sizes
- Custom code lengths beyond 6-8 digits
- State management or persistence

---

## Architecture

### File Location

`LuBan.Common/OptUtil.cs`

### Dependencies

**NuGet Packages:**
- `Otp.NET` (latest stable) - Core TOTP implementation
- Existing: `Encrypt.Library`, `ZXing.Net` (already in project)

### Class Structure

```csharp
namespace LuBan.Common;

/// <summary>
/// TOTP双因素认证工具类
/// </summary>
public static class OptUtil
{
    // Setup methods
    public static string GenerateSecret(int length = 20);
    public static string GenerateOtpAuthUri(string secret, string email, string issuer);
    public static byte[] GenerateQrCode(string secret, string email, string issuer, int width = 300, int height = 300);
    
    // Core TOTP methods
    public static string GetTotp(string secret, int digits = 6);
    public static bool ValidateTotp(string code, string secret, int digits = 6);
}
```

### Design Principles

- **Static utility class** - Matches existing pattern (PasswordUtil, TokenUtil, RandomUtil)
- **No state** - All methods are pure functions
- **Thread-safe** - No shared state, safe for concurrent access
- **Standard TOTP parameters** - SHA-1, 6 digits, 30-second time step
- **Fail-fast validation** - Clear exceptions for caller errors, bool returns for validation

### Integration Points

- `RandomUtil.GetWFACode(key, size)` → calls `OptUtil.GetTotp(key, size)`
- `RandomUtil.ValideWFACode(code, key, size)` → calls `OptUtil.ValidateTotp(code, key, size)`

Existing code in RandomUtil.cs (lines 120-143) already references OptUtil, but the class doesn't exist yet. This design fills that gap.

---

## Public API

### Setup Methods

#### GenerateSecret

```csharp
/// <summary>
/// Generate secure random secret key for MFA setup
/// </summary>
/// <param name="length">Secret length in bytes (default 20 for SHA-1)</param>
/// <returns>Base32-encoded secret (standard for authenticator apps)</returns>
public static string GenerateSecret(int length = 20)
```

**Behavior:**
- Uses Otp.NET's `KeyGeneration.GenerateRandomKey()`
- Returns Base32-encoded string (standard format for authenticator apps)
- Default 20 bytes is optimal for SHA-1 based TOTP
- Cryptographically secure random generation

**Examples:**
```csharp
var secret = OptUtil.GenerateSecret(); // "JBSWY3DPEHPK3PXP"
var longSecret = OptUtil.GenerateSecret(32); // Longer secret
```

#### GenerateOtpAuthUri

```csharp
/// <summary>
/// Generate otpauth:// URI for authenticator app setup
/// </summary>
/// <param name="secret">Base32-encoded secret</param>
/// <param name="email">User email/account identifier</param>
/// <param name="issuer">Application name</param>
/// <returns>otpauth://totp/ URI string</returns>
public static string GenerateOtpAuthUri(string secret, string email, string issuer)
```

**Behavior:**
- Generates standard `otpauth://totp/` URI
- Properly URL-encodes issuer and email
- Format: `otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}`

**Examples:**
```csharp
var uri = OptUtil.GenerateOtpAuthUri("JBSWY3DPEHPK3PXP", "user@example.com", "MyApp");
// otpauth://totp/MyApp:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=MyApp
```

#### GenerateQrCode

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
```

**Behavior:**
- Generates PNG format QR code using ZXing.Net
- Encodes `otpauth://` URI for scanning
- Uses existing ZXing.Net and SkiaSharp dependencies
- Returns PNG bytes ready for HTTP response or file storage

**Examples:**
```csharp
var qrCode = OptUtil.GenerateQrCode("JBSWY3DPEHPK3PXP", "user@example.com", "MyApp");
// Can be saved to file or returned as HTTP response
File.WriteAllBytes("qrcode.png", qrCode);
```

### Core TOTP Methods

#### GetTotp

```csharp
/// <summary>
/// Generate current TOTP code
/// </summary>
/// <param name="secret">Base32-encoded secret</param>
/// <param name="digits">Code length (default 6)</param>
/// <returns>6-digit TOTP code</returns>
public static string GetTotp(string secret, int digits = 6)
```

**Behavior:**
- Generates current TOTP code based on system time
- Uses SHA-1 and 30-second time step
- Returns numeric string of specified length
- Changes every 30 seconds

**Examples:**
```csharp
var secret = OptUtil.GenerateSecret();
var code = OptUtil.GetTotp(secret); // "123456"
var code8 = OptUtil.GetTotp(secret, 8); // "12345678" (8 digits)
```

#### ValidateTotp

```csharp
/// <summary>
/// Validate TOTP code with ±1 step tolerance
/// </summary>
/// <param name="code">User-provided code</param>
/// <param name="secret">Base32-encoded secret</param>
/// <param name="digits">Expected code length (default 6)</param>
/// <returns>True if valid, false otherwise</returns>
public static bool ValidateTotp(string code, string secret, int digits = 6)
```

**Behavior:**
- Validates code against current, previous, and next time steps (90-second window)
- Returns `false` for any invalid input (no exceptions)
- Matches existing `RandomUtil.ValideWFACode` pattern
- Case-insensitive code comparison

**Examples:**
```csharp
var secret = OptUtil.GenerateSecret();
var code = OptUtil.GetTotp(secret);
var valid = OptUtil.ValidateTotp(code, secret); // true
var invalid = OptUtil.ValidateTotp("000000", secret); // false
```

---

## Implementation Details

### GenerateSecret Implementation

```csharp
public static string GenerateSecret(int length = 20)
{
    var key = KeyGeneration.GenerateRandomKey(length);
    return Base32Encoding.ToString(key);
}
```

- Uses Otp.NET's `KeyGeneration.GenerateRandomKey()` for cryptographic randomness
- Converts to Base32 encoding (standard for TOTP secrets)

### GetTotp Implementation

```csharp
public static string GetTotp(string secret, int digits = 6)
{
    if (string.IsNullOrEmpty(secret))
        throw new ArgumentNullException(nameof(secret), "Secret cannot be null or empty");
    
    if (digits < 6 || digits > 8)
        throw new ArgumentOutOfRangeException(nameof(digits), "Digits must be between 6 and 8");
    
    try
    {
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key, step: 30, totpSize: digits);
        return totp.ComputeTotp();
    }
    catch (Exception ex)
    {
        throw new ArgumentException("Invalid secret format. Expected Base32-encoded string.", nameof(secret), ex);
    }
}
```

- Validates inputs before processing
- Creates Totp instance with standard parameters (30s step, SHA-1 default)
- Returns current code based on system time

### ValidateTotp Implementation

```csharp
public static bool ValidateTotp(string code, string secret, int digits = 6)
{
    if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(secret))
        return false;
    
    if (code.Length != digits)
        return false;
    
    try
    {
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key, step: 30, totpSize: digits);
        long timeStepMatched;
        return totp.VerifyTotp(code, out timeStepMatched, new VerificationWindow(1, 1));
    }
    catch
    {
        return false;
    }
}
```

- Returns `false` for all invalid inputs (no exceptions)
- Uses `VerificationWindow(1, 1)` for ±1 step tolerance
- Catches all exceptions and returns false (fail-safe)

### GenerateOtpAuthUri Implementation

```csharp
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

- Validates all parameters
- Properly URL-encodes components
- Follows standard otpauth URI format

### GenerateQrCode Implementation

```csharp
public static byte[] GenerateQrCode(string secret, string email, string issuer, int width = 300, int height = 300)
{
    if (string.IsNullOrEmpty(secret))
        throw new ArgumentNullException(nameof(secret));
    if (string.IsNullOrEmpty(email))
        throw new ArgumentNullException(nameof(email));
    if (string.IsNullOrEmpty(issuer))
        throw new ArgumentNullException(nameof(issuer));
    
    if (width < 100 || width > 2000)
        throw new ArgumentOutOfRangeException(nameof(width), "Width must be between 100 and 2000");
    if (height < 100 || height > 2000)
        throw new ArgumentOutOfRangeException(nameof(height), "Height must be between 100 and 2000");
    
    var uri = GenerateOtpAuthUri(secret, email, issuer);
    
    var writer = new BarcodeWriter<SKBitmap>();
    writer.Format = BarcodeFormat.QR_CODE;
    writer.Options = new EncodingOptions
    {
        Width = width,
        Height = height,
        Margin = 1
    };
    
    using var bitmap = writer.Write(uri);
    using var stream = new MemoryStream();
    bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
    return stream.ToArray();
}
```

- Validates parameters including size bounds
- Uses existing ZXing.Net dependency for QR generation
- Uses existing SkiaSharp dependency for image encoding
- Returns PNG format bytes

---

## Error Handling Strategy

### Exception vs Boolean Return

**Methods that throw exceptions:**
- `GetTotp()` - Throws `ArgumentException` for invalid inputs
- `GenerateOtpAuthUri()` - Throws `ArgumentNullException` for null parameters
- `GenerateQrCode()` - Throws `ArgumentException`/`ArgumentOutOfRangeException` for invalid inputs

**Methods that return bool:**
- `ValidateTotp()` - Returns `false` for any invalid input (matches existing pattern)

### Error Messages

- Clear, specific messages for each error case
- Include parameter name in exceptions
- Wrap lower-level exceptions with context

### Graceful Degradation

- `ValidateTotp()` never throws exceptions - always returns true/false
- Invalid Base32 secrets result in false, not exceptions
- Code length mismatches result in false, not exceptions

---

## Testing Strategy

### Test File

`LuBan.XTestProject/OptUtilTests.cs`

### Test Categories

1. **Secret Generation Tests**
   - Valid Base32 output
   - Custom length support
   - Uniqueness across calls

2. **TOTP Generation Tests**
   - Valid 6-digit format
   - Consistency across calls at same time
   - Custom digit lengths (6-8)

3. **Validation Tests**
   - Valid code acceptance
   - Invalid code rejection
   - Null/empty input handling
   - Wrong length handling
   - Time window tolerance verification

4. **QR Code Tests**
   - Valid otpauth:// URI generation
   - Valid PNG output
   - Parameter validation

5. **Integration Tests**
   - Compatibility with existing `RandomUtil.GetWFACode()`
   - Compatibility with existing `RandomUtil.ValideWFACode()`

### Test Execution

- Run with `dotnet test` in CI/CD
- All tests must pass before merge
- Target: >90% code coverage

---

## Usage Examples

### Basic MFA Setup Flow

```csharp
// 1. User enables MFA in application
var secret = OptUtil.GenerateSecret();
var qrCode = OptUtil.GenerateQrCode(secret, user.Email, "MyApp");

// 2. Save secret to database (user record)
await userRepository.UpdateMfaSecretAsync(userId, secret);

// 3. Return QR code to frontend
return File(qrCode, "image/png");
```

### Validate MFA Code

```csharp
// User login with MFA
var user = await userRepository.GetByEmailAsync(email);
var storedSecret = user.MfaSecret;

if (OptUtil.ValidateTotp(userProvidedCode, storedSecret))
{
    // MFA validation successful
    return LoginSuccess();
}
else
{
    // MFA validation failed
    return LoginFailed("Invalid MFA code");
}
```

### Generate Current Code (Testing/Admin)

```csharp
// Generate current TOTP code for testing or admin purposes
var secret = user.MfaSecret;
var currentCode = OptUtil.GetTotp(secret);
Console.WriteLine($"Current MFA code: {currentCode}");
```

---

## Security Considerations

### Secret Storage

- Secrets must be stored securely (encrypted at rest)
- Never log or expose secrets in error messages
- Consider using existing `Encrypt.Library` for storage encryption

### Time-Based Attacks

- TOTP depends on system time - ensure servers are time-synchronized
- ±1 step tolerance provides 90-second window for clock drift
- Do not increase tolerance beyond ±1 (security vs usability tradeoff)

### Brute Force Protection

- Implement rate limiting on MFA validation endpoints
- Consider account lockout after multiple failed attempts
- Do not expose detailed error messages about why validation failed

### QR Code Distribution

- QR codes contain secrets - protect during generation and display
- Use HTTPS when serving QR codes
- Consider one-time display with regeneration option

---

## Dependencies

### NuGet Package

**Otp.NET**
- Version: Latest stable (as of implementation)
- Purpose: Core TOTP/HOTP implementation
- License: MIT
- URL: https://www.nuget.org/packages/Otp.NET

### Existing Dependencies (No changes needed)

- `Encrypt.Library` - Already referenced in LuBan.Common
- `ZXing.Net` - Already referenced for barcode generation
- `ZXing.Net.Bindings.SkiaSharp` - Already referenced for image rendering
- `SkiaSharp` - Already referenced for image processing

---

## Implementation Checklist

1. Add Otp.NET NuGet package to `LuBan.Common.csproj`
2. Create `LuBan.Common/OptUtil.cs` file
3. Implement all methods according to spec
4. Update `RandomUtil.cs` comments to reference OptUtil
5. Create unit tests in `LuBan.XTestProject/OptUtilTests.cs`
6. Verify all tests pass
7. Verify integration with existing RandomUtil methods
8. Code review and merge

---

## Success Criteria

- All unit tests pass
- Integration with RandomUtil works correctly
- QR codes successfully scan with Google Authenticator
- TOTP codes validate correctly with authenticator apps
- Code coverage >90%
- No breaking changes to existing code
- Follows existing LuBan.Common utility patterns

---

## Future Considerations

- Consider adding backup code generation for MFA recovery
- Consider adding rate limiting helpers for MFA validation
- Consider adding configuration-based issuer/defaults
- Consider adding HOTP support if needed in future (would require separate methods)

---

## References

- [RFC 6238: TOTP](https://tools.ietf.org/html/rfc6238)
- [Otp.NET GitHub](https://github.com/ks6000/Otp.NET)
- [Google Authenticator URI Format](https://github.com/google/google-authenticator/wiki/Key-Uri-Format)