/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Http
*文件名： FormUpload
*版本号： V1.0.0.0
*唯一标识：c5b558e7-873c-4200-a977-51ce6ad7ac54
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/1 13:44:07
*描述：
*
*=================================================
*修改标记
*修改时间：2025/9/1 13:44:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.Http;

/// <summary>
/// 上传表单
/// </summary>
internal static class FormUpload
{
    private static readonly Encoding encoding = Encoding.UTF8;

    static HttpWebResponse? MultipartFormPost(string postUrl, Dictionary<string, string> headers, Dictionary<string, object> postParameters)
    {
        string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
        string contentType = "multipart/form-data; boundary=" + formDataBoundary;

        byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

        return PostForm(postUrl, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.0.0 Safari/537.36", contentType, formData, headers);
    }

    /// <summary>
    /// 多部分表单提交
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="postUrl"></param>
    /// <param name="headers"></param>
    /// <param name="postParameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T?> MultipartFormPostAsync<T>(string postUrl, Dictionary<string, string> headers, Dictionary<string, object> postParameters,
        CancellationToken cancellationToken = default)
    {
        using var response = MultipartFormPost(postUrl, headers, postParameters);
        if (response == null) return default;
        using var reader = new StreamReader(response.GetResponseStream(), encoding);
        var txt = await reader.ReadToEndAsync(cancellationToken);
        return txt.ToObject<T>();
    }


    private static HttpWebResponse? PostForm(string postUrl,
        string userAgent,
        string contentType,
        byte[] formData,
        Dictionary<string, string> headers)
    {
        if (WebRequest.Create(postUrl) is not HttpWebRequest request)
        {
            throw new NullReferenceException("request is not a http request");
        }

        // Set up the request properties.  
        request.Method = "POST";
        request.ContentType = contentType;
        request.UserAgent = userAgent;
        request.CookieContainer = new CookieContainer();
        request.ContentLength = formData.Length;


        //Add header if needed  
        if (headers != null && headers.Count > 0)
        {
            foreach (var item in headers)
            {
                request.Headers.Add(item.Key, item.Value);
            }
        }

        // Send the form data to the request.  
        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(formData, 0, formData.Length);
            requestStream.Close();
        }

        return request.GetResponse() as HttpWebResponse;
    }

    private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
    {
        Stream formDataStream = new System.IO.MemoryStream();
        bool needsCLRF = false;

        foreach (var param in postParameters)
        {

            if (needsCLRF)
                formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

            needsCLRF = true;

            if (param.Value is FileParameter) // to check if parameter if of file type   
            {
                FileParameter fileToUpload = (FileParameter)param.Value;

                // Add just the first part of this param, since we will write the file data directly to the Stream  
                string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                    boundary,
                    param.Key,
                    fileToUpload.FileName ?? param.Key,
                    fileToUpload.ContentType ?? "application/octet-stream");

                formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                // Write the file data directly to the Stream, rather than serializing it to a string.  
                formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
            }
            else
            {
                string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                    boundary,
                    param.Key,
                    param.Value);
                formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
            }
        }

        // Add the end of the request.  Start with a newline  
        string footer = "\r\n--" + boundary + "--\r\n";
        formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

        // Dump the Stream into a byte[]  
        formDataStream.Position = 0;
        byte[] formData = new byte[formDataStream.Length];
        formDataStream.Read(formData, 0, formData.Length);
        formDataStream.Close();

        return formData;
    }
}
