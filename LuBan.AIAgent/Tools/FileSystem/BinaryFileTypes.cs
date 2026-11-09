/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__ _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.FileSystem
*文件名： BinaryFileTypes
*版本号： V1.0.0.0
*唯一标识：f7c2d9a4-8e31-4b6f-a05c-2d3e4f5a6b71
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/6
*描述：二进制扩展名判定与「不能按文本读取」的统一提示
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.FileSystem;

/// <summary>
/// 二进制/表格扩展名判定，以及统一的不可文本读取提示。
/// <para>内置文件工具与内置 MCP 文件客户端共用同一份判定，避免两条路径给出不同结论。</para>
/// </summary>
internal static class BinaryFileTypes
{
    /// <summary>按文本读取只会得到乱码的扩展名。</summary>
    internal static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".exe", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".tif", ".tiff",
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".woff", ".woff2", ".ttf", ".eot", ".otf",
        ".mp3", ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".wav",
        ".bin", ".dat", ".db", ".sqlite",
        ".so", ".dylib", ".a", ".lib", ".obj", ".o"
    };

    /// <summary>电子表格扩展名：应引导到 ReadSpreadsheet。</summary>
    internal static readonly HashSet<string> SpreadsheetExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xls", ".xlsx", ".xlsm", ".xlsb", ".ods"
    };

    /// <summary>判断扩展名是否为二进制。</summary>
    internal static bool Contains(string? extension)
        => !string.IsNullOrEmpty(extension) && Extensions.Contains(extension);

    /// <summary>判断扩展名是否为电子表格。</summary>
    internal static bool IsSpreadsheet(string? extension)
        => !string.IsNullOrEmpty(extension) && SpreadsheetExtensions.Contains(extension);

    /// <summary>
    /// 生成「该文件不能用 ReadFile 按文本读取」的可操作提示。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <param name="extension">文件扩展名。</param>
    internal static string UnreadableMessage(string path, string extension)
    {
        if (IsSpreadsheet(extension))
        {
            return $"{path} 是电子表格文件（{extension}），不能按文本读取。"
                + "请改用 ReadSpreadsheet（支持 .xlsx / .csv / .tsv）；旧版 .xls 请先另存为 .xlsx 或 .csv。";
        }

        return $"{path} 是二进制文件（{(string.IsNullOrEmpty(extension) ? "无扩展名/未知格式" : extension)}），不能按文本读取。"
            + "请说明需要从中提取什么信息，改用对应的解析工具或脚本处理。";
    }
}