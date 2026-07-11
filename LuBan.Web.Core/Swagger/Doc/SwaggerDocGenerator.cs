/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger.Doc
*文件名： SwaggerDocGenerator
*版本号： V1.0.0.0
*唯一标识：c0107be4-1d5d-4c30-9ccc-e44cc25124b9
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 11:21:43
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 11:21:43
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Web.Core.Swagger.Doc.Models;

namespace LuBan.Web.Core.Swagger.Doc;

#nullable disable warnings

/// <summary>
/// Swagger 文档生成器
/// </summary>
public class SwaggerDocGenerator : ISwaggerDocGenerator
{
    /// <summary>
    /// SwaggerGenerator
    /// </summary>
    private readonly SwaggerGenerator _generator;
    /// <summary>
    /// Schemas
    /// </summary>
    private IDictionary<string, OpenApiSchema> Schemas;
    /// <summary>
    /// contentType
    /// </summary>
    const string contentType = "application/json";
    
    /// <summary>
    /// 文档缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> _docCache = new();
    /// <summary>
    /// SDK缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, byte[]> _sdkCache = new();
    /// <summary>
    /// 文档内容缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, byte[]> _docContentCache = new();
    
    /// <summary>
    /// SwaggerDocGenerator
    /// </summary>
    /// <param name="swagger"></param>
    public SwaggerDocGenerator(SwaggerGenerator swagger)
    {
        _generator = swagger;
    }
    /// <summary>
    /// 生成MarkDown
    /// </summary>
    /// <returns></returns>
    public string GetSwaggerDoc(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new Exception("name is null !");
            
        // 检查缓存
        if (_docCache.TryGetValue(name, out string cachedDoc))
        {
            return cachedDoc;
        }
        
        var document = _generator.GetSwagger(name);
        if (document == null)
            throw new Exception("document is null !");
        Schemas = document.Components.Schemas;
        var markDown = new StringPlus();
        markDown.AppendLine(document?.Info?.Title?.H() ?? "");//文档标题
        markDown.AppendLine(document?.Info?.Description?.Ref() ?? "");//文档描述
        var line = 1;
        foreach (var path in document!.Paths)
        {
            foreach (var operationItem in path.Value.Operations)
            {
                try
                {
                    var operation = operationItem.Value;
                    var method = operationItem.Key.ToString();
                    var row = new StringPlus();
                    var url = path.Key;
                    var title = $"{line}、{operation.Summary ?? url}";
                    var query = GetMarkdownForParameters(operation.Parameters);
                    var (requestExapmle, requestSchema) = GetMarkdownForRequestBody(operation.RequestBody);
                    var (responseExapmle, responseSchema) = GetMarkdownResponses(operation.Responses);
                    row.AppendLine(title.H(2));//接口名称
                    line++;
                    row.AppendLine("基本信息".H(3).NewLine());//基本信息
                    row.AppendLine($"{"接口地址：".B()}{url}".Li().NewLine());
                    row.AppendLine($"{"请求方式：".B()}{method}".Li().NewLine());
                    if (method == "Post" || method == "Put")
                    {
                        row.AppendLine($"{"接口请求类型：".B()}{contentType}".Li().NewLine());
                    }
                    if (string.IsNullOrWhiteSpace(query) == false)//Query
                    {
                        row.AppendLine("接口输入参数".H(3));
                        row.AppendLine(query);
                    }
                    if (string.IsNullOrWhiteSpace(requestSchema) == false)//RequestSchema
                    {
                        row.AppendLine("接口请求结构".H(3));
                        row.AppendLine(requestSchema);
                    }
                    if (string.IsNullOrWhiteSpace(requestExapmle) == false)//RequestBody
                    {
                        row.AppendLine("接口请求Json例子".H(3));
                        row.AppendLine(requestExapmle.Code());
                    }
                    if (string.IsNullOrWhiteSpace(responseSchema) == false)//ResponseSchema
                    {
                        row.AppendLine("接口回复结构".H(3));
                        row.AppendLine(responseSchema);
                    }
                    if (string.IsNullOrWhiteSpace(responseExapmle) == false)//ResponseBody
                    {
                        row.AppendLine("接口回复例子".H(3));
                        row.AppendLine(responseExapmle.Code());
                    }
                    if (string.IsNullOrWhiteSpace(row.ToString()) == false)
                        markDown.AppendLine(row.ToString().Br());
                }
                catch
                {
                    continue;
                }
            }
        }
        var docContent = markDown.ToString();
        
        // 存入缓存
        _docCache[name] = docContent;
        
        return docContent;
    }
    /// <summary>
    /// 获取参数
    /// </summary>
    /// <param name="apiParameters"></param>
    /// <returns></returns>
    private string GetMarkdownForParameters(IList<OpenApiParameter> apiParameters)
    {
        var str = new StringPlus();
        if (apiParameters == null || apiParameters.Count < 1) return str.ToString();
        str.AppendLine("|参数名称|参数类型|参数位置|是否可空|描述|");
        str.AppendLine("|:----:|:----:|:----:|:----:|:----:|");
        foreach (var parameter in apiParameters)
        {
            var des = parameter.Description;
            if (des.IsNullOrEmpty()) des = "-";
            str.AppendLine($"|{parameter.Name}|{parameter.Schema.Type ?? parameter.Schema.Reference.Id}|{parameter.In}|{(parameter.Required ? "否" : "是")}|{des}|");
        }
        return str.ToString();
    }
    /// <summary>
    /// 获取 RequestBody 参数说明、JSON 示例
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    private (string? exampleJson, string? markdownText) GetMarkdownForRequestBody(OpenApiRequestBody body)
    {
        if (body == null || body.Content.ContainsKey(contentType) == false) return (null, null);
        string? exampleJson = null, markdownText = null;
        var schema = body.Content[contentType].Schema;
        exampleJson = GetExapmple(schema)?.ToJson() ?? "";
        markdownText = GetMarkDownForModelInfo(schema, (id) => GetModelInfo(id) ?? "");
        return (exampleJson, markdownText);
    }
    /// <summary>
    /// 获取 GetResponses 参数说明、JSON 示例
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    private (string? exampleJson, string? markdownText) GetMarkdownResponses(OpenApiResponses body)
    {
        if (body == null || body["200"].Content.ContainsKey(contentType) == false) return (null, null);
        string? exampleJson = null, markdownText = null;
        var schema = body["200"].Content[contentType].Schema;
        exampleJson = GetExapmple(schema)?.ToJson() ?? "";
        markdownText = GetMarkDownForModelInfo(schema, (id) => GetModelInfo(id, false) ?? "");
        return (exampleJson, markdownText);
    }

    /// <summary>
    /// 获取 Body 示例
    /// </summary>
    /// <param name="apiSchema"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private object? GetExapmple(OpenApiSchema apiSchema, int level = 1)
    {
        if (apiSchema == null) return null;
        if (level > 2) return null;
        object? exapmle = null;
        if (apiSchema.IsObject(Schemas))
        {
            var key = apiSchema.Reference.Id;
            level++;
            exapmle = GetExapmple(key, level);
        }
        else if (apiSchema.IsArray())
        {
            if (apiSchema.IsBaseTypeArray())
                exapmle = new[] { GetDefaultValue(apiSchema.Items.Type) };
            else
            {
                level++;
                if (apiSchema.Items != null && apiSchema.Items.Reference != null && apiSchema.Items.Reference.Id != null)
                    exapmle = new[] { GetExapmple(apiSchema.Items.Reference.Id, level) };
            }

        }
        else if (apiSchema.IsEnum(Schemas))
        {
            var key = apiSchema.Reference.Id;
            exapmle = GetEnum(key).Select(x => x.Value).Min();
        }
        else
        {
            exapmle = GetDefaultValue(apiSchema.Type);
        }
        return exapmle;
    }

    /// <summary>
    /// 获取枚举的值
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    private int[] GetEnumValues(string enumType) => GetEnum(enumType).Select(x => x.Value).ToArray();
    /// <summary>
    /// 获取枚举
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    private IEnumerable<OpenApiInteger> GetEnum(string enumType) => GetEnumSchema(enumType).Enum.Select(x => ((OpenApiInteger)x));
    /// <summary>
    /// 获取枚举Schema
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    private OpenApiSchema GetEnumSchema(string enumType) => Schemas.SingleOrDefault(x => x.Key == enumType).Value;

    /// <summary>
    /// 递归获取 Body 示例
    /// </summary>
    /// <param name="key"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private ModelExample? GetExapmple(string key, int level = 1)
    {
        if (string.IsNullOrEmpty(key) || !Schemas.ContainsKey(key)) return null;
        if (level > 2) return null;
        var schema = Schemas.SingleOrDefault(x => x.Key == key).Value;
        if (schema.Properties.Any() == false) return null;
        var exapmle = new ModelExample();
        foreach (var item in schema.Properties)
        {
            if (item.Value == null)
            {
                exapmle.Add(item.Key, null);
                continue;
            }
            else if (item.Value.IsObject(Schemas))
            {
                var objKey = item.Value.Reference.Id;
                if (objKey == key)
                    exapmle.Add(item.Key, null);
                else
                {
                    level++;
                    exapmle.Add(item.Key, GetExapmple(objKey, level));
                }
            }
            else if (item.Value.IsArray())
            {
                if (item.Value.IsBaseTypeArray())
                {
                    level++;
                    var data = GetExapmple(item.Value.Items.Type, level);
                    if (data != null)
                        exapmle.Add(item.Key, new[] { data });
                }
                else
                {
                    level++;
                    var data = GetExapmple(item.Value.Items.Reference.Id, level);
                    if (data != null)
                        exapmle.Add(item.Key, new[] { data });
                }
            }
            else
            {
                if (item.Value.IsEnum(Schemas))
                    exapmle.Add(item.Key, GetEnumValues(item.Value.Reference.Id).Min());
                else
                    exapmle.Add(item.Key, GetDefaultValue(item.Value.Format ?? item.Value.Type));
            }
        }
        return exapmle;
    }


    /// <summary>
    /// 获取 Body 参数说明，递归扁平化输出所有属性到同一个markdown表格
    /// </summary>
    /// <param name="apiSchema">OpenApiSchema对象</param>
    /// <param name="func">递归获取属性信息的委托</param>
    /// <returns>markdown表格字符串</returns>
    private string GetMarkDownForModelInfo(OpenApiSchema apiSchema, Func<string, object> func)
    {
        var markdown = new StringPlus();
        void AppendRows(object info, string parent = "", string position = "Body")
        {
            if (info == null)
            {
                return;
            }
            if (parent.IsNullOrEmpty()) parent = "-";
            markdown.AppendLine("|参数名称|参数类型|参数位置|是否可空|描述|");
            markdown.AppendLine("|:----:|:----:|:----:|:----:|:----:|");
            // 处理枚举
            if (info is Models.EnumInfo enumInfo)
            {
                markdown.AppendLine($"|{parent ?? "-"}|enum|{position}|{enumInfo.枚举描述 ?? "-"}|-");
                return;
            }
            // 处理属性字典
            if (info is IDictionary<string, object> dict)
            {
                foreach (var kv in dict)
                {
                    var name = (string.IsNullOrEmpty(parent) || parent == "-") ? kv.Key : $"{parent}.{kv.Key}";
                    if (name.IsNullOrEmpty()) name = "-";
                    if (kv.Value is RequestModelInfo req)
                    {
                        // 对象类型递归
                        if (req.参数类型 is IDictionary<string, object> subDict)
                        {
                            AppendRows(subDict, name, position);
                        }
                        else
                        {
                            var argType = req.参数类型;
                            if (argType == null)
                            {
                                argType = "-";
                            }
                            if (argType is Models.EnumInfo argEnumInfo)
                            {
                                markdown.AppendLine($"|{name ?? "-"}|enum|{position}|{(req.是否必传 ? "否" : "是")}|{req.描述 ?? "-"}|");
                            }
                            else
                            {
                                markdown.AppendLine($"|{name ?? "-"}|{req.参数类型 ?? "-"}|{position}|{(req.是否必传 ? "否" : "是")}|{req.描述 ?? "-"}|");
                            }
                        }
                    }
                    else if (kv.Value is ResponseModelInfo resp)
                    {
                        if (resp.参数类型 is IDictionary<string, object> subDict)
                        {
                            AppendRows(subDict, name, position);
                        }
                        else
                        {
                            var argType = resp.参数类型;
                            if (argType == null)
                            {
                                argType = "-";
                            }
                            if (argType is Models.EnumInfo argEnumInfo)
                            {
                                markdown.AppendLine($"|{name ?? "-"}|enum|{position}|{(resp.可空类型 ? "否" : "是")}|{resp.描述 ?? "-"}|");
                            }
                            else
                            {
                                markdown.AppendLine($"|{name ?? "-"}|{resp.参数类型 ?? "-"}|{position}|{(resp.可空类型 ? "是" : "否")}|{resp.描述 ?? "-"}|");
                            }
                        }
                    }
                    else if (kv.Value is IDictionary<string, object> subDict2)
                    {
                        AppendRows(subDict2, name, position);
                    }
                }
                return;
            }

            // 处理数组
            if (info is IEnumerable<object> list && !(info is string))
            {
                foreach (var item in list)
                {
                    AppendRows(item, parent, position);
                }
                return;
            }

            markdown.AppendLine($"|{parent ?? "-"}|{apiSchema.Type}|{position}|{(apiSchema.Nullable ? "是" : "否")}|{apiSchema.Description ?? "-"}|");
            return;
        }
        // 获取根对象信息
        object? rootInfo = null;
        var key = "";
        if (apiSchema.IsObject(Schemas) || apiSchema.IsEnum(Schemas))
            key = apiSchema.Reference.Id;
        else if (apiSchema.IsArray())
            key = apiSchema.Items.Type ?? apiSchema.Items.Reference.Id;
        else if (apiSchema.IsBaseType())
            key = apiSchema.Type;
        if (key != null)
            rootInfo = func(key);
        if (rootInfo != null)
            AppendRows(rootInfo);
        return markdown.ToString();
    }
    /// <summary>
    /// 递归获取 Body 参数说明
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isShowRequired"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private object? GetModelInfo(string key, bool isShowRequired = true, int level = 1)
    {
        if (key == null) return null;
        if (level > 2) return null;
        if (key == null || Schemas.ContainsKey(key) == false) return key;
        var schema = Schemas.SingleOrDefault(x => x.Key == key).Value;
        if (schema.Properties.Any() == false)
            return new Models.EnumInfo()
            {
                枚举范围 = GetEnumValues(key),
                枚举描述 = schema.Description,
                枚举类型 = schema.Format,
                枚举名称 = key
            };
        var properties = new Dictionary<string, object>();
        foreach (var item in schema.Properties)
        {
            object? obj = "object";
            if (item.Value.IsObject(Schemas))
            {
                var objKey = item.Value.Reference.Id;
                if (objKey == key)
                    obj = objKey;
                else
                {
                    level++;
                    obj = GetModelInfo(objKey, isShowRequired, level);
                }

            }
            else if (item.Value.IsArray())
            {
                var arrayKey = "";
                if (item.Value.IsBaseTypeArray())
                    arrayKey = item.Value.Items.Type;
                else
                    arrayKey = item.Value.Items.Reference.Id;
                level++;
                obj = new[] { GetModelInfo(arrayKey, isShowRequired, level) };
            }
            else if (item.Value.IsEnum(Schemas))
            {
                var enumKey = item.Value.Reference.Id;
                var enumObj = GetEnumSchema(enumKey);
                obj = new Models.EnumInfo()
                {
                    枚举范围 = GetEnumValues(enumKey),
                    枚举类型 = enumObj.Format,
                    枚举名称 = enumKey,
                    枚举描述 = enumObj.Description
                };
            }
            else
            {
                obj = item.Value.Format ?? item.Value.Type;
            }

            if (isShowRequired)
            {
                var requestModelInfo = new RequestModelInfo
                {
                    参数类型 = obj ?? "",
                    描述 = item.Value.Description,
                    是否必传 = schema.Required.Any(x => x == item.Key),
                    可空类型 = item.Value.Nullable
                };
                properties.Add(item.Key, requestModelInfo);
            }
            else
            {
                var responseModelInfo = new ResponseModelInfo
                {
                    参数类型 = obj ?? "",
                    描述 = item.Value.Description,
                    可空类型 = item.Value.Nullable
                };
                properties.Add(item.Key, responseModelInfo);
            }
        }
        return properties;
    }
    /// <summary>
    /// 获取类型默认值
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private object? GetDefaultValue(string type)
    {
        var number = new string[] { "byte", "decimal", "double", "enum", "float", "int32", "int64", "sbyte", "short", "uint", "ulong", "ushort" };
        if (number.Any(x => type == x)) return 0;
        if (type == "string") return "string";
        if (type == "bool" || type == "boolean") return false;
        if (type == "date-time") return DateTime.Now;
        return null;
    }

    /// <summary>
    /// 获取 MarkDown 文件流
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte[] GetSwaggerDocContent(string name)
    {
        // 检查缓存
        if (_docContentCache.TryGetValue(name, out byte[] cachedContent))
        {
            return cachedContent;
        }
        
        var content = GetSwaggerDoc(name);
        var bytes = content.ToBytes();
        
        // 存入缓存
        _docContentCache[name] = bytes;
        
        return bytes;
    }

    /// <summary>
    /// 生成api的js sdk,
    /// 解析 Swagger JSON 生成javascript Fetch 代码
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte[] GetSwaggerJsSdk(string name)
    {
        // 检查缓存
        if (_sdkCache.TryGetValue(name, out byte[] cachedSdk))
        {
            return cachedSdk;
        }
        
        // 获取swagger文档
        var document = _generator.GetSwagger(name);
        if (document == null)
            throw new Exception("document is null !");
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// Auto-generated JS SDK by LuBan-Framework-Generator");
        sb.AppendLine("// 使用方法: let apiSdk = new ApiSdk('http://xxx.xx.xxx', 'jwt'); apiSdk.方法名(params, options)");
        sb.AppendLine($"class {name}ApiSdk {{");
        sb.AppendLine("    constructor(baseUrl, jwt) {");
        sb.AppendLine("        this.baseUrl = baseUrl;");
        sb.AppendLine("        this.jwt = jwt;");
        sb.AppendLine("    }");
        foreach (var path in document.Paths)
        {
            foreach (var operationItem in path.Value.Operations)
            {
                var operation = operationItem.Value;
                var method = operationItem.Key.ToString().ToUpper();
                // 方法名小写加下划线
                string funcNameRaw = operation.OperationId ?? ($"{method}_{path.Key.Replace("/", "_").Replace("{", "").Replace("}", "")}");
                var funcName = string.Join("_", funcNameRaw
                    .Replace("-", "_")
                    .Replace("/", "_")
                    .Replace("{", "_")
                    .Replace("}", "_")
                    .Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.ToLower()));
                var url = path.Key;
                // 检查content-type
                string contentType = "application/json";
                string responseType = "json";
                if (operation.RequestBody != null && operation.RequestBody.Content != null && operation.RequestBody.Content.Keys.Any())
                {
                    contentType = operation.RequestBody.Content.Keys.First();
                }
                if (operation.Responses != null && operation.Responses.ContainsKey("200"))
                {
                    var resp = operation.Responses["200"];
                    if (resp.Content != null && resp.Content.Keys.Any())
                    {
                        var respType = resp.Content.Keys.First();
                        if (respType.Contains("stream"))
                        {
                            responseType = "blob";
                        }
                    }
                }
                // 修正C#字符串插值中url模板的}转义
                var jsUrl = url.Replace("{", "${{params.").Replace("}", "}}");
                //var jwtExpr = "${{this.jwt}}";
                sb.AppendLine($"    /**");
                sb.AppendLine($"     * {operation.Summary ?? url}");
                sb.AppendLine($"     * @url {url}");
                sb.AppendLine($"     * @method {method}");
                sb.AppendLine($"     * @content-type {contentType}");
                sb.AppendLine($"     * @response-type {responseType}");
                sb.AppendLine($"     */");
                sb.AppendLine($"    async {funcName}(params, options) {{");
                sb.AppendLine($"        try {{");
                sb.AppendLine($"            const response = await fetch(this.baseUrl + `{jsUrl}` , {{");
                sb.AppendLine($"                method: '{method}',");
                sb.AppendLine($"                headers: Object.assign({{ 'Content-Type': '{contentType}', 'Authorization': 'Bearer ' + this.jwt }}, options && options.headers),");
                if (method == "GET")
                {
                    sb.AppendLine($"                // GET请求参数拼接到URL");
                }
                else
                {
                    sb.AppendLine($"                body: JSON.stringify(params),");
                }
                sb.AppendLine($"                ...options");
                sb.AppendLine($"            }});");
                sb.AppendLine($"            if (!response.ok) throw new Error('网络请求失败: ' + response.status);");
                if (responseType == "blob")
                {
                    sb.AppendLine($"            return await response.blob();");
                }
                else
                {
                    sb.AppendLine($"            const data = await response.json();");
                    sb.AppendLine($"            if (typeof data === 'object' && data !== null && 'code' in data) {{");
                    sb.AppendLine($"                if (data.code !== 200) throw new Error(data.message || '业务异常');");
                    sb.AppendLine($"                return data.result;");
                    sb.AppendLine($"            }}");
                    sb.AppendLine($"            return data;");
                }
                sb.AppendLine($"        }} catch (error) {{");
                sb.AppendLine($"            if (error instanceof TypeError || (error.message && error.message.toLowerCase().includes('network'))) {{");
                sb.AppendLine($"                throw new Error('网络异常: ' + (error.message || error));");
                sb.AppendLine($"            }} else {{");
                sb.AppendLine($"                throw error;");
                sb.AppendLine($"            }}");
                sb.AppendLine($"        }}");
                sb.AppendLine($"    }}");
            }
        }
        sb.AppendLine("}");
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        
        // 存入缓存
        _sdkCache[name] = bytes;
        
        return bytes;
    }
}
