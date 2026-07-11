/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： StreamUtil
*版本号： V1.0.0.0
*唯一标识：c7f79854-e2e4-4e1d-b624-32c9ecdf2c2a
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/9/24 10:24:27
*描述：流工具类
*
*=====================================================================
*修改标记
*修改时间：2021/9/24 10:24:27
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：流工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 流工具类
/// </summary>
public static class StreamUtil
{
    /// <summary>
    /// 将流转换成byte数组
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[]? ToBytes(this Stream stream)
    {
        if (stream == null)
        {
            return null;
        }
        if (stream is FileStream fs)
        {
            var bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        else if (stream is MemoryStream ms)
        {
            ms.Position = 0;
            return ms.ToArray();
        }
        else
        {
            using (var ms2 = new MemoryStream())
            {
                stream.CopyTo(ms2);
                ms2.Position = 0;
                return ms2.ToArray();
            }
        }
    }

    /// <summary>
    /// 将流转换成byte数组
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<byte[]> ToBytesAsync(this Stream stream)
    {
        if (stream == null)
        {
            return [];
        }
        if (stream is FileStream fs)
        {
            var bytes = new byte[fs.Length];
            await fs.ReadAsync(bytes, 0, bytes.Length);
            return bytes;
        }
        else if (stream is MemoryStream ms)
        {
            ms.Position = 0;
            return ms.ToArray();
        }
        else
        {
            using var ms2 = new MemoryStream();
            await stream.CopyToAsync(ms2);
            ms2.Position = 0;
            return ms2.ToArray();
        }
    }

    /// <summary>
    /// 读取流中的文本
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string ReadToEnd(this Stream? stream, Encoding? encoding = null, bool leaveOpen = false, int bufferSize = 64 * 1024)
    {
        if (stream == null) return string.Empty;
        if (stream.CanSeek)
            stream.Position = 0;
        if (encoding == null) encoding = Encoding.UTF8;
        using var reader = new StreamReader(stream, encoding, true, bufferSize, leaveOpen);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 将字符串转换成UTF8字节数组
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] ToBytes([NotNull] this string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// 将字符串转换为UTF8流
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Stream? ToStream(this string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        return new MemoryStream(Encoding.UTF8.GetBytes(str)) { Position = 0 };
    }

    /// <summary>
    /// 将流中内容保存到文件
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="filePath"></param>
    public static void Save(this Stream stream, string filePath)
    {
        if (stream == null || !stream.CanRead) return;

        using (var fs = FileUtil.GetStream(filePath))
        {
            stream.CopyTo(fs);
        }
    }
    /// <summary>
    /// 字节数组转换成流
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static Stream ToStream(this byte[] bytes)
    {
        return new MemoryStream(bytes);
    }

    /// <summary>
    /// 读取流中的文本
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <param name="leaveOpen"></param>
    /// <returns></returns>
    public static async Task<string> ReadToEndAsync(this Stream? stream, Encoding? encoding, int bufferSize = 64 * 1024, bool leaveOpen = false)
    {
        string requestBody = string.Empty;
        if (stream == null) return requestBody;
        if (encoding == null) encoding = Encoding.UTF8;
        using (var reader = new StreamReader(
            stream: stream,
            encoding: encoding,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: bufferSize,
            leaveOpen: true
        ))
        {
            requestBody = await reader.ReadToEndAsync();
            if (leaveOpen) stream.Position = 0;
        }
        return requestBody;
    }
}
