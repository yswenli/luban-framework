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