/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Http
*文件名： HttpClientUtil
*版本号： V1.0.0.0
*唯一标识：766250de-3a73-4f99-b810-67109ae9ee9f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/1 13:45:15
*描述：http请求工具类
*
*=================================================
*修改标记
*修改时间：2025/9/1 13:45:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：http请求工具类
*
*****************************************************************************/
using LuBan.Common.Http;

namespace LuBan.Common;


/// <summary>
/// http请求工具类
/// </summary>
public sealed class HttpClientProxy
{
    #region private

    HttpClient _httpClient;

    static ConcurrentDictionary<string, HttpClientProxy> _httpClients;

    bool _useLog = false;

    /// <summary>
    /// HttpClientHelper
    /// </summary>
    static HttpClientProxy()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
            | SecurityProtocolType.Tls11
            | SecurityProtocolType.Tls12;

        _httpClients = new ConcurrentDictionary<string, HttpClientProxy>();
    }


    void FillHeaders(HttpRequestMessage reqMsg, Dictionary<string, string>? headers = null, bool withContentType = true)
    {
        if (headers != null && headers.Any())
        {
            if (withContentType)
            {
                foreach (var item in headers)
                {
                    if (item.Key.IndexOf("Content-Type", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        //传入类型
                        if (!string.IsNullOrEmpty(item.Value) && reqMsg.Content != null)
                        {
                            var encodingArr = item.Value.Split(";", StringSplitOptions.RemoveEmptyEntries);
                            if (encodingArr.Length > 1)
                            {
                                var encoding = encodingArr[1].Split("=")[1];
                                if (reqMsg.Content != null)
                                {
                                    reqMsg.Content.Headers.ContentType = new MediaTypeHeaderValue(encodingArr[0]);
                                    reqMsg.Content.Headers.ContentType.CharSet = encoding;
                                }
                            }
                            else
                            {
                                reqMsg.Content.Headers.ContentType = new MediaTypeHeaderValue(encodingArr[0]);
                            }
                        }
                    }
                    else if (item.Key.IndexOf("Authorization", StringComparison.OrdinalIgnoreCase) > -1 && !string.IsNullOrEmpty(item.Value) && item.Value.IndexOf(" ") > -1)
                    {
                        //basic Bearer 等标准较验
                        var authArr = item.Value.Split(" ");
                        if (authArr.Length == 2)
                        {
                            reqMsg.Headers.Authorization = new AuthenticationHeaderValue(authArr[0], authArr[1]);
                        }
                    }
                    else
                    {
                        //自定义headers
                        reqMsg.Headers.Add(item.Key, item.Value);
                    }
                }
            }
            else
            {
                foreach (var item in headers)
                {
                    if (item.Key.IndexOf("Content-Type", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        continue;
                    }
                    if (item.Key.IndexOf("Authorization", StringComparison.OrdinalIgnoreCase) > -1 && !string.IsNullOrEmpty(item.Value) && item.Value.IndexOf(" ") > -1)
                    {
                        //basic Bearer 等标准较验
                        var authArr = item.Value.Split(" ");
                        if (authArr.Length == 2)
                        {
                            reqMsg.Headers.Authorization = new AuthenticationHeaderValue(authArr[0], authArr[1]);
                        }
                    }
                    else
                    {
                        //自定义headers
                        reqMsg.Headers.Add(item.Key, item.Value);
                    }
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// http请求代理类
    /// </summary>
    /// <param name="baseUri"></param>
    /// <param name="timeout"></param>
    /// <param name="version"></param>
    /// <param name="cookiescontainer"></param>
    /// <param name="webProxy"></param>
    /// <param name="useLog"></param>
    public HttpClientProxy(Uri baseUri,
        int timeout = 180,
        string version = "1.1",
        CookieContainer? cookiescontainer = null,
        WebProxy? webProxy = null,
        bool useLog = true)
    {
        HttpClientHandler handler;

        if (cookiescontainer != null)
        {
            handler = new HttpClientHandler() { CookieContainer = cookiescontainer, AllowAutoRedirect = true, UseCookies = true };
        }
        else
        {
            handler = new HttpClientHandler() { UseCookies = false }; //手动header里面添加cookie
        }
        if (webProxy != null)
        {
            handler.UseProxy = true;
            handler.Proxy = webProxy;
        }

        handler.ServerCertificateCustomValidationCallback = (msg, cer, chain, sslPolic) => true;

        _httpClient = new HttpClient(handler);

        switch (version)
        {
            case "1.0":
                _httpClient.DefaultRequestVersion = new Version(1, 0);
                break;
            case "2.0":
                _httpClient.DefaultRequestVersion = new Version(2, 0);
                break;
            case "3.0":
                _httpClient.DefaultRequestVersion = new Version(3, 0);
                break;
            default:
            case "1.1":
                _httpClient.DefaultRequestVersion = new Version(1, 1);
                break;
        }
        _httpClient.BaseAddress = baseUri;
        _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        _useLog = useLog;
    }

    #region Create


    /// <summary>
    /// 创建http请求代理类
    /// </summary>
    /// <param name="baseUri"></param>
    /// <param name="timeout"></param>
    /// <param name="version"></param>
    /// <param name="cookiescontainer"></param>
    /// <param name="webProxy"></param>
    /// <param name="useLog"></param>
    /// <returns></returns>
    public static HttpClientProxy Create(Uri baseUri,
        int timeout = 180,
        string version = "1.1",
        CookieContainer? cookiescontainer = null,
        WebProxy? webProxy = null,
        bool useLog = true)
    {
        return _httpClients.GetOrAdd(baseUri.ToString().ToLower(), (k) => new HttpClientProxy(baseUri, timeout, version, cookiescontainer, webProxy, useLog));
    }


    /// <summary>
    /// 创建http请求代理类
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="timeout"></param>
    /// <param name="version"></param>
    /// <param name="cookiescontainer"></param>
    /// <param name="webProxy"></param>
    /// <param name="useLog"></param>
    /// <returns></returns>
    public static HttpClientProxy Create(string baseUrl,
        int timeout = 180,
        string version = "1.1",
        CookieContainer? cookiescontainer = null,
        WebProxy? webProxy = null,
        bool useLog = true)
    {
        return Create(new Uri(baseUrl), timeout, version, cookiescontainer, webProxy, useLog);
    }


    #endregion


    /// <summary>
    /// 基础的http请求
    /// </summary>
    /// <param name="httpRequestMessage"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public HttpResponseMessage Send(HttpRequestMessage httpRequestMessage, int timeout = 180)
    {
        using CancellationTokenSource cts = new(timeout * 1000);

        var resMsg = _httpClient.Send(httpRequestMessage, cts.Token);

        resMsg.EnsureSuccessStatusCode();

        return resMsg;
    }


    /// <summary>
    /// 基础的http请求
    /// </summary>
    /// <param name="httpRequestMessage"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, int timeout = 180)
    {
        using CancellationTokenSource cts = new(timeout * 1000);

        var resMsg = await _httpClient.SendAsync(httpRequestMessage, cts.Token);

        resMsg.EnsureSuccessStatusCode();

        return resMsg;
    }

    /// <summary>
    /// get请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public byte[] GetBytes(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        byte[] bytes = [];

        try
        {
            var reqMsg = new HttpRequestMessage(new HttpMethod("GET"), resource);

            FillHeaders(reqMsg, headers);

            using var resMsg = Send(reqMsg, timeout);

            using (var stream = resMsg.Content.ReadAsStream())
            {
                bytes = stream.ToBytes() ?? throw new NetworkInformationException();
            }

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    if (bytes != null)
                    {
                        bytes = ZipUtil.Decompress(bytes) ?? [];
                    }
                }
            }


        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString().RemoveLast() ?? "" + resource,
            RequestMethod = "GET",
            Header = (headers == null ? "" : headers.ToJson()),
            Input = string.Empty,
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = bytes?.ToNoTitleString() ?? "";

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return bytes ?? [];
    }


    /// <summary>
    /// get请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<byte[]> GetBytesAsync(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        byte[] bytes = [];

        try
        {
            var reqMsg = new HttpRequestMessage(new HttpMethod("GET"), resource);

            FillHeaders(reqMsg, headers);

            using var resMsg = await SendAsync(reqMsg, timeout);

            bytes = await resMsg.Content.ReadAsByteArrayAsync();

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    if (bytes != null)
                    {
                        bytes = ZipUtil.Decompress(bytes) ?? [];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString().RemoveLast() ?? "" + resource,
            RequestMethod = "GET",
            Header = (headers == null ? "" : headers.ToJson()),
            Input = string.Empty,
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = bytes?.ToNoTitleString() ?? "";

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return bytes ?? [];
    }

    /// <summary>
    /// get请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public string GetJson(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        if (headers == null)
            headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!headers.ContainsKey("Content-Type"))
            headers.Add("Content-Type", "application/json; charset=utf-8");

        var bytes = GetBytes(resource, headers, timeout);

        return bytes?.ToNoTitleString() ?? "";
    }

    /// <summary>
    /// get请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> GetAsync(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        if (headers == null)
            headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!headers.ContainsKey("Content-Type"))
            headers.Add("Content-Type", "application/json; charset=utf-8");

        var bytes = await GetBytesAsync(resource, headers, timeout);

        return bytes?.ToNoTitleString() ?? "";
    }

    /// <summary>
    /// get请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public T GetViewModel<T>(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var json = GetJson(resource, headers, timeout);
        if (json.IsNullOrEmpty()) throw new Exception("接口返回值为空");
        var model = json.ToObject<T>();
        if (model == null) throw new Exception($"接口返回model为空,json:{json}");
        return model;
    }

    /// <summary>
    /// get请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T> GetViewModelAsync<T>(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var json = await GetAsync(resource, headers, timeout);
        if (json.IsNullOrEmpty()) throw new Exception("接口返回值为空");
        var model = json.ToObject<T>();
        if (model == null) throw new Exception($"接口返回model为空,json:{json}");
        return model;
    }


    /// <summary>
    /// 获取请求的模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public T Get<T>(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return GetViewModel<T>(resource, headers, timeout);
    }

    /// <summary>
    /// 获取请求的模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return await GetViewModelAsync<T>(resource, headers, timeout);
    }

    /// <summary>
    /// 提交form 表单，
    /// application/x-www-form-urlencoded
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="formData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<byte[]> PostFormForParamsAsync(string resource, Dictionary<string, string> formData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        string result = "";

        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        byte[] bytes = [];

        string input = string.Empty;

        try
        {
            var reqMsg = new HttpRequestMessage(new HttpMethod("POST"), resource);

            var formStr = input = string.Join('&', formData.Select(kv => $"{kv.Key}={kv.Value.UrlEncode()}"));

            var formCtx = new StringContent(formStr, Encoding.UTF8, "application/x-www-form-urlencoded");

            reqMsg.Content = formCtx;

            FillHeaders(reqMsg, headers, false);

            using var resMsg = await SendAsync(reqMsg, timeout);

            bytes = await resMsg.Content.ReadAsByteArrayAsync();

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    if (bytes != null)
                    {
                        bytes = ZipUtil.Decompress(bytes) ?? [];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString() ?? "".RemoveLast() + resource,
            RequestMethod = "POST",
            Header = (headers == null ? "" : headers.ToJson()),
            Input = input,
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = result;

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return bytes ?? [];
    }

    /// <summary>
    /// post表单数据，
    /// multipart/form-data
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="formData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<byte[]> PostFormAsync(string resource, Dictionary<string, string> formData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        byte[] bytes = [];

        string input = string.Empty;

        try
        {
            var reqMsg = new HttpRequestMessage(new HttpMethod("POST"), resource);

            if (headers != null && headers.ContainsKey("Content-Type") && headers["Content-Type"] == "multipart/form-data")
            {
                string boundary = "----WebKitFormBoundary" + DateTimeUtil.Now.Ticks.ToString("x");
                var form = new MultipartFormDataContent(boundary);
                foreach (var item in formData)
                {
                    form.Add(new StringContent(item.Value.ToString()), item.Key);
                }
                input = formData.ToJson();
                reqMsg.Content = form;
            }
            else
            {
                reqMsg.Content = new FormUrlEncodedContent(formData.ToList());
            }

            FillHeaders(reqMsg, headers, false);

            using var resMsg = await SendAsync(reqMsg, timeout);

            bytes = await resMsg.Content.ReadAsByteArrayAsync();

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    if (bytes != null)
                    {
                        bytes = ZipUtil.Decompress(bytes) ?? [];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString() ?? "".RemoveLast() + resource,
            RequestMethod = "Post",
            Header = (headers == null ? "" : headers.ToJson()),
            Input = input,
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = bytes?.ToNoTitleString() ?? "";

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return bytes ?? [];
    }

    /// <summary>
    /// post文件
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="formData"></param>
    /// <param name="fileData">文件全路径名，文件名</param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> PostFileAsync(string resource, Dictionary<string, string> formData, List<string> fileData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        string boundary = $"----WebKitFormBoundary{DateTime.Now.Ticks.ToString("x")}";

        var form = new MultipartFormDataContent(boundary);

        foreach (var item in formData)
        {
            form.Add(new StringContent(item.Value.ToString()), item.Key);
        }

        foreach (var filePath in fileData)
        {
            form.Add(new ByteArrayContent(FileUtil.Read(filePath)), FileTypeUtil.GetHttpContentType(filePath), Path.GetFileName(filePath));
        }

        var reqMsg = new HttpRequestMessage(new HttpMethod("POST"), resource);

        reqMsg.Content = form;

        FillHeaders(reqMsg, headers, false);

        using var resMsg = await SendAsync(reqMsg, timeout);

        return await resMsg.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// post请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<byte[]> PostBytesAsync(string resource, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        string input = string.Empty;

        byte[] bytes = [];

        try
        {
            if (postData == null) postData = [];

            input = postData.ToNoTitleString() ?? "";

            var form = new ByteArrayContent(postData);

            var reqMsg = new HttpRequestMessage(new HttpMethod("POST"), resource);

            reqMsg.Content = form;

            FillHeaders(reqMsg, headers);

            using var resMsg = await SendAsync(reqMsg, timeout);

            bytes = await resMsg.Content.ReadAsByteArrayAsync();

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    if (bytes != null)
                    {
                        return ZipUtil.Decompress(bytes) ?? [];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString() ?? "".RemoveLast() + resource,
            RequestMethod = "Post",
            Header = (headers == null ? "" : headers.ToJson()),
            Input = input,
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = bytes?.ToNoTitleString() ?? "";

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null)
            throw exception;

        return bytes ?? [];
    }

    //post微信消息回调消息流，并返回字符串


    /// <summary>
    /// post请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T?> PostAsync<T>(string resource, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var data = await PostBytesAsync(resource, postData, headers, timeout);
        if (data == null || data.Length < 1) return default;
        var json = data.ToStr();
        return json.ToObject<T>();
    }

    /// <summary>
    /// 传入Json
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public string Post(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        string result = "";

        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        try
        {
            var reqMsg = new HttpRequestMessage(new HttpMethod("POST"), resource);

            if (!string.IsNullOrEmpty(json))
            {
                var stringContent = new StringContent(json);

                if (headers == null)
                    headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (!headers.ContainsKey("Content-Type"))
                {
                    headers.Add("Content-Type", "application/json; charset=utf-8");
                    stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                }

                reqMsg.Content = stringContent;
            }

            FillHeaders(reqMsg, headers);

            using var resMsg = Send(reqMsg, timeout);

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    var bytes = resMsg.Content.ReadAsStream().ToBytes();

                    if (bytes != null)
                    {
                        var zbytes = ZipUtil.Decompress(bytes) ?? [];
                        return Encoding.UTF8.GetString(zbytes);
                    }
                }
            }

            result = resMsg.Content.ReadAsStream().ReadToEnd();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString().RemoveLast() + resource,
            RequestMethod = "POST",
            Header = (headers == null ? "" : headers.ToJson()),
            Input = json,
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = result;

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return result;
    }

    /// <summary>
    /// 传入Json
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> PostAsync(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        string result = "";

        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        try
        {
            var reqMsg = new HttpRequestMessage(new HttpMethod("POST"), resource);

            if (!string.IsNullOrEmpty(json))
            {
                var stringContent = new StringContent(json);

                if (headers == null)
                    headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (!headers.ContainsKey("Content-Type"))
                {
                    headers.Add("Content-Type", "application/json; charset=utf-8");
                    stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                }

                reqMsg.Content = stringContent;
            }

            FillHeaders(reqMsg, headers);

            using var resMsg = await SendAsync(reqMsg, timeout);

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    var bytes = await resMsg.Content.ReadAsByteArrayAsync();

                    if (bytes != null)
                    {
                        var zbytes = ZipUtil.Decompress(bytes) ?? [];
                        return Encoding.UTF8.GetString(zbytes);
                    }
                }
            }

            result = await resMsg.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString() ?? "".RemoveLast() + resource,
            RequestMethod = "POST",
            Header = (headers == null ? "" : headers.ToJson()),
            Input = json,
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = result;

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return result;
    }

    /// <summary>
    /// 传入Json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public T PostJson<T>(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var result = Post(resource, json, headers, timeout);
        if (result.IsNullOrEmpty()) throw new Exception("接口返回的值为空");
        var model = result.ToObject<T>();
        if (model == null) throw new Exception($"接口返回的model为空，json:{result}");
        return model;
    }


    /// <summary>
    /// 传入Json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T> PostJsonAsync<T>(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var result = await PostAsync(resource, json, headers, timeout);
        if (result.IsNullOrEmpty()) throw new Exception("接口返回的值为空");
        var model = result.ToObject<T>();
        if (model == null) throw new Exception($"接口返回的model为空，json:{result}");
        return model;
    }

    /// <summary>
    /// post请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="postModel"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T> PostViewModelAsync<T>(string resource, object postModel, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        string? postData = null;
        if (postModel != null)
        {
            if (postModel is string)
            {
                postData = postModel.ToString();
            }
            else
            {
                postData = postModel.ToJson(false, true);
            }
        }

        if (headers == null)
            headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var json = await PostAsync(resource, postData ?? "", headers, timeout);

        if (string.IsNullOrEmpty(json)) throw new Exception("接口返回的json为空");

        var model = json.ToObject<T>();
        if (model == null) throw new Exception($"接口返回的model为空，json:{json}");

        return model;
    }

    /// <summary>
    /// 请求post
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="postModel"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T> PostAsync<T>(string resource, object postModel, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return await PostViewModelAsync<T>(resource, postModel, headers, timeout);
    }

    /// <summary>
    /// 下载文件,get
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="fileName"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task DownLoadFileAsync(string resource, string fileName, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var buffer = await GetBytesAsync(resource, headers, timeout);
        using var locker = await LockerBuilder.Default.CreateAsync($"HttpClientUtil.DownLoadFileAsync:{fileName}");
        await FileUtil.WriteAsync(fileName, buffer);
    }

    /// <summary>
    /// 下载文件,post
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="fileName"></param>
    /// <param name="formData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task DownLoadFileAsync(string resource, string fileName, Dictionary<string, string> formData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var buffer = await PostFormAsync(resource, formData, headers, timeout);
        using var locker = await LockerBuilder.Default.CreateAsync($"HttpClientUtil.DownLoadFileAsync:{fileName}");
        await FileUtil.WriteAsync(fileName, buffer);
    }

    /// <summary>
    /// 下载并获取下载文件目录
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> DownLoadZipFileAsync(string resource, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var fullName = PathUtil.GetFileName($"{GuidUtil.GuidString}.zip");
        await DownLoadFileAsync(resource, fullName, headers, timeout);
        var unzipPath = PathUtil.GetRootFilePath("zip");
        ZipUtil.UnZip(fullName, unzipPath);
        FileUtil.Delete(fullName);
        return unzipPath;
    }

    #region restful 

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="resource"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<byte[]> DoAsync(string methodName, string resource, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        string result = "";

        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        byte[] bytes = [];

        try
        {

            var reqMsg = new HttpRequestMessage(new HttpMethod(methodName), resource);

            if (postData != null && postData.Length > 0)
            {
                var form = new ByteArrayContent(postData);

                reqMsg.Content = form;
            }

            FillHeaders(reqMsg, headers);

            using var resMsg = await SendAsync(reqMsg, timeout);

            bytes = await resMsg.Content.ReadAsByteArrayAsync();

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    if (bytes != null)
                    {
                        bytes = ZipUtil.Decompress(bytes) ?? [];
                    }
                }
            }

            result = bytes.ToNoTitleString() ?? "";

        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var input = string.Empty;
        if (postData != null && postData.Length > 0)
        {
            input = postData.ToNoTitleString();
        }

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString() ?? "".RemoveLast() + resource,
            RequestMethod = methodName,
            Header = (headers == null ? "" : headers.ToJson()),
            Input = input ?? "",
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = result;

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return bytes ?? [];
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> DoAsync(string methodName, string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        string result = "";

        Stopwatch stopwatch = Stopwatch.StartNew();

        Exception? exception = null;

        try
        {
            var form = new StringContent(json);

            var reqMsg = new HttpRequestMessage(new HttpMethod(methodName), resource);

            reqMsg.Content = form;

            if (headers == null)
                headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!headers.ContainsKey("Content-Type"))
                headers.Add("Content-Type", "application/json; charset=utf-8");

            FillHeaders(reqMsg, headers);

            using var resMsg = await SendAsync(reqMsg, timeout);

            var bytes = await resMsg.Content.ReadAsByteArrayAsync();

            if (resMsg.Content.Headers.ContentEncoding != null)
            {
                if (resMsg.Content.Headers.ContentEncoding.Count > 0 && resMsg.Content.Headers.ContentEncoding.First() == "gzip")
                {
                    if (bytes != null)
                    {
                        bytes = ZipUtil.Decompress(bytes) ?? [];
                    }
                }
            }

            result = bytes.ToNoTitleString() ?? "";
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString() ?? "".RemoveLast() + resource,
            RequestMethod = methodName,
            Header = (headers == null ? "" : headers.ToJson()),
            Input = json,
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = result;

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return result;
    }


    /// <summary>
    /// 发送SSE请求
    /// </summary>
    /// <param name="methodName">请求方法</param>
    /// <param name="resource">资源路径</param>
    /// <param name="json">请求数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间(秒)</param>
    /// <param name="retryCount">重试次数</param>
    /// <param name="retryDelay">重试延迟(毫秒)</param>
    /// <returns>SSE流</returns>
    public async Task<Stream> CreateSSEAsync(
        string methodName,
        string resource,
        string json,
        Dictionary<string, string>? headers = null,
        int timeout = 180,
        int retryCount = 3,
        int retryDelay = 1000)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Exception? lastException = null;
        int currentRetry = 0;

        while (currentRetry <= retryCount)
        {
            try
            {
                var form = new StringContent(json);

                var reqMsg = new HttpRequestMessage(new HttpMethod(methodName), resource);
                reqMsg.Content = form;

                if (headers == null)
                    headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // 设置 SSE 所需的请求头
                headers.TryAdd("Accept", "text/event-stream");
                headers.TryAdd("Cache-Control", "no-cache");
                headers.TryAdd("Connection", "keep-alive");
                headers.TryAdd("Content-Type", "application/json; charset=utf-8");

                // 添加重试相关的请求头
                if (currentRetry > 0)
                {
                    headers["Last-Event-ID"] = currentRetry.ToString();
                }

                FillHeaders(reqMsg, headers);

                using var resMsg = await SendAsync(reqMsg, timeout);

                // 验证响应类型
                if (!resMsg.Content.Headers.ContentType?.MediaType?.Contains("text/event-stream") ?? true)
                {
                    throw new Exception($"服务器返回了非SSE响应类型: {resMsg.Content.Headers.ContentType?.MediaType}");
                }

                // 返回响应流
                return await resMsg.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {
                lastException = ex;
                currentRetry++;

                if (currentRetry <= retryCount)
                {
                    // 记录重试日志
                    var retryLog = new ApiLogInfo()
                    {
                        CallIp = "",
                        Url = _httpClient.BaseAddress?.ToString() ?? "".RemoveLast() + resource,
                        RequestMethod = methodName,
                        Header = (headers == null ? "" : headers.ToJson()),
                        Input = json,
                        Cost = stopwatch.ElapsedMilliseconds,
                        Created = DateTimeUtil.Now,
                        StatusCode = 500,
                        Exception = ex,
                        Output = $"SSE连接失败，正在进行第{currentRetry}次重试"
                    };

                    if (_useLog) Logger.ApiCallLog(retryLog);

                    // 等待一段时间后重试
                    await Task.Delay(retryDelay * currentRetry);
                }
            }
            finally
            {
                if (currentRetry > retryCount)
                {
                    stopwatch.Stop();

                    var apiLog = new ApiLogInfo()
                    {
                        CallIp = "",
                        Url = _httpClient.BaseAddress?.ToString() ?? "".RemoveLast() + resource,
                        RequestMethod = methodName,
                        Header = (headers == null ? "" : headers.ToJson()),
                        Input = json,
                        Cost = stopwatch.ElapsedMilliseconds,
                        Created = DateTimeUtil.Now
                    };

                    if (lastException != null)
                    {
                        apiLog.Exception = lastException;
                        apiLog.StatusCode = 500;
                        apiLog.Output = $"SSE连接失败，已重试{retryCount}次";
                    }
                    else
                    {
                        apiLog.StatusCode = 200;
                    }

                    if (_useLog) Logger.ApiCallLog(apiLog);
                }
            }
        }

        throw new Exception($"SSE连接失败，已重试{retryCount}次: {lastException?.Message}");
    }




    #endregion


    /// <summary>
    /// delete
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<byte[]> DeleteAsync(string resource, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return await DoAsync("Delete", resource, postData, headers, timeout);
    }

    /// <summary>
    /// Delete
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> DeleteJsonAsync(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return await DoAsync("Delete", resource, json, headers, timeout);
    }

    /// <summary>
    /// Delete
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T?> DeleteJsonAsync<T>(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var result = await DeleteJsonAsync(resource, json, headers, timeout);
        return result.ToObject<T>();

    }

    /// <summary>
    /// Delete
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="model"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T?> DeleteAsync<T>(string resource, object model, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        if (headers == null)
            headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var json = await DeleteJsonAsync(resource, model.ToJson() ?? "", headers, timeout);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }
        return json.ToObject<T>();
    }


    /// <summary>
    /// put 请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<byte[]> PutAsync(string resource, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return await DoAsync("Put", resource, postData, headers, timeout);
    }

    /// <summary>
    /// 传入Json
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> PutJsonAsync(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return await DoAsync("Put", resource, json, headers, timeout);
    }

    /// <summary>
    /// Put 请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="postModel"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T?> PutAsync<T>(string resource, object postModel, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        if (headers == null)
            headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var json = await PutJsonAsync(resource, postModel.ToJson() ?? "", headers, timeout);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }
        return json.ToObject<T>();
    }


    /// <summary>
    /// Patch 请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<byte[]> PatchAsync(string resource, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return await DoAsync("Patch", resource, postData, headers, timeout);
    }

    /// <summary>
    /// Patch 传入Json
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> PatchJsonAsync(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        return await DoAsync("Patch", resource, json, headers, timeout);
    }

    /// <summary>
    /// Patch 传入Json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T?> PatchJsonAsync<T>(string resource, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var result = await PatchJsonAsync(resource, json, headers, timeout);
        return result.ToObject<T>();
    }

    /// <summary>
    /// Patch 请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="postModel"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T?> PatchAsync<T>(string resource, object postModel, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        if (headers == null)
            headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var json = await PatchJsonAsync(resource, postModel.ToJson() ?? "", headers, timeout);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }
        return json.ToObject<T>();
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="url"></param>
    /// <param name="localFullName"></param>
    public static void Download(string url, string localFullName)
    {
        using var locker = LockerBuilder.Default.Create($"HttpClientUtil.Download_{localFullName}");
        FileUtil.GetDirecotry(localFullName);
        using WebClient webClient = new();
        webClient.DownloadFile(url, localFullName);
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string Download(string url)
    {
        var downloadPath = Path.Combine(PathUtil.CurrentPath, "downloads", DateTime.Now.ToString("yyyyMMdd"));
        var fileName = Path.GetFileName(url);
        if (fileName.IsNullOrEmpty()) return string.Empty;
        var localFullName = Path.Combine(downloadPath, fileName);
        Download(url, localFullName);
        return localFullName;
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static byte[] DownloadBytes(string url)
    {
        using WebClient webClient = new();
        return webClient.DownloadData(url);
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Stream? DownloadStream(string url)
    {
        var bytes = DownloadBytes(url);
        if (bytes == null || bytes.Length < 1) return null;
        var ms = new MemoryStream(bytes);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="headers"></param>
    /// <param name="formData"></param>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T?> UploadFileAsync<T>(string resource,
        Dictionary<string, string> headers,
        Dictionary<string, object> formData,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            if (formData == null)
            {
                formData = [];
            }
            formData.Add("file", new FileParameter(File.ReadAllBytes(filePath), Path.GetFileName(filePath), "application/octet-stream"));
        }
        return await FormUpload.MultipartFormPostAsync<T>($"{_httpClient.BaseAddress}{resource}", headers, formData, cancellationToken);
    }

    #region 微信回调请求

    /// <summary>
    /// post微信消息回调消息流，并返回字符串
    /// </summary>
    /// <param name="resource">资源路径</param>
    /// <param name="xmlDataStream">XML格式的消息数据</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间(秒)</param>
    /// <returns>响应字符串</returns>
    public async Task<string> PostWechatCallbackAsync(string resource, Stream xmlDataStream, int timeout = 10)
    {
        string result = "";

        Stopwatch stopwatch = Stopwatch.StartNew();
        Exception? exception = null; // 修复问题 5

        try
        {
            var reqMsg = new HttpRequestMessage(new HttpMethod("POST"), resource);

            if (xmlDataStream != null) // 修复问题 1
            {
                if (xmlDataStream.CanRead && xmlDataStream.CanSeek)
                {
                    xmlDataStream.Seek(0, SeekOrigin.Begin); // 确保流的起始位置是正确的
                }
                using var reader = new StreamReader(xmlDataStream, leaveOpen: true);
                var xmlContent = await reader.ReadToEndAsync(); // 从流中读取内容
                var stringContent = new StringContent(xmlContent, Encoding.UTF8, "application/xml"); // 修复问题 2 和问题 3

                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Content-Type", "application/xml; charset=utf-8" }
                };

                reqMsg.Content = stringContent;

                FillHeaders(reqMsg, headers);
            }

            using var resMsg = await SendAsync(reqMsg, timeout);

            result = await resMsg.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString().RemoveLast() + resource, // 修复问题 6
            RequestMethod = "POST",
            Header = "",
            Input = xmlDataStream != null ? "[Stream Content]" : "", // 修复问题 4
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = result;

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return result;
    }


    /// <summary>
    /// 测试微信回调 GET 请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="signature"></param>
    /// <param name="timestamp"></param>
    /// <param name="nonce"></param>
    /// <param name="echostr"></param>
    /// <param name="token"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string> GetWechatCallbackAsync(string resource, string signature, string timestamp, string nonce, string echostr, string token, int timeout = 10)
    {
        string result = "";

        Stopwatch stopwatch = Stopwatch.StartNew();
        Exception? exception = null;
        var queryParams = new Dictionary<string, string>
        {
            { "signature", signature },
            { "timestamp", timestamp },
            { "nonce", nonce },
            { "echostr", echostr },
            { "token", token }
        };
        try
        {
            var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value.UrlEncode()}"));

            var fullResource = $"{resource}?{queryString}";

            var reqMsg = new HttpRequestMessage(HttpMethod.Get, fullResource);

            using var resMsg = await SendAsync(reqMsg, timeout);

            result = await resMsg.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        stopwatch.Stop();

        var apiLog = new ApiLogInfo()
        {
            CallIp = "",
            Url = _httpClient.BaseAddress?.ToString().RemoveLast() + resource,
            RequestMethod = "GET",
            Header = "",
            Input = queryParams.ToJson(),
            Cost = stopwatch.ElapsedMilliseconds,
            Created = DateTimeUtil.Now
        };

        if (exception != null)
        {
            apiLog.Exception = exception;
            apiLog.StatusCode = 500;
        }
        else
        {
            apiLog.StatusCode = 200;
        }

        apiLog.Output = result;

        if (_useLog) Logger.ApiCallLog(apiLog);

        if (exception != null) throw exception;

        return result;
    }

    #endregion



}
