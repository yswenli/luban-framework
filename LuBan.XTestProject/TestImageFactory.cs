using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace LuBan.XTestProject;

internal static class TestImageFactory
{
    public static string CreatePng(int width, int height)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
        using var image = new Image<Rgba32>(width, height);
        image.Save(path, new PngEncoder());
        return path;
    }

    public static string CreateGif(int width, int height)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".gif");
        using var image = new Image<Rgba32>(width, height);
        image.Save(path, new GifEncoder());
        return path;
    }
}