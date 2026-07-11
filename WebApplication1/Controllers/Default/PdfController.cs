/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Controllers.Default
*文件名： PdfController
*版本号： V1.0.0.0
*唯一标识：8ff632ab-df9f-4db4-a57c-40355f36c23a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/8 11:03:25
*描述：pdf
*
*=================================================
*修改标记
*修改时间：2025/8/8 11:03:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：pdf
*
*****************************************************************************/
namespace WebApplication1.Controllers.Default
{
    /// <summary>
    /// pdf
    /// </summary>
    [AllowAccess, AllowAnonymous]
    public class PdfController : BaseApiController
    {

        /// <summary>
        /// 替换文本
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public string GetReplaceFile()
        {
            var srcFileName = Path.Combine(WebApp.GetPhysicalPath("wwwroot", "files"), "PDF编辑器使用教程.pdf");
            var dstFileName = Path.Combine(WebApp.GetPhysicalPath("wwwroot", "files"), "PDF编辑器使用教程Out.pdf");

            var data = new Dictionary<string, string>();
            data.Add("表单", "表表单单");
            //仅在.netframeworke 版本可用
            //var pdfDoc = new PdfDocument(srcFileName);
            //pdfDoc.ReplaceTexts(data);
            //pdfDoc.Save(dstFileName);

            return dstFileName;
        }

    }
}
