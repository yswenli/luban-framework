/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： TestImageFactory
*版本号： V1.0.0.0
*唯一标识：d7824460-3826-4e37-b483-d173e6f5e6bd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/18 10:41:54
*描述：测试用图片构造辅助（SkiaSharp 生成样本图）
*
*=================================================
*修改标记
*修改时间：2026/9/18 10:41:54
*修改人： yswenli
*版本号： V1.0.0.0
*描述：测试用图片构造辅助（SkiaSharp 生成样本图）
*
*****************************************************************************/

using SkiaSharp;

namespace LuBan.XTestProject;

internal static class TestImageFactory
{
    public static string CreatePng(int width, int height)
        => Create(width, height, ".png", SKEncodedImageFormat.Png);

    public static string CreateWebp(int width, int height)
        => Create(width, height, ".webp", SKEncodedImageFormat.Webp);

    private static string Create(int width, int height, string extension, SKEncodedImageFormat format)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.CornflowerBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 100);
        using var fs = File.Create(path);
        data.SaveTo(fs);
        return path;
    }
}