/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： SerializeUtil
*版本号： V1.0.0.0
*唯一标识：45fe9777-9787-4611-924c-0779ab548fb7
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 13:15:45
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 13:15:45
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

using Newtonsoft.Json.Serialization;

using System.Xml;
using System.Xml.Serialization;

namespace LuBan.Common;

/// <summary>
/// 序列化
/// </summary>
public static class SerializeUtil
{
    /// <summary>
    /// newton.json序列化
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="indented"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <param name="camelCase"></param>
    /// <returns></returns>
    public static string Serialize(object obj, bool indented = false, bool defalutVal = true, bool nullValue = false, bool camelCase = false)
    {
        if (obj == null)
        {
            return string.Empty;
        }
        var settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = defalutVal ? DefaultValueHandling.Include : DefaultValueHandling.Ignore,
            NullValueHandling = nullValue ? NullValueHandling.Ignore : NullValueHandling.Include
        };

        if (camelCase)
        {
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
        settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
        settings.Converters ??= [];
        settings.Converters.Add(new NewtonsoftExceptionConverter());
        settings.Converters.Add(new AssemblyConverter());
        settings.Converters.Add(new MemberInfoConverter());
        return JsonConvert.SerializeObject(obj, indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None, settings);
    }

    /// <summary>
    /// 序列化异常
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static string ToJson(this Exception ex)
    {
        if (ex == null)
        {
            return string.Empty;
        }
        var json = Serialize(ex);
        if (json.IsNullOrEmpty())
        {
            json = Serialize(new Exception(ex.Message, ex));
        }
        return json;
    }

    /// <summary>
    /// newton.json反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    public static T? Deserialize<T>(string json, bool defalutVal = true, bool nullValue = false)
    {
        if (json.IsNullOrEmpty()) return default;
        try
        {
            JsonSerializerSettings settings = new();
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            settings.DefaultValueHandling = defalutVal ? DefaultValueHandling.Include : DefaultValueHandling.Ignore;
            settings.NullValueHandling = nullValue ? NullValueHandling.Ignore : NullValueHandling.Include;
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
        catch
        {
            return default;
        }
    }



    /// <summary>
    /// newton.json反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    public static dynamic? Deserialize(string json, Type type, bool defalutVal = true, bool nullValue = false)
    {
        JsonSerializerSettings settings = new();
        settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
        settings.DefaultValueHandling = defalutVal ? DefaultValueHandling.Include : DefaultValueHandling.Ignore;
        settings.NullValueHandling = nullValue ? NullValueHandling.Ignore : NullValueHandling.Include;
        settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
        return JsonConvert.DeserializeObject(json, type, settings);
    }

    /// <summary>
    /// newton.json反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    public static dynamic? Deserialize(string json, bool defalutVal = true, bool nullValue = false)
    {
        JsonSerializerSettings settings = new();
        settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
        settings.DefaultValueHandling = defalutVal ? DefaultValueHandling.Include : DefaultValueHandling.Ignore;
        settings.NullValueHandling = nullValue ? NullValueHandling.Ignore : NullValueHandling.Include;
        settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
        return JsonConvert.DeserializeObject(json, settings);
    }

    /// <summary>
    /// 通过json序列化和反序列化方式转换模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    public static T? Convert<T>(dynamic val, bool defalutVal = true, bool nullValue = false)
    {
        try
        {
            JsonSerializerSettings settings = new();
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            settings.DefaultValueHandling = defalutVal ? DefaultValueHandling.Include : DefaultValueHandling.Ignore;
            settings.NullValueHandling = nullValue ? NullValueHandling.Ignore : NullValueHandling.Include;
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
            settings.Converters ??= [];
            settings.Converters.Add(new NewtonsoftExceptionConverter());
            settings.Converters.Add(new AssemblyConverter());
            settings.Converters.Add(new MemberInfoConverter());
            var json = JsonConvert.SerializeObject(val, Newtonsoft.Json.Formatting.None, settings);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 深复制当前对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T? DeepClone<T>(this T obj)
    {
        if (obj == null) return default;
        var json = Serialize(obj);
        if (!string.IsNullOrEmpty(json))
            return Deserialize<T>(json);
        return default;
    }

    /// <summary>
    /// 深复制当前对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T? DeepClone<T>(this object obj)
    {
        var json = Serialize(obj);
        if (!string.IsNullOrEmpty(json))
            return Deserialize<T>(json);
        return default(T);
    }

    /// <summary>
    /// 转换成josn格式字符串
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <param name="hasIndentation"></param>
    /// <returns></returns>
    public static string ToJson(this object obj, bool defalutVal = true, bool nullValue = false, bool hasIndentation = true)
    {
        if (obj == null) return string.Empty;
        try
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter;
            if (hasIndentation)
            {
                jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
            }
            else
            {
                jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Newtonsoft.Json.Formatting.None
                };
            }
            JsonSerializerSettings settings = new();
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            settings.DefaultValueHandling = defalutVal ? DefaultValueHandling.Include : DefaultValueHandling.Ignore;
            settings.NullValueHandling = nullValue ? NullValueHandling.Include : NullValueHandling.Ignore;
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
            settings.Converters ??= [];
            settings.Converters.Add(new NewtonsoftExceptionConverter());
            settings.Converters.Add(new AssemblyConverter());
            settings.Converters.Add(new MemberInfoConverter());
            JsonSerializer serializer = JsonSerializer.Create(settings);
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }
        catch { }
        return string.Empty;
    }

    /// <summary>
    /// newton.json反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    public static T? ToObject<T>(this string json, bool defalutVal = true, bool nullValue = false)
    {
        return Deserialize<T>(json, defalutVal, nullValue);
    }


    /// <summary>
    /// 转json格式
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string ConvertJsonString(string str)
    {
        JsonSerializer serializer = new();
        TextReader tr = new StringReader(str);
        JsonTextReader jtr = new(tr);
        var obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }
        else
        {
            return str;
        }
    }

    #region stuct

    /// <summary>
    /// The serialize delegate.
    /// </summary>
    /// <param name="obj">obj to be serialized.</param>
    /// <returns></returns>
    public delegate string TypeSerializeHandler(object obj);

    /// <summary>
    /// The deserialize delegate.
    /// </summary>
    /// <param name="data">the data to be deserialied.</param>
    /// <returns></returns>
    public delegate object TypeDeserializeHandler(string data);

    private static ConcurrentDictionary<Type, KeyValuePair<TypeSerializeHandler, TypeDeserializeHandler>> handlers = new ConcurrentDictionary<Type, KeyValuePair<TypeSerializeHandler, TypeDeserializeHandler>>();

    /// <summary>
    /// Deserializes the specified return type.
    /// </summary>
    /// <param name="returnType">Type of the return.</param>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    public static object? XmlDeserialize(Type returnType, string? data)
    {
        if (data.IsNullOrEmpty())
        {
            return null;
        }

        if (handlers.ContainsKey(returnType))
        {
            return handlers[returnType].Value(data);
        }
        else
        {
            StringReader sr = new(data);
            XmlSerializer serializer = new(returnType);
            var obj = serializer.Deserialize(sr);
            sr.Close();
            return obj;
        }
    }

    /// <summary>
    /// 返序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xml"></param>
    /// <param name="rootXml"></param>
    /// <returns></returns>
    public static T? XmlDeserialize<T>(string xml, string rootXml = "")
    {
        if (string.IsNullOrEmpty(xml))
        {
            return default;
        }
        XmlSerializer serializer;
        if (!string.IsNullOrEmpty(rootXml))
        {
            serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootXml));
        }
        else
        {
            serializer = new XmlSerializer(typeof(T));
        }
        using (var reader = new StringReader(xml))
        {
            var obj = serializer.Deserialize(reader);
            if (obj is T t)
                return t;
            return default;
        }
    }

    /// <summary>
    /// Serializes the specified obj.
    /// </summary>
    /// <param name="obj">The obj.</param>
    /// <returns></returns>
    public static string XmlSerialize(object obj)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        if (handlers.ContainsKey(obj.GetType()))
        {
            return handlers[obj.GetType()].Key(obj);
        }
        else
        {
            StringBuilder sb = new();
            StringWriter sw = new(sb);
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(sw, obj);
            sw.Close();
            return sb.ToString();
        }
    }

    #endregion stuct

    #region 因为dotnet core的xml反序列化bug，自定义

    /// <summary>
    /// 因为dotnet core的xml反序列化bug，自定义
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xml"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static T? CustomXmlDeserialize<T>(string xml, string prefix = "")
    {
        if (string.IsNullOrEmpty(xml)) return default;

        var obj = Activator.CreateInstance<T>();
        if (obj == null) return default;

        var xmlDoc = new XmlDocument();

        xmlDoc.LoadXml(xml);
        if (xmlDoc.DocumentElement == null) return default;
        var elems = xmlDoc.DocumentElement.ChildNodes;
        if (elems != null && elems.Count > 0)
        {
            var eFirst = elems[0];
            if (eFirst == null) return default;
            var name = eFirst.Name;
            var nameArr = name.Split(":");
            if (nameArr.Length > 1)
            {
                prefix = nameArr[0] + ":";
            }
        }
        return (T)FillModel(obj, elems, prefix);
    }

    private static object FillModel(object? obj, XmlNodeList? xmlNodeList, string prefix = "")
    {
        if (obj == null) throw new Exception("obj is null");
        var type = obj.GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            Attribute[] attributes = [];
            string pXMLName = string.Empty;

            var elems = GetItems(xmlNodeList)?.Where(q => q != null && (q.Name.Equals(property.Name, true) || q.Name.Equals(prefix + property.Name, true))).ToList();

            if (elems == null || elems.Count <= 0)
            {
                #region 由于生成实体的时候可能会有改名的情况所以只能取xml相关标签的name作为判断依据
                attributes = Attribute.GetCustomAttributes(property, typeof(XmlArrayAttribute));
                if (attributes != null && attributes.Length > 0)
                {
                    pXMLName = ((XmlArrayAttribute)attributes[0]).ElementName;
                }
                else
                {
                    attributes = Attribute.GetCustomAttributes(property, typeof(XmlArrayItemAttribute));
                    if (attributes != null && attributes.Length > 0)
                    {
                        pXMLName = ((XmlArrayItemAttribute)attributes[0]).ElementName;
                    }
                    else
                    {
                        attributes = Attribute.GetCustomAttributes(
                          property, typeof(XmlElementAttribute));
                        if (attributes != null && attributes.Length > 0)
                        {
                            pXMLName = ((XmlElementAttribute)attributes[0]).ElementName;
                        }

                    }
                }
                if (string.IsNullOrEmpty(pXMLName))
                {
                    pXMLName = property.Name;
                }
                #endregion
                elems = GetItems(xmlNodeList)?.Where(q => q != null && (q.Name.Equals(pXMLName, true) || q.Name.Equals(prefix + pXMLName, true))).ToList();
                if (elems == null || elems.Count <= 0) continue;
            }
            if (property.PropertyType.IsClass)
            {
                if (property.PropertyType.Name != "String")
                {
                    if (property.PropertyType.Name == "Nullable`1")
                    {
                        var gType = property.PropertyType.GetGenericArguments().First();

                        if (gType.IsClass)
                        {
                            if (gType.Name == "String")
                            {
                                property.SetValue(obj, elems.First()?.FirstChild?.Value);
                            }
                            else
                            {
                                var sNodeList = elems?.First()?.ChildNodes;
                                if (sNodeList != null)
                                {
                                    var sobj = Activator.CreateInstance(gType);
                                    var sval = FillModel(sobj, sNodeList, prefix);
                                    property.SetValue(obj, sval);
                                }
                            }
                        }
                        else
                        {
                            var val = elems?.First()?.FirstChild?.Value;
                            if (!string.IsNullOrEmpty(val))
                            {
                                property.SetValue(obj, val.ConvertToType(property.PropertyType));
                            }
                        }
                    }
                    else if (property.PropertyType.Name == "List`1")
                    {
                        var gType = property.PropertyType.GetGenericArguments().First();

                        var list = ReflectionUtil.CreateList(gType);

                        if (gType.IsClass)
                        {
                            if (gType.Name == "String")
                            {
                                foreach (var item in elems)
                                {
                                    if (item != null)
                                        list.Add(item.Value);
                                }
                                property.SetValue(obj, list);
                            }
                            else
                            {
                                #region 由于生成实体的时候可能会有改名的情况所以只能取xml相关标签的name作为判断依据
                                if (string.IsNullOrEmpty(pXMLName))
                                {

                                    attributes = Attribute.GetCustomAttributes(property, typeof(XmlArrayAttribute));
                                    if (attributes != null && attributes.Length > 0)
                                    {
                                        pXMLName = ((XmlArrayAttribute)attributes[0]).ElementName;
                                    }
                                    else
                                    {
                                        attributes = Attribute.GetCustomAttributes(property, typeof(XmlArrayItemAttribute));
                                        if (attributes != null && attributes.Length > 0)
                                        {
                                            pXMLName = ((XmlArrayItemAttribute)attributes[0]).ElementName;
                                        }
                                        else
                                        {
                                            attributes = Attribute.GetCustomAttributes(
                                              property, typeof(XmlElementAttribute));
                                            if (attributes != null && attributes.Length > 0)
                                            {
                                                pXMLName = ((XmlElementAttribute)attributes[0]).ElementName;
                                            }

                                        }
                                    }
                                }
                                if (string.IsNullOrEmpty(pXMLName))
                                {
                                    pXMLName = property.Name;
                                }
                                #endregion
                                foreach (var item in elems)
                                {
                                    if (item == null) continue;

                                    if (item.Name.Equals(pXMLName, true) || item.Name.Equals(prefix + pXMLName, true))
                                    {
                                        var sNodes = item.ChildNodes;
                                        if (sNodes == null || sNodes.Count < 1) continue;
                                        var sObj = Activator.CreateInstance(gType);
                                        var sVal = FillModel(sObj, sNodes, prefix);
                                        list.Add(sVal);
                                    }
                                    else
                                    {
                                        var sNodes = item.ChildNodes;
                                        if (sNodes == null || sNodes.Count < 1) continue;
                                        foreach (XmlNode nd in sNodes)
                                        {
                                            if (nd.Name.Equals(pXMLName, true) || nd.Name.Equals(prefix + pXMLName, true))
                                            {
                                                var sObj = Activator.CreateInstance(gType);
                                                var sVal = FillModel(sObj, nd.ChildNodes, prefix);
                                                list.Add(sVal);
                                            }
                                        }
                                    }


                                }
                                property.SetValue(obj, list);
                            }
                        }
                        else
                        {
                            foreach (var item in elems)
                            {
                                if (item != null)
                                    list.Add(item.Value);
                            }
                            property.SetValue(obj, list);
                        }
                    }
                    else
                    {

                        var sNodes = elems?.First()?.ChildNodes;
                        if (sNodes == null || sNodes.Count < 1) continue;
                        var sObj = Activator.CreateInstance(property.PropertyType);
                        var sVal = FillModel(sObj, sNodes, prefix);


                        property.SetValue(obj, sVal);
                    }
                }
                else
                {
                    var val = elems.First()?.FirstChild?.Value;
                    if (!string.IsNullOrEmpty(val))
                    {
                        property.SetValue(obj, val);
                    }
                }
            }
            else
            {
                var val = elems.First()?.FirstChild?.Value;
                if (!string.IsNullOrEmpty(val))
                {
                    property.SetValue(obj, val.ConvertToType(property.PropertyType));
                }
            }
        }

        return obj;
    }

    private static IEnumerable<XmlElement?> GetItems(XmlNodeList? xmlNodeList)
    {
        if (xmlNodeList == null || xmlNodeList.Count < 1) yield break;

        foreach (XmlElement item in xmlNodeList)
        {
            yield return item;
        }
    }

    #endregion 因为dotnet core的xml反序列化bug，自定义      
}


/// <summary>
/// Json.NET 序列化/反序列化扩展方法
/// </summary>
public class NewtonsoftExceptionConverter : JsonConverter<Exception>
{
    /// <summary>
    /// 允许反序列化（如需反序列化 Exception，需额外处理，通常不推荐）
    /// </summary>
    public override bool CanRead => false;

    /// <summary>
    /// 自定义序列化逻辑
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, Exception? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        // 只序列化 Exception 的核心信息，避免循环引用和不可序列化成员
        writer.WriteStartObject();

        // 1. 异常类型（如 "System.NullReferenceException"）
        writer.WritePropertyName("ExceptionType");
        writer.WriteValue(value.GetType().FullName);

        // 2. 异常消息
        writer.WritePropertyName("Message");
        writer.WriteValue(value.Message);

        // 3. 栈跟踪（可选，根据需求决定是否包含）
        writer.WritePropertyName("StackTrace");
        writer.WriteValue(value.StackTrace);

        // 4. 内部异常（递归处理，避免循环引用）
        if (value.InnerException != null)
        {
            writer.WritePropertyName("InnerException");
            WriteJson(writer, value.InnerException, serializer); // 递归序列化内部异常
        }

        // 5. 其他自定义信息（如异常来源、HResult 等，按需添加）
        writer.WritePropertyName("Source");
        writer.WriteValue(value.Source);

        writer.WriteEndObject();
    }

    /// <summary>
    /// 反序列化逻辑（如需支持，需手动处理，通常 Exception 反序列化意义不大）
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override Exception ReadJson(JsonReader reader, Type objectType, Exception? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("Exception 反序列化暂不支持");
    }
}
/// <summary>
/// 自定义 Assembly 转换器：只序列化程序集名称（忽略循环引用属性）
/// </summary>
public class AssemblyConverter : JsonConverter<Assembly>
{
    /// <summary>
    /// 允许序列化
    /// </summary>
    public override bool CanRead => false; // 无需反序列化
    /// <summary>
    /// 自定义序列化逻辑
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, Assembly? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        // 只序列化程序集的核心信息，避免触发循环引用
        writer.WriteStartObject();
        writer.WritePropertyName("AssemblyName");
        writer.WriteValue(value.FullName); // 程序集全名
        writer.WritePropertyName("Location");
        writer.WriteValue(value.Location); // 程序集路径（可选，按需保留）
        writer.WriteEndObject();
    }
    /// <summary>
    /// 反序列化逻辑（如需支持，需手动处理，通常不推荐）
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override Assembly ReadJson(JsonReader reader, Type objectType, Assembly? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("无需反序列化 Assembly");
    }
}
/// <summary>
/// 自定义 MemberInfo 转换器（处理方法/属性等成员信息）
/// </summary>
public class MemberInfoConverter : JsonConverter<MemberInfo>
{
    /// <summary>
    /// 允许序列化
    /// </summary>
    public override bool CanRead => false;
    /// <summary>
    /// 自定义序列化逻辑
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, MemberInfo? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        // 只序列化成员的关键信息，不序列化 DeclaringType/Module 等可能引发循环的属性
        writer.WriteStartObject();
        writer.WritePropertyName("MemberName");
        writer.WriteValue(value.Name); // 成员名称
        writer.WritePropertyName("MemberType");
        writer.WriteValue(value.MemberType.ToString()); // 成员类型（如 Method、Property）
        writer.WritePropertyName("DeclaringTypeName");
        writer.WriteValue(value.DeclaringType?.FullName); // 声明类型名（只保留字符串，避免循环）
        writer.WriteEndObject();
    }
    /// <summary>
    /// 反序列化逻辑（如需支持，需手动处理，通常不推荐）
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override MemberInfo ReadJson(JsonReader reader, Type objectType, MemberInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("无需反序列化 MemberInfo");
    }
}