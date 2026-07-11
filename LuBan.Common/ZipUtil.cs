/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common.Encryption
*文件名： GZipUtil
*版本号： V1.0.0.0
*唯一标识：3268f43e-2362-4c80-ba28-2bc316d81524
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 10:50:18
*描述：Zip工具类
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 10:50:18
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：Zip工具类
*
*****************************************************************************/

namespace LuBan.Common;

/// <summary>
/// Zip工具类
/// </summary>
public static class ZipUtil
{
    /// <summary>
    /// 压缩
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[]? Compress(byte[] data)
    {
        if (data == null) return null;

        byte[]? buffer = null;
        using (var stream = new MemoryStream())
        {
            using (GZipStream inflateStream = new GZipStream(stream, CompressionMode.Compress, true))
            {
                inflateStream.Write(data, 0, data.Length);
            }
            if (stream.CanRead)
                stream.Seek(0, SeekOrigin.Begin);
            int length = Convert.ToInt32(stream.Length);
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
        }
        return buffer;
    }

    /// <summary>
    /// 解压缩
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[]? Decompress(byte[] data)
    {
        if (data == null) return null;

        using (var srcMs = new MemoryStream(data))
        {
            using (var zipStream = new GZipStream(srcMs, CompressionMode.Decompress))
            {
                using (var ms = new MemoryStream())
                {
                    var bytes = new byte[40960];
                    int n;
                    while ((n = zipStream.Read(bytes, 0, bytes.Length)) > 0)
                        ms.Write(bytes, 0, n);
                    return ms.ToArray();
                }
            }
        }
    }

    /// <summary>
    /// 压缩
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string Compress(string data)
    {
        var bytes = Compress(Encoding.UTF8.GetBytes(data));
        if (bytes == null) return string.Empty;
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 解压缩
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string Decompress(string data)
    {
        var bytes = Decompress(Encoding.UTF8.GetBytes(data));
        if (bytes == null) return string.Empty;
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Deflate
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] Deflate(byte[] data)
    {
        if (data == null) return [];

        byte[] buffer = [];
        using (MemoryStream stream = new MemoryStream())
        {
            using (DeflateStream inflateStream = new DeflateStream(stream, CompressionMode.Compress, true))
            {
                inflateStream.Write(data, 0, data.Length);
            }
            if (stream.CanRead)
                stream.Seek(0, SeekOrigin.Begin);

            int length = Convert.ToInt32(stream.Length);
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
        }

        return buffer;
    }

    /// <summary>
    /// UnDeflate
    /// </summary>
    /// <param name="compressedData"></param>
    /// <returns></returns>
    public static byte[] UnDeflate(byte[] compressedData)
    {
        if (compressedData == null) return [];

        int deflen = compressedData.Length * 2;
        byte[] buffer = [];

        using (MemoryStream stream = new(compressedData))
        {
            using DeflateStream inflatestream = new(stream, CompressionMode.Decompress);
            using MemoryStream uncompressedstream = new();
            using BinaryWriter writer = new(uncompressedstream);
            int offset = 0;
            while (true)
            {
                byte[] tempbuffer = new byte[deflen];

                int bytesread = inflatestream.Read(tempbuffer, offset, deflen);

                writer.Write(tempbuffer, 0, bytesread);

                if (bytesread < deflen || bytesread == 0) break;
            }
            if (uncompressedstream.CanRead)
                uncompressedstream.Seek(0, SeekOrigin.Begin);
            buffer = uncompressedstream.ToArray();
        }
        return buffer;
    }

    /// <summary>
    /// ICSharpCode gzip Compress
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] GZipCompress(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var outMs = new MemoryStream();
        GZip.Compress(ms, outMs, true);
        return outMs.ToArray();
    }

    /// <summary>
    /// ICSharpCode gzip Decompress
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] GZipDecompress(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var outMs = new MemoryStream();
        GZip.Decompress(ms, outMs, true);
        return outMs.ToArray();
    }


    /// <summary>
    /// ICSharpCode gzip Compress
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GZipCompress(string data)
    {
        return Convert.ToBase64String(GZipCompress(Encoding.UTF8.GetBytes(data)));
    }

    /// <summary>
    /// ICSharpCode gzip Decompress
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GZipDecompress(string data)
    {
        return Encoding.UTF8.GetString(GZipDecompress(Convert.FromBase64String(data)));
    }

    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="zipFileName"></param>
    public static void Zip(string dir, string zipFileName)
    {
        var fastzip = new FastZip
        {
            CreateEmptyDirectories = true
        };
        fastzip.CreateZip(zipFileName, dir, true, string.Empty);
    }

    /// <summary>
    /// 解压文件夹
    /// </summary>
    /// <param name="zipFileName"></param>
    /// <param name="dir"></param>
    public static void UnZip(string zipFileName, string dir)
    {
        var fastzip = new FastZip
        {
            CreateEmptyDirectories = true
        };
        fastzip.ExtractZip(zipFileName, dir, string.Empty);
    }

    /// <summary>
    /// 解压文件
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Dictionary<string, byte[]> UncompressFiles(byte[] buffer)
    {
        var dicFiles = new Dictionary<string, byte[]>();
        var data = new MemoryStream(buffer);
        var archive = new ZipArchive(data);
        foreach (var entry in archive.Entries)
        {
            var s = entry.Open();
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                bytes = ms.ToArray();
            }
            dicFiles.Add(entry.Name, bytes);
        }
        return dicFiles;
    }


    /// <summary>
    /// 根据文件列表创建 ZIP 压缩包
    /// </summary>
    /// <param name="filePaths">要压缩的文件绝对路径列表</param>
    /// <param name="zipFilePath">压缩包保存路径（如 "D:/files.zip"）</param>
    /// <param name="compressionLevel">压缩级别（0-9，9 为最高压缩率）</param>
    /// <exception cref="ArgumentException">文件不存在时抛出</exception>
    public static void CreateZipFromFiles(List<string> filePaths, string zipFilePath, int compressionLevel = 0)
    {
        // 校验压缩级别
        if (compressionLevel < 0 || compressionLevel > 9)
            throw new ArgumentException("压缩级别必须在 0-9 之间");

        // 校验文件是否存在
        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath))
                throw new ArgumentException($"文件不存在：{filePath}");
        }

        // 创建压缩包目录（若不存在）
        var zipDir = Path.GetDirectoryName(zipFilePath);
        if (zipDir.IsNotNullOrEmpty() && !Directory.Exists(zipDir))
            Directory.CreateDirectory(zipDir);

        // 开始压缩
        using (var fs = File.Create(zipFilePath))
        using (var zipStream = new ZipOutputStream(fs))
        {
            zipStream.SetLevel(compressionLevel); // 设置压缩级别

            foreach (var filePath in filePaths)
            {
                // 获取文件名（不含路径，避免压缩包内包含完整目录结构）
                var fileName = Path.GetFileName(filePath);

                // 创建 ZIP 条目
                var zipEntry = new ZipEntry(fileName)
                {
                    DateTime = File.GetLastWriteTime(filePath) // 保留文件修改时间
                };
                zipStream.PutNextEntry(zipEntry);

                // 写入文件内容到压缩包
                using (var fileStream = File.OpenRead(filePath))
                {
                    var buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        zipStream.Write(buffer, 0, bytesRead);
                    }
                }

                zipStream.CloseEntry(); // 关闭当前条目
            }
        }
    }
}
