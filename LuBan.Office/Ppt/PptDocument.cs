/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Office.Pptx
*文件名： PptDocument
*版本号： V1.0.0.0
*唯一标识：e82d892b-864d-4eb2-936e-93f7bd51e6c4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/15 18:14:15
*描述：ppt文档
*
*=================================================
*修改标记
*修改时间：2024/7/15 18:14:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ppt文档
*
*****************************************************************************/
using Aspose.Slides;

namespace LuBan.Office.Ppt;

/// <summary>
/// ppt文档，
/// https://reference.aspose.com/tutorials/slides/zh/net/
/// </summary>
[SupportedOSPlatform("windows")]
public class PptDocument : IDisposable
{
    internal Presentation _presentation;

    string _pptFilePath;

    ICommentAuthor _author;

    /// <summary>
    /// ppt文档
    /// </summary>
    [SupportedOSPlatform("windows")]
    public PptDocument() : this(string.Empty)
    {

    }

    /// <summary>
    /// ppt文档
    /// </summary>
    /// <param name="pptPath"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    [SupportedOSPlatform("windows")]
    public PptDocument(string pptPath)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            throw new PlatformNotSupportedException("LuBan.Office only support Windows");
        }

        License.Instance.SetSlidesLicense();

        LoadOptions loadOptions = new LoadOptions
        {
            BlobManagementOptions = new BlobManagementOptions
            {
                PresentationLockingBehavior = PresentationLockingBehavior.KeepLocked,
                IsTemporaryFilesAllowed = true
            }
        };

        if (pptPath.IsNullOrEmpty())
        {
            _presentation = new Presentation(loadOptions);
        }
        else
        {
            if (!File.Exists(pptPath)) throw new FileNotFoundException("ppt file not found", pptPath);
            _pptFilePath = pptPath;
            _presentation = new Presentation(_pptFilePath, loadOptions);
        }
        _author = _presentation.CommentAuthors.AddAuthor("yswenil", "SA");
    }

    /// <summary>
    /// ppt文档
    /// </summary>
    /// <param name="ms"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    [SupportedOSPlatform("windows")]
    public PptDocument(MemoryStream ms)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            throw new PlatformNotSupportedException("LuBan.Office only support Windows");
        }

        License.Instance.SetSlidesLicense();

        LoadOptions loadOptions = new LoadOptions
        {
            BlobManagementOptions = new BlobManagementOptions
            {
                PresentationLockingBehavior = PresentationLockingBehavior.KeepLocked,
                IsTemporaryFilesAllowed = true
            }
        };

        if (ms == null || !ms.CanRead)
        {
            _presentation = new Presentation(loadOptions);
        }
        else
        {
            _presentation = new Presentation(ms, loadOptions);
        }
        _author = _presentation.CommentAuthors.AddAuthor("yswenil", "SA");
    }

    /// <summary>
    /// 获取ppt中所有文本
    /// </summary>
    /// <returns></returns>
    public List<string>? GetAllText()
    {
        IPresentationText text = new PresentationFactory().GetPresentationText(_pptFilePath, TextExtractionArrangingMode.Unarranged);
        if (text == null || text.SlidesText == null || text.SlidesText.Length < 1) return null;
        var result = new List<string>();
        foreach (var slideText in text.SlidesText)
        {
            if (slideText.LayoutText.IsNotNullOrEmpty())
                result.Add(slideText.LayoutText);
            if (slideText.Text.IsNotNullOrEmpty())
                result.Add(slideText.Text);
        }
        return result;
    }

    /// <summary>
    /// 通过备注查找幻灯片
    /// </summary>
    /// <param name="nodeText"></param>
    /// <returns></returns>
    public IEnumerable<PptSlide> GetSlidesByNoteText(string nodeText)
    {
        var sliders = _presentation.Slides;
        if (sliders != null && sliders.Count > 0)
        {
            foreach (ISlide slide in sliders)
            {
                var noteText = slide.NotesSlideManager.NotesSlide.NotesTextFrame.Text;
                if (noteText.IsNotNullOrEmpty() && noteText.IndexOf(nodeText) > -1)
                {
                    yield return new PptSlide(this, slide);
                }
            }
        }
        yield break;
    }

    /// <summary>
    /// 获取幻灯片
    /// </summary>
    /// <param name="slideId"></param>
    /// <returns></returns>
    public PptSlide? GetSlide(int slideId)
    {
        var slide = _presentation.Slides.Where(q => q.SlideId == slideId).FirstOrDefault();
        if (slide != null) return new PptSlide(this, slide);
        return null;
    }

    /// <summary>
    /// 获取幻灯片
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public PptSlide? GetSlideByIndex(int index)
    {
        var slides = _presentation.Slides;
        if (slides != null && slides.Count > index)
        {
            return new PptSlide(this, slides[index]);
        }
        return null;
    }

    /// <summary>
    /// 获取幻灯片
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public PptSlide? GetSlideByNumber(int number)
    {
        var slide = _presentation.Slides.Where(q => q.SlideNumber == number).FirstOrDefault();
        if (slide != null) return new PptSlide(this, slide);
        return null;
    }

    /// <summary>
    /// 添加幻灯片
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public PptSlide AddSlide(int index = -1)
    {
        var layerout = _presentation.LayoutSlides.First();
        if (index == -1)
        {
            index = _presentation.Slides.Count - 1;
        }
        return new PptSlide(this, _presentation.Slides.InsertEmptySlide(index, layerout));
    }
    /// <summary>
    /// 添加幻灯片
    /// </summary>
    /// <param name="slide"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public PptSlide AddSlide(PptSlide slide, int index = -1)
    {
        if (index == -1)
        {
            index = _presentation.Slides.Count - 1;
        }
        var targetSlide = _presentation.Slides.InsertClone(index, slide._slide);
        return new PptSlide(this, targetSlide);
    }

    /// <summary>
    /// 添加幻灯片
    /// </summary>
    /// <param name="fromIndex"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public PptSlide AddSlide(int fromIndex, int index = -1)
    {
        var slider = _presentation.Slides[fromIndex];
        if (index == -1)
        {
            index = _presentation.Slides.Count - 1;
        }
        return new PptSlide(this, _presentation.Slides.InsertClone(index, slider));
    }

    /// <summary>
    /// 删除幻灯片
    /// </summary>
    /// <param name="index"></param>
    public void RemoveSlide(int index)
    {
        _presentation.Slides.RemoveAt(index);
    }

    /// <summary>
    /// 删除幻灯片
    /// </summary>
    /// <param name="pptSlide"></param>
    public void RemoveSlide(PptSlide pptSlide)
    {
        _presentation.Slides.Remove(pptSlide._slide);
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="pptFilePath"></param>
    public bool Save(string pptFilePath = "")
    {
        try
        {
            if (pptFilePath.IsNullOrEmpty())
            {
                pptFilePath = _pptFilePath;
            }
            if (pptFilePath.IsNullOrEmpty())
            {
                pptFilePath = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.pptx";
            }
            var extensionName = Path.GetExtension(pptFilePath);
            if (extensionName.StartsWith(".pptx"))
            {
                _presentation.Save(pptFilePath, Aspose.Slides.Export.SaveFormat.Pptx);
            }
            else
            {
                _presentation.Save(pptFilePath, Aspose.Slides.Export.SaveFormat.Ppt);
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        return false;
    }


    /// <summary>
    /// 保存为PDF
    /// </summary>
    /// <param name="pdfFilePath"></param>
    public void SaveAsPdf(string pdfFilePath)
    {
        _presentation.Save(pdfFilePath, Aspose.Slides.Export.SaveFormat.Pdf);
    }

    /// <summary>
    /// 保存为Html
    /// </summary>
    /// <param name="pdfFilePath"></param>
    public void SaveAsHtml(string pdfFilePath)
    {
        _presentation.Save(pdfFilePath, Aspose.Slides.Export.SaveFormat.Html);
    }

    ConcurrentDictionary<string, IPPImage> _images = [];

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="imagePath"></param>
    /// <returns></returns>
    public IPPImage GetOrAddImage(string imagePath)
    {
        var fileName = Path.GetFileName(imagePath);
        return _images.GetOrAdd(fileName, (key) => _presentation.Images.AddImage(
        FileUtil.Read(imagePath)));
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="name"></param>
    /// <param name="imagePath"></param>
    /// <returns></returns>
    public IPPImage GetOrAddImage(string name, string imagePath)
    {
        return _images.GetOrAdd(name, (key) => _presentation.Images.AddImage(FileUtil.Read(imagePath)));
    }


    /// <summary>
    /// 释放文档资源
    /// </summary>
    public void Dispose()
    {
        _ = Save();
        _images.Clear();
        _presentation.Dispose();
    }


    #region 快捷方法
    /// <summary>
    /// 加载ppt文档
    /// </summary>
    /// <param name="pptFilePath"></param>
    /// <returns></returns>
    public static PptDocument Load(string pptFilePath)
    {
        return new PptDocument(pptFilePath);
    }

    /// <summary>
    /// 从html加载ppt文档
    /// </summary>
    /// <param name="htmlFilePath"></param>
    /// <returns></returns>
    public static PptDocument LoadFromHtml(string htmlFilePath)
    {
        var doc = new PptDocument();
        doc._presentation.Slides.AddFromHtml(htmlFilePath);
        return doc;
    }

    /// <summary>
    /// 从pdf加载ppt文档
    /// </summary>
    /// <param name="pdfFilePath"></param>
    /// <returns></returns>
    public static PptDocument LoadFromPdf(string pdfFilePath)
    {
        using (var ms = new MemoryStream())
        {
            ms.Position = 0;
            Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(pdfFilePath);
            pdf.Save(ms, Aspose.Pdf.SaveFormat.Pptx);
            ms.Position = 0;
            return new PptDocument(ms);
        }
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="document"></param>
    /// <param name="slideIndex"></param>
    /// <param name="replaceStrs"></param>
    /// <returns></returns>
    public static bool ReplaceText(PptDocument document, int slideIndex, (string, string)[] replaceStrs)
    {
        var slide = document.GetSlideByIndex(slideIndex);
        if (slide == null) return false;
        var shapes = slide.GetTextShapes();
        if (shapes != null)
        {
            foreach (var shape in shapes)
            {
                if (shape == null) continue;
                shape.ReplaceText(replaceStrs);
            }
        }
        return true;
    }

    /// <summary>
    /// 根据alt文本替换图片
    /// </summary>
    /// <param name="document"></param>
    /// <param name="slideIndex"></param>
    /// <param name="imagePath"></param>
    /// <param name="altTxt"></param>
    /// <param name="deletionFile"></param>
    /// <param name="autoDeleteIfNull"></param>
    /// <returns></returns>
    public static bool ReplaceImage(PptDocument document, int slideIndex, string imagePath, string altTxt, DeletionFile deletionFile, bool autoDeleteIfNull = true)
    {
        var slide = document.GetSlideByIndex(slideIndex);
        if (slide == null) return false;
        var shapes = slide.GetShapesByAltTxt(altTxt);
        if (shapes != null)
        {
            foreach (var shape in shapes)
            {
                shape.ReplaceImage(imagePath);
                deletionFile.Add(imagePath);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 根据alt文本替换视频
    /// </summary>
    /// <param name="document"></param>
    /// <param name="slideIndex"></param>
    /// <param name="videoPath"></param>
    /// <param name="coverImgPath"></param>
    /// <param name="altTxt"></param>
    /// <param name="deletionFile"></param>
    /// <param name="autoDeleteIfNull"></param>
    /// <returns></returns>
    public static bool ReplaceVideo(PptDocument document, int slideIndex, string videoPath, string coverImgPath, string altTxt, DeletionFile deletionFile, bool autoDeleteIfNull = true)
    {
        var slide = document.GetSlideByIndex(slideIndex);
        if (slide == null) return false;
        var shapes = slide.GetShapesByAltTxt(altTxt);
        if (shapes != null)
        {
            foreach (var shape in shapes)
            {
                shape.ReplaceVideo(videoPath, coverImgPath);
                deletionFile.Add(videoPath);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 替换表格文本
    /// </summary>
    /// <param name="document"></param>
    /// <param name="slideIndex"></param>
    /// <param name="replaceStrs"></param>
    /// <returns></returns>
    public static bool ReplaceTableText(PptDocument document, int slideIndex, (string, string)[] replaceStrs)
    {
        var slide = document.GetSlideByIndex(slideIndex);
        if (slide == null) return false;
        var shapes = slide.GeTableShapes();
        if (shapes != null)
        {
            foreach (var shape in shapes)
            {
                if (shape == null) continue;
                shape.ReplaceTextInTable(replaceStrs);
            }
        }
        return true;
    }


    #endregion
}
