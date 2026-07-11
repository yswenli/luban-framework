/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Office.Ppt
*文件名： PptShape
*版本号： V1.0.0.0
*唯一标识：b056ffb3-45c8-4be5-8df8-62ec71650c0c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/16 9:53:13
*描述：形状
*
*=================================================
*修改标记
*修改时间：2024/7/16 9:53:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：形状
*
*****************************************************************************/
using Aspose.Slides;

using ShapeType = Aspose.Slides.ShapeType;

namespace LuBan.Office.Ppt;

/// <summary>
/// 形状
/// </summary>
[SupportedOSPlatform("windows")]
public class PptShape
{
    IShape _shape;

    PptSlide _slide;

    PptDocument _pptDocument;

    /// <summary>
    /// 形状
    /// </summary>
    /// <param name="pptDocument"></param>
    /// <param name="slide"></param>
    /// <param name="shape"></param>
    internal PptShape(PptDocument pptDocument, PptSlide slide, IShape shape)
    {
        _pptDocument = pptDocument;
        _slide = slide;
        _shape = shape;
    }

    /// <summary>
    /// 获取文本
    /// </summary>
    /// <returns></returns>
    public string GetText()
    {
        return (_shape is AutoShape autoShap) ? autoShap.TextFrame?.Text ?? "" : "";
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public bool SetText(string str)
    {
        if (_shape is AutoShape autoShap && autoShap != null && autoShap.TextFrame != null)
        {
            autoShap.TextFrame.Text = str;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="oldStr"></param>
    /// <param name="newStr"></param>
    /// <returns></returns>
    public bool ReplaceText(string oldStr, string newStr)
    {
        var text = GetText();
        if (text == oldStr)
        {
            return SetText(newStr);
        }
        return false;
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="replaceDic"></param>
    /// <returns></returns>
    public bool ReplaceText(Dictionary<string, string> replaceDic)
    {
        if (replaceDic == null) return false;
        foreach (var replaceStr in replaceDic)
        {
            if (replaceStr.Key == GetText())
            {
                SetText(replaceStr.Value);
            }
        }
        return true;
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="replaceStrs"></param>
    /// <returns></returns>
    public bool ReplaceText(params (string, string)[] replaceStrs)
    {
        if (replaceStrs == null) return false;
        foreach (var replaceStr in replaceStrs)
        {
            if (replaceStr.Item1 == GetText())
            {
                SetText(replaceStr.Item2);
            }
        }
        return true;
    }


    /// <summary>
    /// 替换表格内的文本，
    /// https://docs.aspose.com/slides/net/manage-cells/
    /// </summary>
    /// <param name="oldStr"></param>
    /// <param name="newStr"></param>
    /// <returns></returns>
    public bool ReplaceTextInTable(string oldStr, string newStr)
    {
        if (oldStr.IsNotNullOrEmpty()) return false;
        if (_shape is Table table && table != null && table.Rows != null && table.Rows.Count > 0)
        {
            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                for (int cellIndex = 0; cellIndex < table.Rows[rowIndex].Count; cellIndex++)
                {
                    var cell = table.Rows[rowIndex][cellIndex];
                    if (cell != null && cell.TextFrame != null && cell.TextFrame.Text.IsNotNullOrEmpty() && cell.TextFrame.Text.IndexOf(oldStr) > -1)
                    {
                        cell.TextFrame.Text.Replace(oldStr, newStr);
                    }
                }
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// 替换表格内的文本，
    /// https://docs.aspose.com/slides/net/manage-cells/
    /// </summary>
    /// <param name="replaceDic"></param>
    /// <returns></returns>
    public bool ReplaceTextInTable(Dictionary<string, string> replaceDic)
    {
        if (replaceDic == null) return false;
        if (_shape is Table table && table != null && table.Rows != null && table.Rows.Count > 0)
        {
            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                for (int cellIndex = 0; cellIndex < table.Rows[rowIndex].Count; cellIndex++)
                {
                    var cell = table.Rows[rowIndex][cellIndex];
                    if (cell != null && cell.TextFrame != null && cell.TextFrame.Text.IsNotNullOrEmpty())
                    {
                        if (replaceDic.ContainsKey(cell.TextFrame.Text))
                        {
                            cell.TextFrame.Text = replaceDic[cell.TextFrame.Text];
                        }
                    }
                }
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// 替换表格内的文本，
    /// https://docs.aspose.com/slides/net/manage-cells/
    /// </summary>
    /// <param name="replaceStrs"></param>
    /// <returns></returns>
    public bool ReplaceTextInTable(params (string, string)[] replaceStrs)
    {
        if (replaceStrs == null || replaceStrs.Length < 1) return false;
        if (_shape is Table table && table != null && table.Rows != null && table.Rows.Count > 0)
        {
            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                for (int cellIndex = 0; cellIndex < table.Rows[rowIndex].Count; cellIndex++)
                {
                    var cell = table.Rows[rowIndex][cellIndex];
                    if (cell != null && cell.TextFrame != null && cell.TextFrame.Text.IsNotNullOrEmpty())
                    {

                        var item = replaceStrs.Where(q => cell.TextFrame.Text.Contains(q.Item1)).FirstOrDefault();
                        if (item != default)
                        {
                            cell.TextFrame.Text = cell.TextFrame.Text.Replace(item.Item1, item.Item2);
                        }
                    }
                }
            }

            return true;
        }
        return false;
    }


    /// <summary>
    /// 添加或替换图像,
    /// https://docs.aspose.com/slides/net/image/
    /// </summary>
    /// <param name="imagePath"></param>
    public void ReplaceImage(string imagePath)
    {
        IPPImage image = _pptDocument.GetOrAddImage(imagePath);
        _shape.Slide.Shapes.AddPictureFrame(ShapeType.Rectangle, _shape.X, _shape.Y, _shape.Width, _shape.Height, image);
    }

    /// <summary>
    /// 添加或替换图像,
    /// https://docs.aspose.com/slides/net/image/
    /// </summary>
    /// <param name="imageIndex"></param>
    public void ReplaceImage(int imageIndex)
    {
        IPPImage image = _shape.Slide.Presentation.Images[imageIndex];
        _shape.Slide.Shapes.AddPictureFrame(ShapeType.Rectangle, _shape.X, _shape.Y, _shape.Width, _shape.Height, image);
    }

    /// <summary>
    /// 替换视频，
    /// https://docs.aspose.com/slides/net/video-frame/
    /// </summary>
    /// <param name="videoPath"></param>
    public void ReplaceVideo(string videoPath, string coverPath)
    {
        IVideoFrame vf = _shape.Slide.Shapes.AddVideoFrame(_shape.X, _shape.Y, _shape.Width, _shape.Height, videoPath);
        vf.PlayMode = VideoPlayModePreset.Auto;
        vf.Volume = AudioVolumeMode.Loud;
        if (coverPath.IsNotNullOrEmpty())
        {
            vf.PictureFormat.Picture.Image = _pptDocument.GetOrAddImage(coverPath);
        }
    }

    /// <summary>
    /// 替换视频，
    /// https://docs.aspose.com/slides/net/video-frame/
    /// </summary>
    /// <param name="videoPath"></param>
    /// <param name="imageIndex"></param>
    public void ReplaceVideo(string videoPath, int imageIndex)
    {
        IVideoFrame vf = _shape.Slide.Shapes.AddVideoFrame(_shape.X, _shape.Y, _shape.Width, _shape.Height, videoPath);
        vf.PlayMode = VideoPlayModePreset.Auto;
        vf.Volume = AudioVolumeMode.Loud;
        vf.PictureFormat.Picture.Image = _shape.Slide.Presentation.Images[imageIndex];
    }


    /// <summary>
    /// 替换视频，
    /// https://docs.aspose.com/slides/net/video-frame/
    /// </summary>
    /// <param name="videoIndex"></param>
    /// <param name="imageIndex"></param>
    public void ReplaceVideo(int videoIndex, int imageIndex)
    {
        var video = _shape.Slide.Presentation.Videos[videoIndex];
        IVideoFrame vf = _shape.Slide.Shapes.AddVideoFrame(_shape.X, _shape.Y, _shape.Width, _shape.Height, video);
        vf.PlayMode = VideoPlayModePreset.Auto;
        vf.Volume = AudioVolumeMode.Loud;
        vf.PictureFormat.Picture.Image = _shape.Slide.Presentation.Images[imageIndex];
    }
}
