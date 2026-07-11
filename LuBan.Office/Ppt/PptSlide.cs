/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Office.Pptx
*文件名： PptSlide
*版本号： V1.0.0.0
*唯一标识：a28a4045-5f36-423b-99f6-922599c4e85a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/15 20:03:30
*描述：幻灯片
*
*=================================================
*修改标记
*修改时间：2024/7/15 20:03:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：幻灯片
*
*****************************************************************************/
using Aspose.Slides;

using System.Drawing;

namespace LuBan.Office.Ppt;

/// <summary>
/// 幻灯片
/// </summary>
[SupportedOSPlatform("windows")]
public class PptSlide
{
    internal ISlide _slide;
    PptDocument _pptDocument;

    /// <summary>
    /// 幻灯片
    /// </summary>
    /// <param name="pptDocument"></param>
    /// <param name="slide"></param>
    internal PptSlide(PptDocument pptDocument, ISlide slide)
    {
        _pptDocument = pptDocument;
        _slide = slide;
    }

    /// <summary>
    /// 复制当前幻灯片
    /// </summary>
    /// <returns></returns>
    public PptSlide Clone()
    {
        return new PptSlide(_pptDocument, _slide.Presentation.Slides.AddClone(_slide));
    }

    /// <summary>
    /// 获取背景色
    /// </summary>
    /// <returns></returns>
    public Color GetBackGroundColor()
    {
        return _slide.Background.GetEffective().FillFormat.SolidFillColor;
    }

    /// <summary>
    /// 设置背景色
    /// </summary>
    /// <param name="color"></param>
    public void SetBackGroundColor(Color color)
    {
        _slide.Background.Type = BackgroundType.OwnBackground;
        _slide.Background.FillFormat.FillType = FillType.Solid;
        _slide.Background.FillFormat.SolidFillColor.Color = color;
    }

    /// <summary>
    /// 设置背景图
    /// </summary>
    /// <param name="backgroundImageFilePath"></param>
    public void SetBackGroundImage(string backgroundImageFilePath)
    {
        _slide.Background.Type = BackgroundType.OwnBackground;
        _slide.Background.FillFormat.FillType = FillType.Picture;
        _slide.Background.FillFormat.PictureFillFormat.PictureFillMode = PictureFillMode.Stretch;
        IPPImage imgx = _pptDocument.GetOrAddImage(backgroundImageFilePath);
        _slide.Background.FillFormat.PictureFillFormat.Picture.Image = imgx;
    }

    /// <summary>
    /// 设置背景图
    /// </summary>
    /// <returns></returns>
    public byte[] GetBackGroundImage()
    {
        return _slide.Background.FillFormat.PictureFillFormat.Picture.Image.BinaryData;
    }


    /// <summary>
    /// 获取全部文本框
    /// </summary>
    /// <returns></returns>
    public IEnumerable<PptShape> GetTextShapes()
    {
        var shapes = _slide.Shapes;
        if (shapes == null || _slide.Shapes.Count < 1) yield break;
        var data = shapes.Where(q => q is AutoShape).ToList();
        if (data != null && data.Count > 0)
        {
            foreach (AutoShape shape in data)
            {
                if (shape.TextFrame != null)
                {
                    yield return new PptShape(_pptDocument, this, shape);
                }

            }
        }
        yield break;
    }

    /// <summary>
    /// 获取表格
    /// </summary>
    /// <returns></returns>
    public IEnumerable<PptShape> GeTableShapes()
    {
        var shapes = _slide.Shapes;
        if (shapes == null || _slide.Shapes.Count < 1) yield break;
        var data = shapes.Where(q => q is Table).ToList();
        if (data != null && data.Count > 0)
        {
            foreach (Table shape in data)
            {
                if (shape != null)
                {
                    yield return new PptShape(_pptDocument, this, shape);
                }

            }
        }
        yield break;
    }


    /// <summary>
    /// 获得形状
    /// </summary>
    /// <param name="alternativeText"></param>
    /// <returns></returns>
    public IEnumerable<PptShape> GetShapesByAltTxt(string alternativeText)
    {
        var shapes = _slide.Shapes;
        if (shapes == null || _slide.Shapes.Count < 1) yield break;
        var data = shapes.Where(q => q.AlternativeText == alternativeText || q.AlternativeTextTitle == alternativeText).ToList();
        if (data != null && data.Count > 0)
        {
            foreach (var shape in data)
            {
                yield return new PptShape(_pptDocument, this, shape);
            }
        }
        yield break;
    }



    /// <summary>
    /// 获得形状
    /// </summary>
    /// <param name="alternativeText"></param>
    /// <returns></returns>
    public PptShape? GetShapeByAltTxt(string alternativeText)
    {
        var slide = _slide.FindShapeByAltText(alternativeText);
        if (slide != null)
        {
            return new PptShape(_pptDocument, this, slide);
        }
        return null;
    }


    /// <summary>
    /// 获取全部文本
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllText()
    {
        var shapes = GetTextShapes();
        if (shapes == null || !shapes.Any()) yield break;
        foreach (var shape in shapes)
        {
            var text = shape.GetText();
            if (text.IsNotNullOrEmpty())
            {
                yield return text;
            }
        }
        yield break;
    }



    /// <summary>
    /// 添加音频
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="audioStream"></param>
    /// <returns></returns>
    public PptShape AddAudioFrameEmbedded(float x, float y, float width, float height, string audioFilePath)
    {
        using (var fs = File.OpenRead(audioFilePath))
        {
            return new PptShape(_pptDocument, this, _slide.Shapes.AddAudioFrameEmbedded(x, y, width, height, fs));
        }
    }

    /// <summary>
    /// 添加视频
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="videoFilePath"></param>
    /// <param name="coverFilePath"></param>
    /// <returns></returns>
    public PptShape AddVideoFrame(float x, float y, float width, float height, string videoFilePath, string coverFilePath)
    {
        var vf = _slide.Shapes.AddVideoFrame(x, y, width, height, videoFilePath);
        vf.PlayMode = VideoPlayModePreset.Auto;
        vf.Volume = AudioVolumeMode.Loud;
        if (coverFilePath.IsNotNullOrEmpty() && File.Exists(coverFilePath))
        {
            vf.PictureFormat.Picture.Image = _pptDocument.GetOrAddImage(coverFilePath);
        }
        return new PptShape(_pptDocument, this, vf);
    }
}
