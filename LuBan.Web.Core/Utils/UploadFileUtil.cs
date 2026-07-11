/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： UploadFileUtil
*版本号： V1.0.0.0
*唯一标识：0a20a67c-d7b0-43b3-95a5-7cd06ad1c163
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/13 9:13:54
*描述：aspnetcore中文件上传处理工具类
*
*=====================================================================
*修改标记
*修改时间：2022/7/13 9:13:54
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：aspnetcore中文件上传处理工具类
*
*****************************************************************************/

namespace System;

/// <summary>
/// aspnetcore中文件上传处理工具类
/// </summary>
public static class UploadFileUtil
{
    static UploadOptions _uploadOptions = ConfigUtil.Read<UploadOptions>() ?? throw new Exception("上传配置文件不存在");

    /// <summary>
    /// 返回物理地址
    /// </summary>
    /// <param name="env"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static string MapPath(this IWebHostEnvironment env, string resource)
    {
        var path = env.WebRootPath;
        if (string.IsNullOrEmpty(path))
        {
            path = env.ContentRootPath;
        }
        return Path.Combine(path, resource);
    }

    /// <summary>
    /// 根据配置文件，验证文件是否合法
    /// </summary>
    /// <param name="file"></param>
    public static void ValidateFile(this IFormFile file)
    {
        var sizeKb = (long)(file.Length / 1024.0); // 大小KB

        if (sizeKb > _uploadOptions.MaxSize)
            throw FriendlyError.Ex(EnumErrorCode.D8002);

        var suffix = Path.GetExtension(file.FileName).ToLower(); // 后缀

        if (string.IsNullOrWhiteSpace(suffix))
            throw FriendlyError.Ex(EnumErrorCode.D8003);

        if (_uploadOptions.ExtensionNames == null
           || _uploadOptions.ExtensionNames.Count < 1
           || !_uploadOptions.ExtensionNames.Contains(suffix))
            throw FriendlyError.Ex(EnumErrorCode.D8001);
    }


    /// <summary>
    /// 保存客户端上传的文件
    /// </summary>
    /// <param name="file"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    public static string SaveFile(this IFormFile file, string folder = "")
    {
        try
        {
            file.ValidateFile();

            string uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{file.FileName}";

            var upload = PathUtil.GetSpecialPath(EnumFolderType.Upload);

            string filePath = PathUtil.GetFullName(upload, EnvironmentUtil.GetEnvironment(), uniqueFileName);

            if (folder.IsNotNullOrEmpty())
            {
                filePath = PathUtil.GetFullName(upload, folder, uniqueFileName);
            }
            using var fs = FileUtil.GetStream(filePath);
            file.OpenReadStream().CopyTo(fs);
            return filePath;
        }
        catch (Exception ex)
        {
            Logger.Error("UploadFileUtil.SaveFiles", ex);
        }
        return string.Empty;
    }

    /// <summary>
    /// 保存客户端上传的文件
    /// </summary>
    /// <param name="file"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    public static async Task<string> SaveFileAsync(this IFormFile file, string folder = "")
    {
        try
        {
            file.ValidateFile();

            string uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{file.FileName}";

            var upload = PathUtil.GetSpecialPath(EnumFolderType.Upload);

            string filePath = PathUtil.GetFullName(upload, uniqueFileName);

            if (folder.IsNotNullOrEmpty())
            {
                filePath = PathUtil.GetFullName(upload, folder, uniqueFileName);
            }
            using (var fs = FileUtil.GetStream(filePath))
            {
                await file.OpenReadStream().CopyToAsync(fs);
                return filePath;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("UploadFileUtil.SaveFilesAsync", ex);
        }
        return string.Empty;
    }

    /// <summary>
    /// 接收文件并返回DataTable
    /// </summary>
    /// <param name="file"></param>
    /// <param name="sheetName"></param>
    /// <param name="startRow"></param>
    /// <param name="hasHeader"></param>
    /// <returns></returns>
    public static DataTable SaveAsDataTable(this IFormFile file, string sheetName = "sheet1", int startRow = 0, bool hasHeader = true)
    {
        file.ValidateFile();

        if (file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".xlsx"))
        {
            using var ms = new MemoryStream();
            file.OpenReadStream().CopyTo(ms);
            ms.Position = 0;
            return ExcelUtil.ImportFromStream(ms, !file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase), sheetName, startRow, hasHeader) ?? new DataTable();
        }
        if (file.FileName.EndsWith(".csv"))
        {
            using var ms = new MemoryStream();
            file.OpenReadStream().CopyTo(ms);
            ms.Position = 0;
            return CsvUtil.ImportFromStream(ms, ",") ?? new DataTable();
        }
        if (file.FileName.EndsWith(".tsv"))
        {
            using var ms = new MemoryStream();
            file.OpenReadStream().CopyTo(ms);
            ms.Position = 0;
            return CsvUtil.ImportFromStream(ms, "\t") ?? new DataTable();
        }
        return new DataTable();
    }
    /// <summary>
    /// 接收文件并返回数据模型列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file">aspnetcore对象</param>
    /// <param name="sheetName">表名</param>
    /// <param name="startRow">数据起始位置</param>
    /// <param name="hasHeader">是否包含列</param>
    /// <param name="convertNames">指定转换的属性名名称映射</param>
    /// <returns></returns>
    public static List<T> SaveAsModels<T>(this IFormFile file,
        string sheetName = "sheet1",
        int startRow = 0,
        bool hasHeader = true,
        IEnumerable<NamePair>? convertNames = null) where T : class, new()
    {
        return file.SaveAsModelsWithRequired<T>(sheetName, startRow, hasHeader, null, convertNames) ?? [];
    }

    /// <summary>
    /// 接收文件并返回数据模型列表,
    /// 检查必填项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file">aspnetcore对象</param>
    /// <param name="sheetName">表名</param>
    /// <param name="startRow">数据起始位置</param>
    /// <param name="hasHeader">是否包含列</param>
    /// <param name="requiredColumn">必填项</param>
    /// <param name="convertNames">指定转换的属性名名称映射</param>
    /// <returns></returns>
    public static List<T>? SaveAsModelsWithRequired<T>(this IFormFile file,
        string sheetName = "sheet1",
        int startRow = 0,
        bool hasHeader = true,
        string? requiredColumn = null,
        IEnumerable<NamePair>? convertNames = null) where T : class, new()
    {
        var dt = file.SaveAsDataTable(sheetName, startRow, hasHeader);

        if (dt == null || dt.Rows == null || dt.Rows.Count < 1) return null;

        //清理必填项为空的数据
        if (!string.IsNullOrEmpty(requiredColumn))
        {
            List<DataRow> rows = [];

            foreach (DataRow dr in dt.Rows)
            {
                if (dr == null) continue;
                if (dr.IsNull(requiredColumn) || dr[requiredColumn] == null || string.IsNullOrEmpty(dr[requiredColumn].ToString()))
                {
                    rows.Add(dr);
                }
            }
            if (rows.Count > 0)
            {
                foreach (var dr in rows)
                {
                    dt.Rows.Remove(dr);
                }
                rows.Clear();
            }

            if (dt.Rows.Count < 1) return null;
        }

        return dt.ToList<T>(convertNames);
    }


    /// <summary>
    /// 接收文件并返回流
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Stream SaveAsStream(this IFormFile file)
    {
        file.ValidateFile();
        var ms = new MemoryStream();
        file.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }


    /// <summary>
    /// 接收文件并返回流
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task<Stream> SaveAsStreamAsync(this IFormFile file)
    {
        file.ValidateFile();
        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }


    /// <summary>
    /// 保存为内存流
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static Dictionary<string, Stream> SaveAsStreams(this IFormCollection collection)
    {
        Dictionary<string, Stream> fileDic = [];

        if (collection != null && collection.Files != null && collection.Files.Any())
        {
            foreach (var file in collection.Files)
            {
                try
                {
                    var stream = file.SaveAsStream();
                    fileDic.Add(file.FileName, stream);
                }
                catch (Exception ex)
                {
                    Logger.Error("UploadFileUtil.SaveAsStreamsAsync", ex);
                }
            }
        }
        return fileDic;
    }

    /// <summary>
    /// 保存为内存流
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, Stream>> SaveAsStreamsAsync(this IFormCollection collection)
    {
        Dictionary<string, Stream> fileDic = [];

        if (collection != null && collection.Files != null && collection.Files.Any())
        {
            foreach (var file in collection.Files)
            {
                try
                {
                    var stream = await file.SaveAsStreamAsync();
                    fileDic.Add(file.FileName, stream);
                }
                catch (Exception ex)
                {
                    Logger.Error("UploadFileUtil.SaveAsStreamsAsync", ex);
                }
            }
        }
        return fileDic;
    }

    /// <summary>
    /// 保存为内存字节
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, byte[]>> SaveAsBytesAsync(this IFormCollection collection)
    {
        Dictionary<string, byte[]> fileDic = [];

        if (collection != null && collection.Files != null && collection.Files.Any())
        {
            foreach (var file in collection.Files)
            {
                try
                {
                    var stream = await file.SaveAsStreamAsync();
                    fileDic.Add(file.FileName, await stream.ToBytesAsync());
                }
                catch (Exception ex)
                {
                    Logger.Error("UploadFileUtil.SaveAsBytesAsync", ex);
                }
            }
        }
        return fileDic;
    }

    /// <summary>
    /// 保存为内存流
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static Dictionary<string, Stream> SaveAsStreams(this IFormCollection collection, string filters = ".jpg|.xls|.xlsx|.doc|.docx|.pdf|.txt|.csv|.xml")
    {
        return collection.SaveAsStreams();
    }

    /// <summary>
    /// 保存客户端上传的文件
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    public static async Task<List<string>> SaveFilesAsync(this IFormCollection collection, string folder = "")
    {
        List<string> fileList = new();
        if (collection != null && collection.Files != null && collection.Files.Any())
            foreach (var file in collection.Files)
            {
                fileList.Add(await file.SaveFileAsync(folder));
            }
        return fileList;
    }

    /// <summary>
    /// 保存客户端上传的文件
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="env"></param>
    /// <param name="folder"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static List<string> SaveFiles(this IFormCollection collection, string folder = "")
    {
        List<string> fileList = [];
        if (collection != null && collection.Files != null && collection.Files.Any())
            foreach (var file in collection.Files)
            {
                fileList.Add(file.SaveFile(folder));
            }
        return fileList;
    }


    /// <summary>
    /// 接收文件并返回DataSet
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="sheetName"></param>
    /// <param name="startRow"></param>
    /// <param name="hasHeader"></param>
    /// <returns></returns>
    public static DataSet? SaveAsDataSet(this IFormCollection collection, string sheetName = "sheet1", int startRow = 0, bool hasHeader = true)
    {
        if (collection == null || collection.Files.Count < 1) return null;
        var ds = new DataSet();
        foreach (var item in collection.Files)
        {
            var dt = item.SaveAsDataTable(sheetName, startRow, hasHeader);
            if (dt != null)
            {
                ds.Tables.Add(dt);
            }
        }
        return ds;
    }

    /// <summary>
    /// 接收文件并返回DataSet
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="sheetName"></param>
    /// <param name="startRow"></param>
    /// <param name="hasHeader"></param>
    /// <returns></returns>
    public static DataSet? SaveAsDataSet(this IFormFileCollection collection, string sheetName = "sheet1", int startRow = 0, bool hasHeader = true)
    {
        if (collection == null || collection.Count < 1) return null;
        var ds = new DataSet();
        foreach (var item in collection)
        {
            var dt = item.SaveAsDataTable(sheetName, startRow, hasHeader);
            if (dt != null)
            {
                ds.Tables.Add(dt);
            }
        }
        return ds;
    }


}
