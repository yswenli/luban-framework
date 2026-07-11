/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Models
*文件名： Attachment
*版本号： V1.0.0.0
*唯一标识：558bd697-c67b-44b4-984f-3b53c4c97dc4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/14 18:50:47
*描述：附件
*
*=================================================
*修改标记
*修改时间：2024/8/14 18:50:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件
*
*****************************************************************************/
namespace LuBan.EMailKit.Models;

/// <summary>
/// 附件
/// </summary>
public class Attachment
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    public Stream Content { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    public Attachment()
    {

    }
    /// <summary>
    /// 附件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="content"></param>
    public Attachment(string name, Stream content)
    {
        Name = name;
        Content = content;
    }

    /// <summary>
    /// 创建附件
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    public static Attachment Create(string localFilePath)
    {
        var fileName = Path.GetFileName(localFilePath);
        return new Attachment()
        {
            Name = fileName,
            Content = File.OpenRead(localFilePath)
        };
    }
    /// <summary>
    /// 创建附件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static Attachment Create(string name, Stream content)
    {
        return new Attachment()
        {
            Name = name,
            Content = content
        };
    }
    /// <summary>
    /// 创建附件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static Attachment Create(string name, byte bytes)
    {
        return new Attachment()
        {
            Name = name,
            Content = new MemoryStream(bytes)
        };
    }
}
