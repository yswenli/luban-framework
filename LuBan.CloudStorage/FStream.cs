/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： FStream
*版本号： V1.0.0.0
*唯一标识：1695a178-32e1-4ca4-bc42-23b998157f8e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/15 12:00:08
*描述：文件流信息
*
*=================================================
*修改标记
*修改时间：2025/9/15 12:00:08
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文件流信息
*
*****************************************************************************/
namespace LuBan.CloudStorage;

/// <summary>
/// 文件流信息
/// </summary>
public class FStream
{
    /// <summary>
    /// 文件名（含扩展名）。
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件的 MIME 类型，如 image/png。
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 文件内容流，默认值为 <c>Stream.Null</c>。
    /// </summary>
    public Stream Stream { get; set; } = Stream.Null;


    /// <summary>
    /// 创建空实例：文件名为空、类型为空、内容为 <c>Stream.Null</c>。
    /// </summary>
    public FStream()
    {

    }

    /// <summary>
    /// 用已确定的文件名、MIME 类型和内容流初始化。
    /// </summary>
    /// <param name="fileName">文件名（含扩展名）。</param>
    /// <param name="contentType">文件的 MIME 类型。</param>
    /// <param name="stream">文件内容流。</param>
    public FStream(string fileName, string contentType, Stream stream)
    {
        FileName = fileName;
        ContentType = contentType;
        Stream = stream;
    }

    /// <summary>
    /// 按文件名自动推断 MIME 类型后初始化（调用 <c>FileTypeUtil.GetHttpContentType</c>）。
    /// </summary>
    /// <param name="fileName">文件名（含扩展名），同时用于推断类型。</param>
    /// <param name="stream">文件内容流。</param>
    public FStream(string fileName, Stream stream)
    {
        FileName = fileName;
        Stream = stream;
        ContentType = FileTypeUtil.GetHttpContentType(fileName);
    }
}
