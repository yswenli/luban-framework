/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.PdfKit
*文件名： PdfUtil
*版本号： V1.0.0.0
*唯一标识：42b81ecf-9ea9-4143-88c6-f2bb09f8594a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/8 11:20:43
*描述：pdf工具类
*
*=================================================
*修改标记
*修改时间：2025/8/8 11:20:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：pdf工具类
*
*****************************************************************************/

namespace LuBan.PdfKit;

/// <summary>
/// pdf工具类
/// </summary>
public static class PdfUtil
{
    /// <summary>
    /// 加载字体
    /// </summary>
    /// <param name="fontFilePath"></param>
    /// <returns></returns>
    static PdfFont GetPdfFont(string? fontFilePath = null)
    {
        PdfFont font;
        try
        {
            if (fontFilePath.IsNotNullOrEmpty())
            {
                font = PdfFontFactory.CreateFont(
                        $"{fontFilePath},0",
                        PdfEncodings.IDENTITY_H,
                        PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED, true);
            }
            else
            {

                if (RuntimeUtil.IsWindows())
                    font = PdfFontFactory.CreateFont(
                        "C:/Windows/Fonts/msyh.ttc,0",
                        PdfEncodings.IDENTITY_H,
                        PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED, true);
                else
                    font = PdfFontFactory.CreateFont(
                    "/usr/share/fonts/msyh.ttc,0",
                    PdfEncodings.IDENTITY_H,
                    PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED, true);
            }
        }
        catch
        {
            font = PdfFontFactory.CreateFont();
        }
        return font;
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="inputPath">输入PDF文件路径</param>
    /// <param name="outputPath">输出PDF文件路径</param>
    /// <param name="oldText">要替换的文本</param>
    /// <param name="newText">替换后的文本</param>
    /// <param name="fontFilePath">字体文件路径</param>
    /// <param name="fontSize">字体大小</param>
    /// <param name="offset">字体大小</param>
    public static void ReplaceText(string inputPath, string outputPath, string oldText, string newText, string? fontFilePath = null, float fontSize = 12.0f, PdfRectangle? offset = null)
    {
        using (var reader = new PdfReader(inputPath))
        using (var writer = new PdfWriter(outputPath))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            var font = GetPdfFont(fontFilePath);
            if (offset == null)
            {
                offset = new PdfRectangle(-2, -2, 2, 6);
            }
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                PdfPage page = pdfDoc.GetPage(i);

                // 先提取页面文本，检查是否包含目标文本
                string pageText = PdfTextExtractor.GetTextFromPage(page);

                if (!pageText.Contains(oldText, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                // 使用自定义监听器来获取文本位置
                var listener = new TextLocationListener(oldText);
                PdfTextExtractor.GetTextFromPage(page, listener);
                if (listener.FoundTextChunks.Any())
                {
                    foreach (var chunk in listener.FoundTextChunks)
                    {
                        var canvas = new PdfCanvas(
                                page.NewContentStreamAfter(),
                                page.GetResources(),
                                pdfDoc);
                        // 绘制白色背景遮盖层（稍微扩大一点确保完全覆盖）
                        var left = chunk.Left + offset.Value.Left;
                        var top = chunk.Bottom + offset.Value.Top;
                        var width = chunk.Width + offset.Value.Width;
                        var height = chunk.Height + offset.Value.Height;
                        canvas.SetFillColor(ColorConstants.WHITE)
                            .Rectangle(left, top, width, height)
                            .Fill();

                        // 绘制黑色文本
                        canvas.BeginText()
                            .SetFontAndSize(font, fontSize)
                            .SetTextRenderingMode(PdfCanvasConstants.TextRenderingMode.FILL) // 正常渲染模式
                            .SetFillColor(ColorConstants.BLACK) // 设置文本颜色为黑色
                            .MoveText(chunk.Left, chunk.Bottom)
                            .ShowText(newText)
                            .EndText();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="data"></param>
    /// <param name="fontFilePath">字体文件路径</param>
    /// <param name="fontSize">字体大小</param>
    /// <param name="offset">字体大小</param>
    public static void ReplaceTexts(string inputPath, string outputPath, Dictionary<string, string> data, string? fontFilePath = null, float fontSize = 12.0f, PdfRectangle? offset = null)
    {
        var source = inputPath;
        var filePath = Path.GetDirectoryName(outputPath) ?? "";
        var fileName = Path.GetFileName(inputPath);
        var i = 1;
        var target = Path.Combine(filePath, $"{i}_{fileName}");
        List<string> tempFileList = [];
        foreach (var item in data)
        {
            target = Path.Combine(filePath, $"{i}_{fileName}");
            ReplaceText(source, target, item.Key, item.Value, fontFilePath, fontSize, offset);
            tempFileList.Add(target);
            source = target;
            i++;
        }
        var last = tempFileList.Last();
        tempFileList.Remove(last);
        tempFileList.ForEach((tf) => FileUtil.Delete(tf));
        File.Move(last, outputPath);
    }

    //将指定文本替换成图片

    /// <summary>
    /// 将指定文本替换成图片
    /// </summary>
    /// <param name="inputPath">输入PDF文件路径</param>
    /// <param name="outputPath">输出PDF文件路径</param>
    /// <param name="oldText">要替换的文本</param>
    /// <param name="imagePath">替换后的图片路径</param>
    /// <param name="imageWidth">图片宽度（可选，默认100）</param>
    /// <param name="imageHeight">图片高度（可选，默认100）</param>
    public static void ReplaceTextWithImage(string inputPath, string outputPath, string oldText, string imagePath, float imageWidth = 100, float imageHeight = 100)
    {
        using (var reader = new PdfReader(inputPath))
        using (var writer = new PdfWriter(outputPath))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                PdfPage page = pdfDoc.GetPage(i);

                // 先提取页面文本，检查是否包含目标文本
                string pageText = PdfTextExtractor.GetTextFromPage(page);

                if (!pageText.Contains(oldText, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // 使用自定义监听器来获取文本位置
                var listener = new TextLocationListener(oldText);
                PdfTextExtractor.GetTextFromPage(page, listener);

                if (listener.FoundTextChunks.Any())
                {
                    foreach (var chunk in listener.FoundTextChunks)
                    {
                        var canvas = new PdfCanvas(
                                page.NewContentStreamAfter(),
                                page.GetResources(),
                                pdfDoc);

                        try
                        {
                            // 加载图片
                            var image = iText.IO.Image.ImageDataFactory.Create(imagePath);
                            // 绘制白色背景遮盖层覆盖原文本
                            canvas.SetFillColor(ColorConstants.WHITE)
                                .Rectangle(chunk.Left - 2, chunk.Bottom - 2, chunk.Width + 4, chunk.Height + 4)
                                .Fill();

                            var left = chunk.Left;
                            var top = chunk.Bottom - imageHeight + chunk.Height + 4;

                            // 在指定位置绘制图片
                            canvas.AddImageFittedIntoRectangle(image, new iText.Kernel.Geom.Rectangle(left, top, imageWidth, imageHeight), false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"加载图片失败: {ex.Message}");
                            // 如果图片加载失败，用白色矩形替代
                            canvas.SetFillColor(ColorConstants.WHITE)
                                .Rectangle(chunk.Left - 2, chunk.Bottom - 2, chunk.Width + 4, chunk.Height + 4)
                                .Fill();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 批量将指定文本替换成图片
    /// </summary>
    /// <param name="inputPath">输入PDF文件路径</param>
    /// <param name="outputPath">输出PDF文件路径</param>
    /// <param name="data">替换数据字典，Key为要替换的文本，Value为图片路径</param>
    /// <param name="imageWidth">图片宽度（可选，默认100）</param>
    /// <param name="imageHeight">图片高度（可选，默认100）</param>
    public static void ReplaceTextsWithImages(string inputPath, string outputPath,
        Dictionary<string, string> data, float imageWidth = 100, float imageHeight = 100)
    {
        var source = inputPath;
        var filePath = Path.GetDirectoryName(outputPath) ?? "";
        var fileName = Path.GetFileName(inputPath);
        var i = 1;
        var target = Path.Combine(filePath, $"{i}_{fileName}");
        List<string> tempFileList = [];

        foreach (var item in data)
        {
            target = Path.Combine(filePath, $"{i}_{fileName}");
            ReplaceTextWithImage(source, target, item.Key, item.Value, imageWidth, imageHeight);
            tempFileList.Add(target);
            source = target;
            i++;
        }

        var last = tempFileList.Last();
        tempFileList.Remove(last);
        tempFileList.ForEach((tf) => FileUtil.Delete(tf));
        File.Move(last, outputPath);
    }

    /// <summary>
    /// 替换PDF中的图片
    /// </summary>
    /// <param name="inputPath">输入PDF文件路径</param>
    /// <param name="outputPath">输出PDF文件路径</param>
    /// <param name="oldImagePath">要替换的图片路径</param>
    /// <param name="newImagePath">替换后的图片路径</param>
    /// <param name="imageWidth">图片宽度（可选，默认100）</param>
    /// <param name="imageHeight">图片高度（可选，默认100）</param>
    public static void ReplaceImage(string inputPath, string outputPath,
        string oldImagePath, string newImagePath, float imageWidth = 100, float imageHeight = 100)
    {
        using (var reader = new PdfReader(inputPath))
        using (var writer = new PdfWriter(outputPath))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                PdfPage page = pdfDoc.GetPage(i);

                try
                {
                    // 简化实现：在页面中心位置替换图片
                    var pageSize = page.GetPageSize();
                    var centerX = pageSize.GetLeft() + (pageSize.GetWidth() - imageWidth) / 2;
                    var centerY = pageSize.GetBottom() + (pageSize.GetHeight() - imageHeight) / 2;

                    var canvas = new PdfCanvas(
                        page.NewContentStreamAfter(),
                        page.GetResources(),
                        pdfDoc);

                    // 绘制白色背景遮盖层（覆盖页面中心区域）
                    canvas.SetFillColor(ColorConstants.WHITE)
                        .Rectangle(centerX - 10, centerY - 10, imageWidth + 20, imageHeight + 20)
                        .Fill();

                    // 加载新图片
                    var newImage = iText.IO.Image.ImageDataFactory.Create(newImagePath);

                    // 在指定位置绘制新图片
                    canvas.AddImageFittedIntoRectangle(newImage,
                        new iText.Kernel.Geom.Rectangle(centerX, centerY, imageWidth, imageHeight), false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理页面 {i} 时出错: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 批量替换PDF中的图片
    /// </summary>
    /// <param name="inputPath">输入PDF文件路径</param>
    /// <param name="outputPath">输出PDF文件路径</param>
    /// <param name="data">替换数据字典，Key为要替换的图片路径，Value为新的图片路径</param>
    /// <param name="imageWidth">图片宽度（可选，默认100）</param>
    /// <param name="imageHeight">图片高度（可选，默认100）</param>
    public static void ReplaceImages(string inputPath, string outputPath,
        Dictionary<string, string> data, float imageWidth = 100, float imageHeight = 100)
    {
        var source = inputPath;
        var filePath = Path.GetDirectoryName(outputPath) ?? "";
        var fileName = Path.GetFileName(inputPath);
        var i = 1;
        var target = Path.Combine(filePath, $"{i}_{fileName}");
        List<string> tempFileList = [];

        foreach (var item in data)
        {
            target = Path.Combine(filePath, $"{i}_{fileName}");
            ReplaceImage(source, target, item.Key, item.Value, imageWidth, imageHeight);
            tempFileList.Add(target);
            source = target;
            i++;
        }

        var last = tempFileList.Last();
        tempFileList.Remove(last);
        tempFileList.ForEach((tf) => FileUtil.Delete(tf));
        File.Move(last, outputPath);
    }

    /// <summary>
    /// 从图片生成PDF
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="imageFiles"></param>
    public static void FromImages(string outputPath, List<string> imageFiles)
    {
        using (var writer = new PdfWriter(outputPath))
        using (var pdfDoc = new PdfDocument(writer))
        using (var document = new Document(pdfDoc))
        {
            foreach (var imageFile in imageFiles)
            {
                if (!File.Exists(imageFile)) continue;
                pdfDoc.AddNewPage();
                var image = new Image(iText.IO.Image.ImageDataFactory.Create(imageFile));
                image.ScaleToFit(pdfDoc.GetDefaultPageSize().GetWidth() - 50,
                                 pdfDoc.GetDefaultPageSize().GetHeight() - 50);
                image.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                document.Add(image);
            }
        }
    }


    /// <summary>
    /// 从HTML生成PDF
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="htmlContent"></param>
    /// <param name="fontFilePath"></param>
    public static void FromHtml(string outputPath, string htmlContent, string? fontFilePath = null)
    {
        using (var writer = new PdfWriter(outputPath))
        using (var pdfDoc = new PdfDocument(writer))
        {
            var font = GetPdfFont(fontFilePath);
            var converterProperties = new ConverterProperties();
            var fontProvider = new DefaultFontProvider(false, false, false);
            fontProvider.AddFont(font.GetFontProgram());
            converterProperties.SetFontProvider(fontProvider);

            HtmlConverter.ConvertToPdf(htmlContent, pdfDoc, converterProperties);
        }
    }

    /// <summary>
    /// 从HTML生成PDF
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="url"></param>
    /// <param name="fontFilePath"></param>
    public static void FromUrl(string outputPath, string url, string? fontFilePath = null)
    {
        var htmlContent = Encoding.UTF8.GetString(HttpClientProxy.DownloadBytes(url));
        using (var writer = new PdfWriter(outputPath))
        using (var pdfDoc = new PdfDocument(writer))
        {
            var font = GetPdfFont(fontFilePath);
            var converterProperties = new ConverterProperties();
            var fontProvider = new DefaultFontProvider(false, false, false);
            fontProvider.AddFont(font.GetFontProgram());
            converterProperties.SetFontProvider(fontProvider);
            converterProperties.SetBaseUri(url);
            HtmlConverter.ConvertToPdf(htmlContent, pdfDoc, converterProperties);
        }
    }


}
