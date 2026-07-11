/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
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
    public byte[] File { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public FileParameter(byte[] file) : this(file, "") { }
    public FileParameter(byte[] file, string filename) : this(file, filename, "") { }
    public FileParameter(byte[] file, string filename, string contenttype)
    {
        File = file;
        FileName = filename;
        ContentType = contenttype;
    }
}
