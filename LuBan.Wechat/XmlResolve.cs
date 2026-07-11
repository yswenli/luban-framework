/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat
*文件名： XmlResolve
*版本号： V1.0.0.0
*唯一标识：303c4539-b708-4a99-ab2e-448e0c72cb7a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 13:29:24
*描述：xml序列化与反序例化
*
*=================================================
*修改标记
*修改时间：2023/12/5 13:29:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：xml序列化与反序例化
*
*****************************************************************************/
namespace LuBan.Wechat;
/// <summary>
/// xml序列化与反序例化
/// </summary>
public class XmlResolve
{
    /// <summary>
    /// 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    internal static string XmlSerialize<T>(T obj)
    {
        string result = string.Empty;
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        using (MemoryStream memoryStream = new MemoryStream())
        {
            xmlSerializer.Serialize(memoryStream, obj);
            result = Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        return result;
    }
    /// <summary>
    /// 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xmlString"></param>
    /// <returns></returns>
    internal static T? XmlDeserialize<T>(string xmlString) where T : Receive
    {
        Type typeFromHandle = typeof(T);
        xmlString = XMLReplaceXML_ClassName(xmlString, typeFromHandle.Name);
        XmlSerializer xmlSerializer = new XmlSerializer(typeFromHandle);
        using Stream input = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
        using XmlReader xmlReader = XmlReader.Create(input);
        var result = xmlSerializer.Deserialize(xmlReader);
        if (result == null) return default;
        return (T)result;
    }

    /// <summary>
    /// 替换
    /// </summary>
    /// <param name="xml"></param>
    /// <param name="classname"></param>
    /// <returns></returns>
    internal static string XMLReplaceXML_ClassName(string xml, string classname)
    {
        return xml.Replace("<xml>", "<" + classname + ">").Replace("</xml>", "</" + classname + ">");
    }
}
/// <summary>
/// XMLFixExtend
/// </summary>
public static class XMLFixExtend
{
    /// <summary>
    /// TextValue
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    internal static string TextValue(this string target)
    {
        if (string.IsNullOrEmpty(target))
        {
            return string.Empty;
        }
        return target.Replace("\r", string.Empty);
    }

    /// <summary>
    /// 读取微信xml内容
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Receive? XmlMsgRevolve(this string xml)
    {
        try
        {
            XDocument xDocument = XDocument.Parse(xml);
            List<XElement> source = (from c in xDocument.Elements()
                                     select c).ToList();
            string empty = string.Empty;
            return source.Elements("MsgType").First().Value.ToLower().Trim()
                switch
            {
                "text" => XmlResolve.XmlDeserialize<ReceiveText>(xml),
                "image" => XmlResolve.XmlDeserialize<ReceiveImage>(xml),
                "voice" => XmlResolve.XmlDeserialize<ReceiveVoice>(xml),
                "video" => XmlResolve.XmlDeserialize<ReceiveVideo>(xml),
                "location" => XmlResolve.XmlDeserialize<ReceiveLocation>(xml),
                "link" => XmlResolve.XmlDeserialize<ReceiveLink>(xml),
                "event" => XmlResolve.XmlDeserialize<ReceiveEvent>(xml),
                _ => null,
            };
        }
        catch (Exception ex)
        {
            throw new Exception("XmlMsgRevolve: " + xml + "-----\n" + ex.Message);
        }
    }
}
