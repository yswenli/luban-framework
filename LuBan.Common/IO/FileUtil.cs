/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： FileUtil
*版本号： V1.0.0.0
*唯一标识：46246be3-87d1-415e-815e-749ffc9cd11d
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 13:13:47
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 13:13:47
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.IO;

/// <summary>
/// 文件操作类
/// </summary>
public static class FileUtil
{
    /// <summary>
    /// Exists
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// CreateFile
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool CreateFile(string filePath)
    {
        GetDirecotry(filePath);

        FileStream fs = File.Create(filePath);

        fs.Close();

        fs.Dispose();

        return true;
    }

    /// <summary>
    /// GetDirecotry,不存在则创建
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetDirecotry(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (dir.IsNotNullOrEmpty() && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            return dir;
        }
        return string.Empty;
    }

    /// <summary>
    /// CreateIfNotExists
    /// </summary>
    /// <param name="filePath"></param>
    public static void CreateIfNotExists(string filePath)
    {
        GetDirecotry(filePath);
        if (!Exists(filePath))
        {
            using FileStream fs = File.Create(filePath);
        }
    }

    /// <summary>
    /// 读取流
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static FileStream GetStream(string filePath)
    {
        filePath = filePath.ToCorrectPath();
        return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
    }

    /// <summary>
    /// 读取内存流
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="deleteFile"></param>
    /// <returns></returns>
    public static Stream GetMemoryStream(string filePath, bool deleteFile = false)
    {
        MemoryStream memoryStream = new MemoryStream();
        using (var fileStream = GetStream(filePath))
        {
            fileStream.CopyTo(memoryStream);
        }
        memoryStream.Position = 0;
        if (deleteFile)
        {
            File.Delete(filePath);
        }
        return memoryStream;
    }


    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="data"></param>
    public static void Write(string filePath, byte[] data)
    {
        GetDirecotry(filePath);
        using var fs = GetStream(filePath);
        fs.Write(data, 0, data.Length);
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="stream"></param>
    public static void Write(string filePath, Stream stream)
    {
        GetDirecotry(filePath);
        stream.Position = 0;
        using var fs = GetStream(filePath);
        stream.CopyTo(fs);
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="base64Str"></param>
    public static void WriteFromBase64Str(string filePath, string base64Str)
    {
        var data = Convert.FromBase64String(base64Str);
        GetDirecotry(filePath);
        using var fs = GetStream(filePath);
        fs.Write(data, 0, data.Length);
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task WriteAsync(string filePath, byte[] data)
    {
        GetDirecotry(filePath);
        using var fs = GetStream(filePath);
        await fs.WriteAsync(data);
    }

    /// <summary>
    /// 写文本
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="txt"></param>
    public static void WriteString(string filePath, string txt)
    {
        var data = Encoding.UTF8.GetBytes(txt);
        Write(filePath, data);
    }

    /// <summary>
    /// 写文本
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="txt"></param>
    /// <returns></returns>
    public static async Task WriteStringAsync(string filePath, string txt)
    {
        var data = Encoding.UTF8.GetBytes(txt);
        await WriteAsync(filePath, data);
    }

    /// <summary>
    /// 追加
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="data"></param>
    public static void Append(string filePath, byte[] data)
    {
        GetDirecotry(filePath);
        using FileStream fs = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        fs.Write(data, 0, data.Length);
    }

    /// <summary>
    /// 追加
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="data"></param>
    public static async Task AppendAsync(string filePath, byte[] data)
    {
        GetDirecotry(filePath);
        using FileStream fs = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        await fs.WriteAsync(data, 0, data.Length);
    }

    /// <summary>
    /// 追加文本
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="txt"></param>
    public static void AppendString(string filePath, string txt)
    {
        var data = Encoding.UTF8.GetBytes(txt);
        Append(filePath, data);
    }

    /// <summary>
    /// 追加文本
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="txt"></param>
    /// <returns></returns>
    public static async Task AppendStringAsync(string filePath, string txt)
    {
        var data = Encoding.UTF8.GetBytes(txt);
        await AppendAsync(filePath, data);
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static byte[] Read(string filePath)
    {
        byte[] data = [];
        if (!File.Exists(filePath))
        {
            return [];
        }
        using (var fs = GetStream(filePath))
        {
            var buffer = new byte[fs.Length];
            fs.Position = 0;
            var offset = 0;

            while ((offset = fs.Read(buffer, offset, buffer.Length)) > 0)
            {
                if (offset == fs.Length) break;

                if (offset == 0) throw new Exception($"读取{filePath}出现异常！");

            }
            data = buffer;
        }
        return data;
    }
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<byte[]> ReadAsync(string filePath)
    {
        byte[] data = [];
        if (!File.Exists(filePath))
        {
            return data;
        }
        using (var fs = GetStream(filePath))
        {
            var buffer = new byte[fs.Length];
            fs.Position = 0;
            var offset = 0;

            while ((offset = await fs.ReadAsync(buffer, offset, buffer.Length)) > 0)
            {
                if (offset == fs.Length) break;

                if (offset == 0) throw new Exception($"读取{filePath}出现异常！");

            }
            data = buffer;
        }
        return data;
    }
    /// <summary>
    /// 读取文本内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string ReadString(string filePath, Encoding? encoding = null)
    {
        if (File.Exists(filePath))
            return File.ReadAllText(filePath, encoding ?? Encoding.UTF8);
        return string.Empty;
    }

    /// <summary>
    /// 读取文本删除文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string ReadStringThenRemvoe(string filePath, Encoding? encoding = null)
    {
        try
        {
            return File.ReadAllText(filePath, encoding ?? Encoding.UTF8);
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            try
            {
                File.Delete(filePath);
            }
            catch { }
        }
    }

    /// <summary>
    /// 读取文本内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<string> ReadStringAsync(string filePath)
    {
        if (!File.Exists(filePath)) return string.Empty;

        var data = await ReadAsync(filePath);

        if (data != null && data.Any())
        {
            return Encoding.UTF8.GetString(data);
        }
        return string.Empty;
    }
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="read"></param>
    /// <param name="bufferSize"></param>
    public static void Read(string filePath, Action<byte[]> read, int bufferSize = 10240)
    {
        if (!File.Exists(filePath)) return;

        using var fs = GetStream(filePath);
        fs.Position = 0;

        var data = new byte[bufferSize];

        while (true)
        {
            var len = fs.Read(data, 0, data.Length);

            if (len == 0) break;

            var buffer = data.AsSpan().Slice(0, len).ToArray();

            read?.Invoke(buffer);
        }
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="read"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static async Task ReadAsync(string filePath, Action<byte[]> read, int bufferSize = 10240)
    {
        if (!File.Exists(filePath)) return;

        using var fs = GetStream(filePath);
        fs.Position = 0;

        var data = new byte[bufferSize];

        while (true)
        {
            var len = await fs.ReadAsync(data, 0, data.Length);

            if (len == 0) break;

            var buffer = data.AsSpan().Slice(0, len).ToArray();

            read?.Invoke(buffer);
        }
    }

    /// <summary>
    /// 读取内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static T? Read<T>(string filePath)
    {
        if (!File.Exists(filePath)) return default;
        var json = ReadString(filePath);
        if (!string.IsNullOrEmpty(json))
        {
            return SerializeUtil.Deserialize<T>(json);
        }
        return default;
    }

    /// <summary>
    /// 写入内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool Write<T>(string filePath, T t)
    {
        if (t != null)
        {
            var json = t.ToJson();

            WriteString(filePath, json);

            return true;
        }
        return false;
    }


    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static FileInfo? GetFileInfo(string filePath)
    {
        if (File.Exists(filePath))
        {
            return new FileInfo(filePath);
        }
        return null;
    }

    /// <summary>
    /// 移除文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool Remove(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            if (!File.Exists(filePath)) return false;

            File.Delete(filePath);

            return true;
        }
        catch
        {

        }
        return false;
    }

    /// <summary>
    /// 删除多个文件
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    public static IEnumerable<bool> Remove(IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            yield return Remove(filePath);
        }
    }

    /// <summary>
    /// 获取目录下全部文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filter"></param>
    /// <param name="all"></param>
    /// <returns></returns>
    public static List<string> GetFiles(string path, string filter = "*.*", bool all = false)
    {
        if (string.IsNullOrEmpty(path)) return [];

        var dirInfo = new DirectoryInfo(path);

        if (!dirInfo.Exists) return [];

        List<string> result = [];

        var files = dirInfo.GetFiles(filter).Select(q => q.FullName);

        if (files != null && files.Any())
        {
            result.AddRange(files);
        }
        if (all)
        {
            var dirs = dirInfo.GetDirectories();

            if (dirs != null && dirs.Length > 0)
            {
                foreach (var dir in dirs)
                {
                    var sfiles = GetFiles(dir.FullName, filter, all);
                    if (sfiles.Count > 0)
                    {
                        result.AddRange(sfiles);
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 获取文件全路径
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetCurrentFile(string fileName, string path = "")
    {
        if (string.IsNullOrEmpty(path)) path = PathUtil.CurrentPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return Path.Combine(path, fileName);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fullName"></param>
    public static void Delete(string fullName)
    {
        _ = Remove(fullName);
    }

    /// <summary>
    /// 打开文件流
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="fileAccess"></param>
    /// <returns></returns>
    public static FileStream Open(string localFilePath, FileAccess fileAccess = FileAccess.Read, FileMode fileMode = FileMode.Open)
    {
        if (fileAccess == FileAccess.Read)
            return File.Open(localFilePath, fileMode, fileAccess, FileShare.ReadWrite);
        else if (fileAccess == FileAccess.Write)
            return File.Open(localFilePath, fileMode, fileAccess, FileShare.ReadWrite);
        else
            return File.Open(localFilePath, fileMode, fileAccess, FileShare.ReadWrite);
    }

    /// <summary>
    /// 从其他流中保存文件，并返回文件流；
    /// 比如网络流保存到本地，再返回文件流
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static FileStream SaveStream(Stream? stream, string localFilePath)
    {
        localFilePath = localFilePath.ToCorrectPath();
        if (stream == null) throw new IOException("传入的stream不能为空");
        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);
        using (var fs = Open(localFilePath, FileAccess.Write, FileMode.OpenOrCreate))
        {
            stream.CopyTo(fs);
        }
        return Open(localFilePath, FileAccess.Read);
    }

    /// <summary>
    /// 从其他流中保存文件，并返回文件流；
    /// 比如网络流保存到本地，再返回文件流
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<FileStream?> SaveStreamAsync(Stream? stream, string localFilePath)
    {
        try
        {
            localFilePath = localFilePath.ToCorrectPath();
            if (stream == null) return null;
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
            var path = Path.GetDirectoryName(localFilePath);
            if (path == null) return null;
            if (!Path.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (var fs = Open(localFilePath, FileAccess.Write, FileMode.OpenOrCreate))
            {
                await stream.CopyToAsync(fs);
            }
            return Open(localFilePath, FileAccess.Read);
        }
        catch
        {
            return null;
        }
    }

    #region 文件类型判断

    /// <summary>
    /// 文件类型签名
    /// </summary>
    public static Dictionary<string, List<byte[]>> FileTypeSignature { get; private set; }


    static FileUtil()
    {
        FileTypeSignature = new Dictionary<string, List<byte[]>>{
            //word文档
            { ".DOC", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }}},
            { ".DOCX", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 }}},
            //PDF文档
            { ".PDF", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 }}},
            //ZIP文档
            { ".ZIP", new List<byte[]>{new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x55 },new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },new byte[] { 0x50, 0x4B, 0x05, 0x06 },new byte[] { 0x50, 0x4B, 0x07, 0x08 },new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 }}},
            //图片格式
            { ".PNG", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }}},
            { ".JPG", new List<byte[]> {new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }}},
            { ".JPEG", new List<byte[]>{ new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }}},
            { ".GIF", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 }}},
            { ".BMP", new List<byte[]> { new byte[] { 0x42, 0x4D }}},
            { ".JFIF", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 }}},
            //视频格式
            //高清，高占用空间
            { ".MP4", new List<byte[]> { new byte[] { 0x00,0x00,0x00,0x18,0x66,0x74,0x79,0x70,0x6D,0x70,0x34,0x32 },new byte[] { 0x00,0x00,0x00,0x1C,0x66,0x74,0x79,0x70,0x6D,0x70,0x34,0x32 }, new byte[] { 0x00,0x00,0x00,0x20,0x66,0x74,0x79,0x70,0x69,0x73,0x6F, 0x6D }}},
            //高清，中度占用空间
            { ".MKV", new List<byte[]> { new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0xA3, 0x42, 0x86, 0x81, 0x01, 0x42, 0xF7 }}},
            //高清，中度占用空间
            { ".MOV", new List<byte[]> { new byte[] { 0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70, 0x71, 0x74 }}},
            //高清，低占用空间
            { ".M4V", new List<byte[]> {new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x4D, 0x34, 0x56 }}},
            //高清，低占用空间
            { ".WEBM", new List<byte[]> {new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0x9F, 0x42, 0x86, 0x81, 0x01, 0x42, 0xF7, 0x81, 0x01, 0x42, 0xF2 }}},
            //低质量
            { ".WMV", new List<byte[]> {new byte[] { 0x30,0x26,0xB2,0x75,0x8E,0x66,0xCF,0x11}}},
            //低质量
            { ".AVI", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46, 0x84, 0x4A, 0x1E, 0x00, 0x41, 0x56, 0x49 }}},
            //低质量
            { ".FLV", new List<byte[]> { new byte[] { 0x46, 0x4C, 0x56 }}},
            //电子表格
            { ".XLS", new List<byte[]>{ new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },new byte[] { 0x09, 0x08, 0x10, 0x00, 0x00, 0x06, 0x05, 0x00 },new byte[] { 0xFD, 0xFF, 0xFF, 0xFF }}},
            { ".XLSX", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 }}},
        };
    }

    #endregion
}

/// <summary>
/// 待删除文件
/// </summary>
public class DeletionFile : IDisposable
{
    readonly List<string> _filePaths;

    /// <summary>
    /// 待删除文件
    /// </summary>
    public DeletionFile()
    {
        _filePaths = [];
    }

    /// <summary>
    /// 待删除文件
    /// </summary>
    /// <param name="filePaths"></param>
    public DeletionFile(params string[] filePaths) : this()
    {
        if (filePaths == null || filePaths.Length < 1) return;
        foreach (var item in filePaths)
        {
            _filePaths.Add(item);
        }
    }

    /// <summary>
    /// 添加待删除文件
    /// </summary>
    /// <param name="filePaths"></param>
    public void Add(params string[] filePaths)
    {
        if (filePaths == null || filePaths.Length < 1) return;
        foreach (var item in filePaths)
        {
            _filePaths.Add(item);
        }
    }

    /// <summary>
    /// 删除待删除的文件列表
    /// </summary>
    public void Remove()
    {
        if (_filePaths == null || _filePaths.Count < 1) return;
        foreach (var item in _filePaths)
        {
            FileUtil.Delete(item);
        }
        _filePaths.Clear();
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public void Dispose()
    {
        Remove();
    }
}
