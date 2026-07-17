/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*文件名： OptUtil
*描述：TOTP双因素认证工具类
*****************************************************************************/
using OtpNet;
using System.IO;

namespace LuBan.Common;

/// <summary>
/// TOTP双因素认证工具类
/// </summary>
public static class OptUtil
{
    private const int DefaultSecretLength = 20;
    private const int DefaultCodeDigits = 6;
    private const int DefaultTimeStep = 30;
    private const int MinSecretLength = 16;
    private const int MaxSecretLength = 64;
    private const int MinCodeDigits = 6;
    private const int MaxCodeDigits = 8;
    private const int MinQrSize = 100;
    private const int MaxQrSize = 2000;

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
}