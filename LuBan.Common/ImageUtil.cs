/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： ImageUtil
*版本号： V1.0.0.0
*唯一标识：dc0d0d9f-ed6a-4476-a033-3019a25ad713
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/15 14:36:25
*描述：图片处理工具类
*
*=================================================
*修改标记
*修改时间：2024/7/15 14:36:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：图片处理工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 图片处理工具类
/// </summary>
public static class ImageUtil
{
    /// <summary>
    /// 生成缩略图
    /// </summary>
    /// <param name="originalImageBytes">源图字节数组</param>
    /// <param name="width">缩略图宽度</param>
    /// <param name="height">缩略图高度</param>
    /// <param name="mode">生成缩略图的方式</param>
    /// <returns>缩略图字节数组</returns>
    public static byte[] MakeThumbnail(byte[] originalImageBytes, int width, int height, string mode)
    {
        using var inputStream = new SKMemoryStream(originalImageBytes);
        using var originalImage = SKImage.FromEncodedData(inputStream);

        int toWidth = width;
        int toHeight = height;
        int x = 0, y = 0, ow = originalImage.Width, oh = originalImage.Height;

        switch (mode)
        {
            case "HW": // 指定高宽缩放（可能变形）
                break;
            case "W": // 指定宽，高按比例
                toHeight = originalImage.Height * width / originalImage.Width;
                break;
            case "H": // 指定高，宽按比例
                toWidth = originalImage.Width * height / originalImage.Height;
                break;
            case "Cut": // 指定高宽裁减（不变形）
                if ((double)originalImage.Width / originalImage.Height > (double)toWidth / toHeight)
                {
                    ow = originalImage.Height * toWidth / toHeight;
                    x = (originalImage.Width - ow) / 2;
                }
                else
                {
                    oh = originalImage.Width * height / toWidth;
                    y = (originalImage.Height - oh) / 2;
                }
                break;
        }

        using var surface = SKSurface.Create(new SKImageInfo(toWidth, toHeight, originalImage.ColorType, originalImage.AlphaType, originalImage.ColorSpace));
        using var canvas = surface.Canvas;
        var srcRect = new SKRect(x, y, x + ow, y + oh);
        var destRect = new SKRect(0, 0, toWidth, toHeight);
        canvas.DrawImage(originalImage, srcRect, destRect, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

        using var resizedImage = surface.Snapshot();
        var data = resizedImage.Encode(SKEncodedImageFormat.Png, 100);
        var outputStream = new MemoryStream();
        data.SaveTo(outputStream);
        outputStream.Seek(0, SeekOrigin.Begin);
        return outputStream.ToArray();
    }

    /// <summary>
    /// 添加水印
    /// </summary>
    /// <param name="originalImageBytes">源图字节数组</param>
    /// <param name="waterMarkTxt">水印文字</param>
    /// <param name="imagePosition">水印位置</param>
    /// <param name="color">水印颜色</param>
    /// <returns>带水印的图像字节数组</returns>
    public static byte[] MakeWaterMark(byte[] originalImageBytes, string waterMarkTxt, EnumImagePosition imagePosition, SKColor color)
    {
        using var inputStream = new SKMemoryStream(originalImageBytes);
        using var originalImage = SKImage.FromEncodedData(inputStream);

        using var surface = SKSurface.Create(originalImage.Info);
        using var canvas = surface.Canvas;

        // 绘制原图
        canvas.DrawImage(originalImage, 0, 0);

        var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true
        };
        var position = GetPosition(originalImage.Width, originalImage.Height, imagePosition, paint, waterMarkTxt);
        using var font = new SKFont();
        canvas.DrawText(waterMarkTxt, position.X, position.Y, SKTextAlign.Left, font, paint);

        using var watermarkedImage = surface.Snapshot();
        var data = watermarkedImage.Encode(SKEncodedImageFormat.Png, 100);
        var outputStream = new MemoryStream();
        data.SaveTo(outputStream);
        outputStream.Seek(0, SeekOrigin.Begin);
        return outputStream.ToArray();
    }

    /// <summary>
    /// 图片灰度化
    /// </summary>
    /// <param name="originalImageBytes">源图字节数组</param>
    /// <returns>灰度化后的图像字节数组</returns>
    public static byte[] MakeGray(byte[] originalImageBytes)
    {
        using var inputStream = new SKMemoryStream(originalImageBytes);
        using var originalImage = SKImage.FromEncodedData(inputStream);

        using var surface = SKSurface.Create(originalImage.Info);
        using var canvas = surface.Canvas;
        var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(
            [
                0.3f, 0.3f, 0.3f, 0, 0,
                0.59f, 0.59f, 0.59f, 0, 0,
                0.11f, 0.11f, 0.11f, 0, 0,
                0, 0, 0, 1, 0
            ])
        };
        canvas.DrawImage(originalImage, 0, 0, paint);

        using var grayImage = surface.Snapshot();
        var data = grayImage.Encode(SKEncodedImageFormat.Png, 100);
        var outputStream = new MemoryStream();
        data.SaveTo(outputStream);
        outputStream.Seek(0, SeekOrigin.Begin);
        return outputStream.ToArray();
    }

    /// <summary>
    /// 指定文字或图片绘制图片
    /// </summary>
    /// <param name="originalImageBytes"></param>
    /// <param name="imageInfo"></param>
    /// <returns></returns>
    public static byte[] DrawImage(byte[] originalImageBytes, ImageInfo imageInfo)
    {
        using var imageTemplate = new ImageTeplate(originalImageBytes);
        using var ms = imageTemplate.Draw(imageInfo);
        return ms.ToArray();
    }


    #region Private Methods

    private static SKPoint GetPosition(int width, int height, EnumImagePosition imagePosition, SKPaint paint, string text)
    {
        var textBounds = new SKRect();
        using var font = new SKFont();
        font.MeasureText(text, out textBounds, paint);

        return imagePosition switch
        {
            EnumImagePosition.TopLeft => new SKPoint(10, textBounds.Height + 10),
            EnumImagePosition.TopCenter => new SKPoint((width - textBounds.Width) / 2, textBounds.Height + 10),
            EnumImagePosition.TopRight => new SKPoint(width - textBounds.Width - 10, textBounds.Height + 10),
            EnumImagePosition.MiddleLeft => new SKPoint(10, (height + textBounds.Height) / 2),
            EnumImagePosition.MiddleCenter => new SKPoint((width - textBounds.Width) / 2, (height + textBounds.Height) / 2),
            EnumImagePosition.MiddleRight => new SKPoint(width - textBounds.Width - 10, (height + textBounds.Height) / 2),
            EnumImagePosition.BottomLeft => new SKPoint(10, height - 10),
            EnumImagePosition.BottomCenter => new SKPoint((width - textBounds.Width) / 2, height - 10),
            EnumImagePosition.BottomRight => new SKPoint(width - textBounds.Width - 10, height - 10),
            _ => new SKPoint(10, 10)
        };
    }

    #endregion
}

/// <summary>
/// 图片模板
/// </summary>
public class ImageTeplate : IDisposable
{
    SKMemoryStream _inputStream;

    /// <summary>
    /// 图片模板
    /// </summary>
    /// <param name="templateFilePath"></param>
    /// <exception cref="Exception"></exception>
    public ImageTeplate(string templateFilePath)
    {
        if (templateFilePath.IsNullOrEmpty()) throw new Exception("图片模板文件路径不能为空");
        if (!File.Exists((string)templateFilePath)) throw new Exception("图片模板文件不存在");
        _inputStream = new SKMemoryStream(File.ReadAllBytes(templateFilePath));
    }

    /// <summary>
    /// 图片模板
    /// </summary>
    /// <param name="originalImageBytes"></param>
    public ImageTeplate(byte[] originalImageBytes)
    {
        _inputStream = new SKMemoryStream(originalImageBytes);
    }

    /// <summary>
    /// 插入文字
    /// </summary>
    /// <param name="text"></param>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="colorName"></param>
    /// <param name="size"></param>
    /// <param name="fontName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public MemoryStream DrawText(string text, float left, float top, string colorName = "#000000", float size = 12, string fontName = "consola")
    {
        var content = new ImageInfo(new ImageInfo.TextDescription()
        {
            Value = text,
            Left = left,
            Top = top,
            Color = colorName,
            Size = size,
            FontFamilyName = fontName
        });
        return Draw(content);
    }

    /// <summary>
    /// 插入图片
    /// </summary>
    /// <param name="imageFilePath"></param>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <returns></returns>
    public MemoryStream DrawImage(string imageFilePath, float left, float top)
    {
        var content = new ImageInfo(new ImageInfo.ImageDescription()
        {
            FilePath = imageFilePath,
            Left = left,
            Top = top
        });
        return Draw(content);
    }

    /// <summary>
    /// 计算等比例缩放后的尺寸（不超过 MaxWidth/MaxHeight，且不拉伸）
    /// </summary>
    /// <param name="sourceWidth">源图片宽度</param>
    /// <param name="sourceHeight">源图片高度</param>
    /// <param name="maxWidth">最大允许宽度</param>
    /// <param name="maxHeight">最大允许高度</param>
    /// <returns>缩放后的宽度、高度</returns>
    private (float Width, float Height) CalculateScaledSize(float sourceWidth, float sourceHeight, float maxWidth, float maxHeight)
    {
        // 边界处理：若 MaxWidth/MaxHeight 为 0 或负数，不缩放（用源图尺寸）
        if (maxWidth <= 0 && maxHeight <= 0)
        {
            return (sourceWidth, sourceHeight);
        }

        // 计算缩放比例（取宽度比例和高度比例的最小值，确保不超过任一最大值）
        float widthRatio = maxWidth > 0 ? maxWidth / sourceWidth : float.MaxValue;
        float heightRatio = maxHeight > 0 ? maxHeight / sourceHeight : float.MaxValue;
        float scaleRatio = Math.Min(widthRatio, heightRatio);

        // 若缩放比例 >=1，不缩放（源图已小于等于最大尺寸）
        if (scaleRatio >= 1)
        {
            return (sourceWidth, sourceHeight);
        }

        // 计算最终缩放尺寸（四舍五入到整数，避免小数像素导致模糊）
        return (
            (float)Math.Round(sourceWidth * scaleRatio),
            (float)Math.Round(sourceHeight * scaleRatio)
        );
    }

    /// <summary>
    /// 生成高质量缩放后的图像
    /// </summary>
    /// <param name="sourceImage">源图像</param>
    /// <param name="targetWidth">目标宽度</param>
    /// <param name="targetHeight">目标高度</param>
    /// <returns>缩放后的图像</returns>
    private SKImage CreateScaledImage(SKImage sourceImage, float targetWidth, float targetHeight)
    {
        // 转换为整数尺寸
        int intTargetWidth = (int)Math.Round(targetWidth);
        int intTargetHeight = (int)Math.Round(targetHeight);

        // 定义目标图像信息
        var targetImageInfo = new SKImageInfo(
            width: intTargetWidth,
            height: intTargetHeight,
            colorType: sourceImage.ColorType,
            alphaType: sourceImage.AlphaType,
            colorspace: sourceImage.ColorSpace
        );

        using var surface = SKSurface.Create(targetImageInfo);
        using var canvas = surface.Canvas;

        // 清空画布
        canvas.Clear(SKColors.Transparent);

        // 绘制并缩放图像
        canvas.DrawImage(sourceImage,
            new SKRect(0, 0, intTargetWidth, intTargetHeight),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

        return surface.Snapshot();
    }

    /// <summary>
    /// 绘制图片
    /// </summary>
    /// <param name="imageInfo"></param>
    /// <returns></returns>
    public MemoryStream Draw(ImageInfo imageInfo)
    {
        using var originalImage = SKImage.FromEncodedData(_inputStream);
        using var surface = SKSurface.Create(originalImage.Info);
        using var canvas = surface.Canvas;

        // 绘制原图
        canvas.DrawImage(originalImage, 0, 0);

        if (imageInfo.Texts != null && imageInfo.Texts.Count > 0)
        {
            foreach (var text in imageInfo.Texts)
            {
                if (!SKColor.TryParse(text.Color, out SKColor color))
                {
                    color = SKColors.Black;
                }
                var paint = new SKPaint()
                {
                    Color = color,
                    IsAntialias = true,
                    IsStroke = false,
                    Style = SKPaintStyle.Fill
                };
                using var font = new SKFont
                {
                    Size = text.Size,
                    Typeface = text.Typeface ?? SKTypeface.FromFamilyName(text.FontFamilyName)
                };
                canvas.DrawText(text.Value, text.Left, text.Top, font, paint);
            }
        }
        if (imageInfo.Image != null)
        {
            using var sourceSubImage = SKImage.FromEncodedData(imageInfo.Image.FilePath);
            //根据imageContentInfo.Image的maxwidth和maxheight属性来调整图片大小
            var (targetSubWidth, targetSubHeight) = CalculateScaledSize(
               sourceWidth: sourceSubImage.Width,
               sourceHeight: sourceSubImage.Height,
               maxWidth: imageInfo.Image.MaxWidth,
               maxHeight: imageInfo.Image.MaxHeight
           );
            //生成高质量缩放后的子图片
            using var scaledSubImage = CreateScaledImage(sourceSubImage, targetSubWidth, targetSubHeight);

            using var imagePaint = new SKPaint();
            imagePaint.IsAntialias = true;
            canvas.DrawImage(scaledSubImage, imageInfo.Image.Left, imageInfo.Image.Top, imagePaint);
        }

        var ms = new MemoryStream();
        using var finalImage = surface.Snapshot();
        finalImage.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
        ms.Seek(0, SeekOrigin.Begin);

        // 强制垃圾回收以释放SkiaSharp资源
        GC.Collect();
        GC.WaitForPendingFinalizers();

        return ms;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _inputStream.Dispose();
    }
}
/// <summary>
/// 图片模板信息类，用于定义图片模板的组成元素，包括文本和图片
/// </summary>
public class ImageInfo
{
    /// <summary>
    /// 文本信息类，用于定义模板中文本的各项属性
    /// </summary>
    public class TextDescription
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 文本左侧位置坐标（相对位置）
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// 文本顶部位置坐标（相对位置）
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        /// 文本字体大小
        /// </summary>
        public float Size { get; set; } = 12;

        /// <summary>
        /// 文本颜色（支持十六进制颜色值，如：#000000）
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// 文本字体名称
        /// </summary>
        public string FontFamilyName { get; set; } = "微软雅黑";

        /// <summary>
        /// 字体文件
        /// </summary>
        internal SKTypeface? Typeface { get; set; }

        /// <summary>
        /// 加载字体文件
        /// </summary>
        /// <param name="fontPath"></param>
        public void LoadFont(string fontPath)
        {
            Typeface = SKTypeface.FromFile(fontPath);
        }
    }

    /// <summary>
    /// 图片信息类，用于定义模板中图片的各项属性
    /// </summary>
    public class ImageDescription
    {
        /// <summary>
        /// 图片文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 图片左侧位置坐标（相对位置）
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// 图片顶部位置坐标（相对位置）
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        /// 图片最大宽度
        /// </summary>
        public int MaxWidth { get; set; } = 50;
        /// <summary>
        /// 图片最大高度
        /// </summary>
        public int MaxHeight { get; set; } = 50;
    }

    /// <summary>
    /// 文本信息列表，用于存储模板中的所有文本元素
    /// </summary>
    public List<TextDescription>? Texts { get; set; }

    /// <summary>
    /// 图片信息，用于存储模板中的图片元素
    /// </summary>
    public ImageDescription? Image { get; set; }

    /// <summary>
    /// 构造函数：使用文本信息初始化模板
    /// </summary>
    /// <param name="textInfo">文本信息对象</param>
    public ImageInfo(TextDescription textInfo)
    {
        Texts = [textInfo];
    }

    /// <summary>
    /// 构造函数：使用文本信息初始化模板
    /// </summary>
    /// <param name="textInfos">文本信息对象</param>
    public ImageInfo(List<TextDescription> textInfos)
    {
        Texts = textInfos;
    }

    /// <summary>
    /// 构造函数：使用图片信息初始化模板
    /// </summary>
    /// <param name="imageInfo">图片信息对象</param>
    public ImageInfo(ImageDescription imageInfo)
    {
        Image = imageInfo;
    }
}
