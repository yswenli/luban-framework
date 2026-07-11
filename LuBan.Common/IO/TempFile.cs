/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.IO
*文件名： TempFile
*版本号： V1.0.0.0
*唯一标识：d2a8bf77-ee90-478c-85ef-14c8b4c6e65d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/10/12 13:30:59
*描述：
*
*=================================================
*修改标记
*修改时间：2024/10/12 13:30:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.IO;

/// <summary>
/// 临时文件
/// </summary>
public class TempFile : IDisposable
{
    string _filePath;

    /// <summary>
    /// 文件地址
    /// </summary>
    public string FilePath
    {
        get
        {
            return _filePath;
        }
    }

    /// <summary>
    /// 临时文件
    /// </summary>
    /// <param name="filePath"></param>
    public TempFile(string filePath)
    {
        _filePath = filePath;
        if (_filePath.IsNullOrEmpty())
        {
            _filePath = Path.GetTempFileName();
        }
        PathUtil.Create(_filePath);
    }

    /// <summary>
    /// 临时文件
    /// </summary>
    public TempFile() : this(Path.GetTempFileName())
    {

    }

    /// <summary>
    /// 保存临时文件
    /// </summary>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public async Task SaveAsync(string content, Encoding encoding)
    {
        await File.WriteAllTextAsync(_filePath, content, encoding);
    }

    /// <summary>
    /// 保存临时文件
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task SaveAsync(string content)
    {
        await SaveAsync(content, Encoding.UTF8);
    }

    /// <summary>
    /// 保存临时文件
    /// </summary>
    /// <param name="content"></param>
    public async Task SaveAsync(byte[] content)
    {
        await File.WriteAllBytesAsync(_filePath, content);
    }

    /// <summary>
    /// 保存临时文件
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public async Task SaveAsync(Stream stream)
    {
        if (stream == null) return;
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
        using var fs = File.Open(_filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        await stream.CopyToAsync(fs);
        stream.Dispose();
    }

    /// <summary>
    /// 读取临时文件流
    /// </summary>
    /// <returns></returns>
    public FileStream Open()
    {
        return File.Open(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
    }

    /// <summary>
    /// 读取临时文件内容
    /// </summary>
    /// <returns></returns>
    public Task<string> ReadAllTextAsync()
    {
        return File.ReadAllTextAsync(_filePath, Encoding.UTF8);
    }

    /// <summary>
    /// 读取临时文件内容
    /// </summary>
    /// <returns></returns>
    public Task<byte[]> ReadAllBytesAsync()
    {
        return File.ReadAllBytesAsync(_filePath);
    }

    /// <summary>
    /// 读取临时文件内容
    /// </summary>
    /// <returns></returns>
    public Stream ReadAsStream()
    {
        return Open();
    }

    /// <summary>
    /// 删除临时文件
    /// </summary>
    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                File.Delete(_filePath);
            }
            catch { }
        }
    }
}

/// <summary>
/// 临时文件扩展
/// </summary>
public static class TempFileEx
{
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="netStream"></param>
    /// <returns></returns>
    public static async Task<TempFile?> DonwloadAsync(this Stream netStream)
    {
        if (netStream == null) return null;
        if (netStream.CanSeek) netStream.Seek(0, SeekOrigin.Begin);
        var tf = new TempFile();
        await tf.SaveAsync(netStream);
        return tf;
    }

    /// <summary>
    /// 将对象序列化到临时文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static async Task<TempFile?> ToTempFileAsync<T>(this T t) where T : class, new()
    {
        if (t == null) return null;
        var json = SerializeUtil.Serialize(t);
        var tf = new TempFile();
        await tf.SaveAsync(json);
        return tf;
    }

    /// <summary>
    /// 从临时文件反序列化对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tf"></param>
    /// <returns></returns>
    public static async Task<T?> ToObjectAsync<T>(this TempFile tf) where T : class, new()
    {
        if (tf == null) return null;
        var json = await tf.ReadAllTextAsync();
        if (json.IsNotNullOrEmpty()) return SerializeUtil.Deserialize<T>(json);
        return null;
    }
}
