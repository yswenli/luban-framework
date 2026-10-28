/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Http
*文件名： FileParameter
*版本号： V1.0.0.0
*唯一标识：bfcf3866-1796-48de-bd1b-a61b6310f105
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/1 13:44:46
*描述：文件参数实体
*
*=================================================
*修改标记
*修改时间：2025/9/1 13:44:46
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文件参数实体
*
*****************************************************************************/
namespace LuBan.Common.Http;


/// <summary>
/// 文件参数实体
/// </summary>
public class FileParameter
{
    /// <summary>
    /// 文件的二进制内容。
    /// </summary>
    public byte[] File { get; set; }
    /// <summary>
    /// 上传时使用的文件名。
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// 文件的 MIME 类型。
    /// </summary>
    public string ContentType { get; set; }
    /// <summary>
    /// 只用文件内容构造，文件名和 MIME 类型均置空。
    /// </summary>
    /// <param name="file">文件的二进制内容。</param>
    public FileParameter(byte[] file) : this(file, "") { }
    /// <summary>
    /// 指定文件内容与文件名构造，MIME 类型置空。
    /// </summary>
    /// <param name="file">文件的二进制内容。</param>
    /// <param name="filename">上传时使用的文件名。</param>
    public FileParameter(byte[] file, string filename) : this(file, filename, "") { }
    /// <summary>
    /// 完整初始化文件内容、文件名与 MIME 类型。
    /// </summary>
    /// <param name="file">文件的二进制内容。</param>
    /// <param name="filename">上传时使用的文件名。</param>
    /// <param name="contenttype">文件的 MIME 类型。</param>
    public FileParameter(byte[] file, string filename, string contenttype)
    {
        File = file;
        FileName = filename;
        ContentType = contenttype;
    }
}
