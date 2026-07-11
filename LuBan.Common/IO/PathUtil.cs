/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： PathUtil
*版本号： V1.0.0.0
*唯一标识：8cd21318-2990-4479-86af-8321471efb0f
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/5/10 19:42:39
*描述：路径工具类
*
*=====================================================================
*修改标记
*修改时间：2021/5/10 19:42:39
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：路径工具类
*
*****************************************************************************/

namespace LuBan.Common.IO;

/// <summary>
/// 路径工具类
/// </summary>
public static class PathUtil
{
    static string _currentPath = string.Empty;

    /// <summary>
    /// 获取当前程序的根目录
    /// </summary>
    public static string CurrentPath
    {
        get
        {
            if (_currentPath.IsNullOrEmpty())
            {
                _currentPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? string.Empty;
            }
            return _currentPath ?? string.Empty;
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="folders"></param>
    /// <returns></returns>
    public static string Combine(params string[] folders)
    {
        return Path.Combine(folders);
    }

    /// <summary>
    /// 获取当前目录下的全名称
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetRootFullName(string fileName)
    {
        var result = Path.Combine(CurrentPath, fileName);
        if (result.IsNullOrEmpty() || result == fileName)
        {
            result = CurrentPath + fileName;
        }
        return result;
    }

    /// <summary>
    /// 获取当前目录下的全名称
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetRootFullName(string dir, string fileName)
    {
        var dirPath = Path.Combine(CurrentPath, dir);
        Create(dirPath);
        return Path.Combine(dirPath, fileName);
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path"></param>
    public static void Create(string? path)
    {
        if (path.IsNullOrEmpty()) return;
        var dir = Path.GetDirectoryName(path);
        if (dir.IsNotNullOrEmpty() && !Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool Exists(string path)
    {
        return Path.Exists(path);
    }


    /// <summary>
    /// 返回指定的全路径
    /// </summary>
    /// <param name="root"></param>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetFullName(string root, string path, string fileName)
    {
        var rpath = Path.Combine(root, path);
        Create(rpath);
        return GetFullName(rpath, fileName);
    }

    /// <summary>
    /// 返回指定的全路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetFullName(string path, string fileName)
    {
        return Path.Combine(path, fileName);
    }

    /// <summary>
    /// 获取目录
    /// </summary>
    /// <param name="folders"></param>
    /// <returns></returns>
    public static string GetRootFilePath(params string[] folders)
    {
        if (folders == null || folders.Length < 1) return CurrentPath;
        var paths = new List<string>
        {
            CurrentPath
        };
        paths.AddRange(folders);
        var dir = Path.Combine([.. paths]);
        Create(dir);
        return dir;
    }

    /// <summary>
    /// 获取专用目录
    /// </summary>
    /// <param name="folderType"></param>
    /// <returns></returns>
    public static string GetSpecialPath(EnumFolderType folderType = EnumFolderType.App)
    {
        switch (folderType)
        {
            case EnumFolderType.App:
                return CurrentPath;
            case EnumFolderType.Wwwroot:
                return Path.Combine(CurrentPath, "wwwroot");
            case EnumFolderType.Upload:
                return Path.Combine(CurrentPath, "upload");
            default:
                return CurrentPath;
        }
    }

    /// <summary>
    /// 获取文件名
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetFileName(string filePath)
    {
        return Path.GetFileName(filePath);
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetExtensionName(this string filePath)
    {
        return Path.GetExtension(filePath);
    }

    /// <summary>
    /// 获取不带扩展名的文件名
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetFileNameWithoutExtension(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// 获取文件目录，文件重命名
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetNewFilePath(this string filePath)
    {
        if (filePath.IsNullOrEmpty()) return string.Empty;
        var path = filePath.Replace(Path.GetFileName(filePath), "");
        if (path.IsNullOrEmpty())
        {
            var extensionName = GetExtensionName(filePath);
            return $"{DateTimeOffset.Now:yyyyMMddHHmmssfff}{RandomUtil.GetRndCodeStr(6, 2)}{extensionName}";
        }
        else
        {
            var extensionName = GetExtensionName(filePath);
            return Path.Combine(path, $"{DateTimeOffset.Now:yyyyMMddHHmmssfff}{RandomUtil.GetRndCodeStr(6, 2)}{extensionName}");
        }
    }

    /// <summary>
    /// 是否包含扩展名
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="extensionName"></param>
    /// <returns></returns>
    public static bool ContainsExtensionName(this string fileName, string extensionName)
    {
        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(extensionName)) return false;

        return fileName.GetExtensionName().IndexOf(extensionName, StringComparison.OrdinalIgnoreCase) > -1;
    }

    /// <summary>
    /// 获取全部目录
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    public static List<DirectoryInfo> GetAllDirs(string dirPath)
    {
        List<DirectoryInfo> list = new();

        var dir = new DirectoryInfo(dirPath);

        foreach (var item in dir.GetDirectories())
        {
            list.Add(item);
            var sDirs = item.GetDirectories();
            if (sDirs != null && sDirs.Any())
            {
                foreach (var sitem in sDirs)
                {
                    list.AddRange(GetAllDirs(sitem.FullName));
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 获取临时文件目录
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetTempFilePath(string fileName)
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Templates), fileName);
    }



    /// <summary>
    /// 覆盖，将source目录下的文件和子目隶复制到dest中
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    public static void Cover(string source, string dest)
    {
        if (!Directory.Exists(source))
        {
            return;
        }

        var paths = Directory.GetFileSystemEntries(source);

        if (paths == null || paths.Length < 1) return;

        Create(dest);

        foreach (string path in paths)
        {
            if (Directory.Exists(path))
            {
                string currentdir = Path.Combine(dest, Path.GetFileName(path));
                Create(currentdir);
                Cover(path, dest);
            }
            else
            {
                string srcfileName = Path.Combine(dest, Path.GetFileName(path));
                Create(dest);
                File.Copy(path, srcfileName);
            }
        }
    }

    /// <summary>
    /// 移动目录或文件
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    public static void Move(string source, string dest)
    {
        if (Directory.Exists(source))
        {
            Directory.Move(source, dest);
        }
    }

    /// <summary>
    /// 删除目录及其文件
    /// </summary>
    /// <param name="dir"></param>
    public static void Delete(DirectoryInfo dir)
    {
        if (!dir.Exists) return;

        var files = dir.EnumerateFiles();

        if (files != null)
        {
            foreach (var item in files)
            {
                item.Delete();
            }
        }

        var dirs = dir.EnumerateDirectories();

        if (dirs != null)
        {
            foreach (var item in dirs)
            {
                Delete(item);
                item.Delete();
            }
        }

        dir.Delete();
    }

    /// <summary>
    /// 删除目录及其文件
    /// </summary>
    /// <param name="dir"></param>
    public static void Delete(string dir)
    {
        var fi = new DirectoryInfo(dir);

        Delete(fi);
    }

    /// <summary>
    /// 将拼接的路径转换成正确的路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ToCorrectPath(this string filePath)
    {
        if (RuntimeUtil.IsWindows())
        {
            filePath = filePath.Replace("/", "\\");
        }
        else
        {
            filePath = filePath.Replace("\\", "/");
        }
        Create(Path.GetDirectoryName(filePath));
        return filePath;
    }
}
