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

namespace LuBan.Common;

/// <summary>
/// 序列化
/// </summary>
public static class SerializeUtil
{
    /// <summary>
    /// json序列化
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="indented"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <param name="camelCase"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("json序列化")]
    public static string Serialize(object obj, bool indented = false, bool defalutVal = true, bool nullValue = false, bool camelCase = false)
    {
        if (obj == null)
        {
            return string.Empty;
        }
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null,
            DefaultIgnoreCondition = GetIgnoreCondition(defalutVal, nullValue),
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss.fff"));
        options.Converters.Add(new ExceptionJsonConverter());
        options.Converters.Add(new AssemblyJsonConverter());
        options.Converters.Add(new MemberInfoJsonConverter());
        return JsonSerializer.Serialize(obj, obj!.GetType(), options);
    }

    private static JsonIgnoreCondition GetIgnoreCondition(bool defalutVal, bool nullValue)
    {
        if (!defalutVal) return JsonIgnoreCondition.WhenWritingDefault;
        return nullValue ? JsonIgnoreCondition.Never : JsonIgnoreCondition.WhenWritingNull;
    }

    /// <summary>
    /// 序列化异常
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("json序列化")]
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
    /// json反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("json反序列化")]
    public static T? Deserialize<T>(string json, bool defalutVal = true, bool nullValue = false)
    {
        if (json.IsNullOrEmpty()) return default;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss.fff"));
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch
        {
            return default;
        }
    }



    /// <summary>
    /// json反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("json反序列化")]
    public static object? Deserialize(string json, Type type, bool defalutVal = true, bool nullValue = false)
    {
        if (json.IsNullOrEmpty()) return null;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss.fff"));
            return JsonSerializer.Deserialize(json, type, options);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// json反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("json反序列化")]
    public static object? Deserialize(string json, bool defalutVal = true, bool nullValue = false)
    {
        if (json.IsNullOrEmpty()) return null;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss.fff"));
            return JsonSerializer.Deserialize(json, typeof(object), options);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 通过json序列化和反序列化方式转换模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="defalutVal"></param>
    /// <param name="nullValue"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("通过json序列化和反序列化方式转换模型")]
    public static T? Convert<T>(dynamic val, bool defalutVal = true, bool nullValue = false)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss.fff"));
            options.Converters.Add(new ExceptionJsonConverter());
            options.Converters.Add(new AssemblyJsonConverter());
            options.Converters.Add(new MemberInfoJsonConverter());
            var json = JsonSerializer.Serialize(val, val?.GetType() ?? typeof(object), options);
            return JsonSerializer.Deserialize<T>(json, options);
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
    [RequiresUnreferencedCode("深复制当前对象")]
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
    [RequiresUnreferencedCode("深复制当前对象")]
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
    [RequiresUnreferencedCode("转换为JSON格式字符串")]
    public static string ToJson(this object obj, bool defalutVal = true, bool nullValue = false, bool hasIndentation = true)
    {
        if (obj == null) return string.Empty;
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = hasIndentation,
                DefaultIgnoreCondition = GetIgnoreCondition(defalutVal, nullValue),
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss.fff"));
            options.Converters.Add(new ExceptionJsonConverter());
            options.Converters.Add(new AssemblyJsonConverter());
            options.Converters.Add(new MemberInfoJsonConverter());
            return JsonSerializer.Serialize(obj, obj.GetType(), options);
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
    [RequiresUnreferencedCode("转换为对象")]
    public static T? ToObject<T>(this string json, bool defalutVal = true, bool nullValue = false)
    {
        return Deserialize<T>(json, defalutVal, nullValue);
    }


    /// <summary>
    /// 转json格式
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns> 
    [RequiresUnreferencedCode("转json格式")]
    private static string ConvertJsonString(string str)
    {
        try
        {
            using var doc = JsonDocument.Parse(str);
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            return JsonSerializer.Serialize(doc.RootElement, options);
        }
        catch
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
    [RequiresUnreferencedCode("XML反序列化")]
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
    /// XML反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xml"></param>
    /// <param name="rootXml"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("XML反序列化")]
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
    
    [RequiresUnreferencedCode("XML序列化")]
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
    [RequiresUnreferencedCode("XML反序列化")]
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

    [RequiresUnreferencedCode("FillModel")]
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

