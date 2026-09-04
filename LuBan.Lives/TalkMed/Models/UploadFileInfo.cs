/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Lives.TalkMed.Models
*文件名： UploadFileInfo.cs
*版本号： V1.0.0.0
*唯一标识：9e888bb6-2a9c-4104-9653-fe891d7c335a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：UploadFileInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：UploadFileInfo 类
*
*****************************************************************************/

namespace LuBan.Lives.TalkMed.Models;

[DataContract]
/// <summary>
/// UploadFileInfo 模型类
/// </summary>
public class UploadFileInfo
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "path")]
    public string Path { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; }
}