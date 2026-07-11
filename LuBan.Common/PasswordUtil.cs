/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*文件名： PasswordUtil
*描述：密码加密工具类
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 密码加密工具类
/// </summary>
public static class PasswordUtil
{
    /// <summary>
    /// 默认盐值（CommonConst.SecretSalt 的 base64 解码值）
    /// </summary>
    public static string DefaultSalt => Encoding.UTF8.GetString(Convert.FromBase64String(CommonConst.SecretSalt));

    /// <summary>
    /// 加密密码：MD5(盐 + 密码)，结果大写
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <param name="salt">盐值，为空时使用默认盐</param>
    /// <returns>加密后的密码</returns>
    public static string Encrypt(string? password, string? salt = null)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        salt = string.IsNullOrEmpty(salt) ? DefaultSalt : salt;
        return (salt + password).GetMD5Str().ToUpperInvariant();
    }

    /// <summary>
    /// 验证密码（兼容旧版小写无盐 MD5）
    /// </summary>
    /// <param name="inputPassword">用户输入的原始密码</param>
    /// <param name="storedPassword">数据库存储的密码</param>
    /// <param name="salt">盐值，为空时使用默认盐</param>
    /// <returns>是否匹配</returns>
    public static bool Verify(string inputPassword, string? storedPassword, string? salt = null)
    {
        if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword)) return false;

        var encrypted = Encrypt(inputPassword, salt);
        if (string.Equals(encrypted, storedPassword, StringComparison.OrdinalIgnoreCase))
            return true;

        // 兼容旧版：小写 MD5(密码)
        var legacy = inputPassword.GetMD5Str().ToLowerInvariant();
        if (string.Equals(legacy, storedPassword, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// 判断是否为旧版密码（小写 MD5，无盐）
    /// </summary>
    public static bool IsLegacyPassword(string? storedPassword)
    {
        return !string.IsNullOrEmpty(storedPassword) && storedPassword == storedPassword.ToLowerInvariant();
    }
}
