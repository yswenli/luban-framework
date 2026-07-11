/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： DownloadFileUtil
*版本号： V1.0.0.0
*唯一标识：624e2ec3-984e-4e0f-8b64-e99ac4342eef
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/11 13:57:22
*描述：文件下载工具类
*
*=====================================================================
*修改标记
*修改时间：2022/7/11 13:57:22
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：文件下载工具类
*
*****************************************************************************/

namespace LuBan.Web.Core.Utils
{
    /// <summary>
    /// 文件下载工具类
    /// </summary>
    public static class DownloadFileUtil
    {
        /// <summary>
        /// aspnetcore 下载文件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static IActionResult Download(this Stream stream, string fileName)
        {
            if (stream == null) return new EmptyResult();
            var actionresult = new FileStreamResult(stream, new MediaTypeHeaderValue(FileTypeUtil.GetHttpContentType(fileName)))
            {
                FileDownloadName = fileName
            };
            return actionresult;
        }

        /// <summary>
        /// aspnetcore 下载文件
        /// </summary>
        /// <param name="filePath">文件全路径</param>
        /// <returns></returns>
        public static IActionResult Download(string filePath)
        {
            if (!FileUtil.Exists(filePath)) return new EmptyResult();
            return new PhysicalFileResult(filePath, new MediaTypeHeaderValue(FileTypeUtil.GetHttpContentType(filePath)));
        }

        /// <summary>
        /// aspnetcore 下载文件
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IActionResult Download(string txt, string fileName)
        {
            try
            {
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(txt));
                return ms.Download(fileName);
            }
            catch { }
            return new EmptyResult();
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="env">aspnetcore controller中的环境变量</param>
        /// <param name="fileName">指定配置目录中的静态文件名称</param>
        /// <returns></returns>
        public static IActionResult Download(this IWebHostEnvironment env, string fileName)
        {
            var path = Path.Combine(env.ContentRootPath, "Download");
            PathUtil.Create(path);
            var filePath = Path.Combine(path, fileName);
            
            // 路径穿越检查
            var fullPath = Path.GetFullPath(filePath);
            var basePath = Path.GetFullPath(path);
            if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return new BadRequestObjectResult("Invalid file path");
            }
            
            return Download(filePath);
        }

        /// <summary>
        /// mvc下载文件
        /// </summary>
        /// <param name="csvStr">csv内容</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static IActionResult ExportCsvTextFile(this string csvStr, string fileName)
        {
            if (csvStr != null && csvStr.Any())
            {
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(csvStr));

                return ms.Download(fileName);
            }
            return new EmptyResult();
        }



        /// <summary>
        /// 下载excel或csv文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="env">aspnetcore controller中的环境变量</param>
        /// <param name="data"></param>
        /// <param name="fileName">文件名称</param>
        /// <param name="columnNameList">自定义全列名</param>
        /// <param name="splitStr">自定义csv分隔符</param>
        /// <returns></returns>
        public static IActionResult ExportFile(this IWebHostEnvironment env,
            DataTable data,
            string fileName,
            IEnumerable<string>? columnNameList = null,
            string splitStr = ",")
        {
            if (data != null && data.Rows != null && data.Rows.Count > 0)
            {
                if (!fileName.ContainsExtensionName(".xls"))
                {
                    return CsvUtil.ExportFromDataTable(data, splitStr, columnNameList: columnNameList).Download(fileName);
                }
                else
                {
                    return ExcelUtil.ExportStreamFromDataTable(data, fileName, columnNameList: columnNameList).Download(fileName);
                }
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 下载excel或csv文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="env">aspnetcore controller中的环境变量</param>
        /// <param name="data"></param>
        /// <param name="fileName">文件名称</param>
        /// <param name="columnNameList">自定义全列名</param>
        /// <param name="splitStr">自定义csv分隔符</param>
        /// <param name="namePairs">自定义部分列名</param>
        /// <returns></returns>
        public static IActionResult ExportFile<T>(this IWebHostEnvironment env,
            IEnumerable<T> data,
            string fileName,
            IEnumerable<string>? columnNameList = null,
            string splitStr = ",",
            IEnumerable<NamePair>? namePairs = null) where T : class, new()
        {
            if (data != null && data.Any())
            {
                if (!fileName.ContainsExtensionName(".xls"))
                {
                    return data.ExportStreamFromModels(splitStr, columnNameList, namePairs).Download(fileName);
                }
                else
                {
                    return ExcelUtil.ExportStreamFromModels(data, fileName, columnNameList: columnNameList, namePairs: namePairs).Download(fileName);
                }
            }
            return new EmptyResult();
        }
    }
}
