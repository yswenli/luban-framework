/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Office.Pdf
*文件名： PdfDocument
*版本号： V1.0.0.0
*唯一标识：b0061f67-6bea-4088-b99a-5875ea74f940
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/8 10:14:14
*描述：pdf文档
*
*=================================================
*修改标记
*修改时间：2025/8/8 10:14:14
*修改人： yswenli
*版本号： V1.0.0.0
*描述：pdf文档
*
*****************************************************************************/
using Aspose.Pdf;
using Aspose.Pdf.Text;

namespace LuBan.Office.Pdf;

/// <summary>
/// pdf文档,
/// //仅在.netframeworke 版本可用,
/// https://www.evget.com/doclib/s/51
/// </summary>
[SupportedOSPlatform("windows")]
public class PdfDocument : IDisposable
{
    Document _doc;

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// pdf文档
    /// </summary>
    /// <param name="filePath"></param>
    public PdfDocument(string filePath)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            throw new PlatformNotSupportedException("LuBan.Office only support Windows");
        }
        License.Instance.SetPdfLicense();
        FilePath = filePath;
        _doc = new Document(filePath);
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="oldText"></param>
    /// <param name="newText"></param>
    /// <returns></returns>
    public void ReplaceText(string oldText, string newText)
    {
        TextFragmentAbsorber textFragmentAbsorber = new TextFragmentAbsorber();
        var textSearchOptions = new TextSearchOptions(true);
        textFragmentAbsorber.TextSearchOptions = textSearchOptions;
        _doc.Pages.Accept(textFragmentAbsorber);
        var textFragmentCollection = textFragmentAbsorber.TextFragments;
        foreach (TextFragment textFragment in textFragmentCollection)
        {
            textFragment.TextState.Font = FontRepository.FindFont(oldText);
            textFragment.Text = newText;
            textFragment.TextState.FontSize = 22;
            textFragment.TextState.ForegroundColor = Aspose.Pdf.Color.FromRgb(System.Drawing.Color.Blue);
            textFragment.TextState.BackgroundColor = Aspose.Pdf.Color.FromRgb(System.Drawing.Color.Green);
        }
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="dictionary"></param>
    public void ReplaceTexts(Dictionary<string, string> dictionary)
    {
        var textFragmentAbsorber = new TextFragmentAbsorber();
        var textSearchOptions = new TextSearchOptions(true);
        textFragmentAbsorber.TextSearchOptions = textSearchOptions;
        _doc.Pages.Accept(textFragmentAbsorber);
        var textFragmentCollection = textFragmentAbsorber.TextFragments;
        foreach (TextFragment textFragment in textFragmentCollection)
        {
            foreach (var item in dictionary)
            {
                textFragment.TextState.Font = FontRepository.FindFont(item.Key);
                textFragment.Text = item.Value;
                textFragment.TextState.FontSize = 22;
                textFragment.TextState.ForegroundColor = Aspose.Pdf.Color.FromRgb(System.Drawing.Color.Blue);
                textFragment.TextState.BackgroundColor = Aspose.Pdf.Color.FromRgb(System.Drawing.Color.Green);
            }
        }
    }

    /// <summary>
    /// 替换图片
    /// </summary>
    /// <param name="newPictureFilePath"></param>
    /// <param name="pageIndex"></param>
    /// <param name="oldPictureIndex"></param>
    public void ReplacePicture(string newPictureFilePath, int pageIndex = 1, int oldPictureIndex = 1)
    {
        using var fileStream = new FileStream(newPictureFilePath, FileMode.Open, FileAccess.Read);
        _doc.Pages[pageIndex].Resources.Images.Replace(oldPictureIndex, fileStream);
    }
    /// <summary>
    /// 将Html思考换成转换为pdf
    /// </summary>
    /// <param name="htmlFilePath"></param>
    public void ConvertFromHtml(string htmlFilePath)
    {
        var htmloptions = new HtmlLoadOptions();
        _doc = new Document(htmlFilePath, htmloptions);
    }

    /// <summary>
    /// 将url内容转换成pdf
    /// </summary>
    /// <param name="url"></param>
    public void ConvertFromUrl(string url)
    {
        var req = WebRequest.Create(url);
        using var stream = req.GetResponse().GetResponseStream();
        var htmloptions = new HtmlLoadOptions();
        _doc = new Document(stream, htmloptions);
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="outputFilePath"></param>
    public void Save(string? outputFilePath = null)
    {
        if (string.IsNullOrEmpty(outputFilePath))
        {
            _doc.Save();
        }
        else
        {
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            _doc.Save(outputFilePath);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _doc.Dispose();
    }
}
