/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： BarCodeUtil
*版本号： V1.0.0.0
*唯一标识：fdea76c3-c444-4a3f-a365-90a5b7cf8f16
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/9/23 17:51:15
*描述：图形码工具类
*
*=====================================================================
*修改标记
*修改时间：2021/9/23 17:51:15
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：图形码工具类
*
*****************************************************************************/

namespace LuBan.Common;

/// <summary>
/// 图形码工具类
/// </summary>
public static class CodeUtil
{
    /// <summary>
    /// 读取图形码
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="codeType"></param>
    /// <returns></returns>
    public static string? Read(string filePath, EnumCodeType codeType)
    {
        var hints = new Dictionary<DecodeHintType, object>
        {
            { DecodeHintType.CHARACTER_SET, "UTF-8" },
            { DecodeHintType.TRY_HARDER, true }
        };

        ZXing.SkiaSharp.BarcodeReader barcodeReader;

        if (codeType == EnumCodeType.QRCode)
        {
            barcodeReader = new ZXing.SkiaSharp.BarcodeReader
            {
                Options = new DecodingOptions
                {
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                    TryHarder = true
                }
            };
        }
        else
        {
            barcodeReader = new ZXing.SkiaSharp.BarcodeReader
            {
                Options = new DecodingOptions
                {
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128 },
                    TryHarder = true
                }
            };
        }

        // 使用SKImage直接加载图像数据，避免中间的SKBitmap转换
        using var image = SKImage.FromEncodedData(filePath);
        if (image == null) return null;

        // 由于ZXing.SkiaSharp需要SKBitmap，我们使用最小化的转换
        using var bitmap = SKBitmap.FromImage(image);
        if (bitmap == null) return null;

        var result = barcodeReader.Decode(bitmap);
        return result?.Text;
    }

    /// <summary>
    /// 生成图形码
    /// </summary>
    /// <param name="text"></param>
    /// <param name="codeType"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static Stream Write(string text, EnumCodeType codeType, SKSizeI size)
    {
        ZXing.SkiaSharp.BarcodeWriter writer;

        if (codeType == EnumCodeType.QRCode)
        {
            writer = new ZXing.SkiaSharp.BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = size.Height,
                    Width = size.Width,
                    Margin = 1
                }
            };
        }
        else
        {
            writer = new ZXing.SkiaSharp.BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = size.Height,
                    Width = size.Width,
                    Margin = 1,
                    PureBarcode = true
                }
            };
        }

        // 使用ZXing.SkiaSharp生成图形码，然后直接转换为SKImage
        using var bitmap = writer.Write(text);
        using var image = SKImage.FromBitmap(bitmap);

        // 直接编码图像数据到流
        var ms = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    /// <summary>
    /// 生成图形码
    /// </summary>
    /// <param name="text"></param>
    /// <param name="codeType"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Stream Write(string text, EnumCodeType codeType, int width, int height)
    {
        return Write(text, codeType, new SKSizeI(width, height));
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="text"></param>
    /// <param name="codeType"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static string Save(string text, EnumCodeType codeType, int width, int height)
    {
        var filePath = FileUtil.GetCurrentFile($"{DateTimeUtil.Now:yyyyMMddHHmmss}.png");
        using (var fs = FileUtil.GetStream(filePath))
        {
            using (var ms = Write(text, codeType, width, height))
            {
                ms.CopyTo(fs);
            }
        }
        return filePath;
    }
}
