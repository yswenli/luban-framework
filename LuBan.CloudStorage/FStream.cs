/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
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
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public Stream Stream { get; set; } = Stream.Null;


    public FStream()
    {

    }

    public FStream(string fileName, string contentType, Stream stream)
    {
        FileName = fileName;
        ContentType = contentType;
        Stream = stream;
    }

    public FStream(string fileName, Stream stream)
    {
        FileName = fileName;
        Stream = stream;
        ContentType = FileTypeUtil.GetHttpContentType(fileName);
    }
}
