/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common
*文件名： Base64Util
*版本号： V1.0.0.0
*唯一标识：07d530e7-1698-4336-bd22-b3bafa724d98
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 14:46:25
*描述：base64工具类
*
*=================================================
*修改标记
*修改时间：2023/12/1 14:46:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：base64工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// base64工具类
/// </summary>
public static class Base64Util
{

    /// <summary>
    /// 字符串转换成base64 string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string? ToBase64Str(this string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        return Encoding.UTF8.GetBytes(str).ToBase64Str();
    }

    /// <summary>
    /// 流转换成base64 string
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string ToBase64Str(this Stream stream)
    {
        return stream.ToBytes()?.ToBase64Str() ?? "";
    }

    /// <summary>
    /// 字节数组转换成base64 string
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string? ToBase64Str(this byte[] bytes)
    {
        if (bytes == null) return null;
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 将base64Str转换成字节数组
    /// </summary>
    /// <param name="base64Str"></param>
    /// <returns></returns>
    public static byte[] ToBytes(string base64Str)
    {
        if (string.IsNullOrEmpty(base64Str)) return [];
        return Convert.FromBase64String(base64Str);
    }
    /// <summary>
    /// 将base64Str转换字符串
    /// </summary>
    /// <param name="base64Str"></param>
    /// <returns></returns>
    public static string ToStr(this string base64Str)
    {
        if (string.IsNullOrEmpty(base64Str)) return string.Empty;
        return ToBytes(base64Str).ToStr();
    }

    /// <summary>
    /// 将base64Str转换流
    /// </summary>
    /// <param name="base64Str"></param>
    /// <returns></returns>
    public static Stream? ToStream(string base64Str)
    {
        if (string.IsNullOrEmpty(base64Str)) return null;
        return new MemoryStream(ToBytes(base64Str) ?? throw new Exception("不正确的base64字符串")) { Position = 0 };
    }
    /// <summary>
    /// 保存成文件
    /// </summary>
    /// <param name="base64Str"></param>
    /// <param name="filePath"></param>
    public static void Save(this string base64Str, string filePath)
    {
        (ToStream(base64Str) ?? throw new Exception("不正确的base64字符串")).Save(filePath);
    }
}
