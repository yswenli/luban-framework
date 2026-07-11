/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.VideoKit
*文件名： VideoUtil
*版本号： V1.0.0.0
*唯一标识：044ee543-e74b-48b9-abcb-d77147f100fa
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/16 16:47:53
*描述：图片处理工具类
*
*=================================================
*修改标记
*修改时间：2025/7/16 16:47:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：图片处理工具类
*
*****************************************************************************/
namespace LuBan.VideoKit;

/// <summary>
/// 图片处理工具类
/// </summary>
public static class VideoUtil
{
    /// <summary>
    /// ffmpeg路径
    /// </summary>
    public static string FFmpegPath { get; set; }

    /// <summary>
    /// 从传入的参数中读取ffmpeg路径    
    /// </summary>
    /// <param name="args">参数示例 --ffmpeg c:/bin/ffmpeg.exe</param>
    /// <returns></returns>
    public static bool SetFFmpegPath(string[] args)
    {
        if (args == null || args.Length < 1)
        {
            FFmpegPath = "ffmpeg";
            return false;
        }
        if (args == null || args.Length < 2) return false;
        FFmpegPath = ArgsUtil.Read<string>(args, "ffmpeg") ?? "ffmpeg";
        return FFmpegPath.IsNotNullOrEmpty();
    }

    /// <summary>
    /// 从视频中提取海报图像
    /// </summary>
    /// <param name="ffmpegPath"></param>
    /// <param name="videoPath"></param>
    /// <param name="outputImagePath"></param>
    /// <param name="screenshotTime"></param>
    /// <param name="imageWidth"></param>
    /// <param name="imageFormat"></param>
    /// <exception cref="Exception"></exception>
    public static void ExtractPoster(
        string ffmpegPath,
        string videoPath,
        string outputImagePath,
        string screenshotTime = "00:00:01",
        int imageWidth = 640,
        string imageFormat = "jpg")
    {
        if (!File.Exists(videoPath))
        {
            throw new Exception($"VideoUtil.ExtractPoster失败：视频文件不存在:{videoPath}");
        }
        var directory = Path.GetDirectoryName(outputImagePath);
        if (string.IsNullOrEmpty(directory))
        {
            throw new Exception($"VideoUtil.ExtractPoster失败：地址不存在:{directory}");
        }
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        try
        {
            // 构造 FFmpeg 命令（关键参数说明）
            // -i: 输入视频文件
            // -ss: 指定截图时间点（放在 -i 前效率更高，直接跳转到目标时间）
            // -vframes: 只提取 1 帧（避免多帧输出）
            // -s: 截图分辨率（如 640x360，若宽度为 0 则不指定，保持原比例）
            // -q:v 2: 图片质量（1-31，值越小质量越高，jpg 推荐 2-5）
            // -y: 覆盖已存在的输出文件（避免交互确认）
            var ffmpegArgs = new List<string>
            {
                "-ss", screenshotTime,  // 截图时间点
                "-i", $"\"{videoPath}\"",  // 输入视频（路径含空格需加引号）
                "-vframes", "1",  // 提取 1 帧
                "-q:v", "2",  // 图片质量
                "-y"  // 覆盖输出文件
            };
            // 若指定宽度，添加分辨率参数（高度自动按比例计算）
            if (imageWidth > 0)
            {
                ffmpegArgs.Add($"-vf scale={imageWidth}:-1");
            }
            // 添加输出路径（格式由路径后缀决定，如 .jpg/.png）
            ffmpegArgs.Add($"\"{outputImagePath}\"");
            var result = ProcessUtil.Start(ffmpegPath, ffmpegArgs, out string output, out string error);
            if (output.IsNotNullOrEmpty())
            {
                Logger.Info(output);
            }
            if (!File.Exists(outputImagePath))
            {
                if (error.IsNotNullOrEmpty())
                {
                    throw new Exception($"VideoUtil.ExtractPoster失败:{error}");
                }
                throw new Exception("VideoUtil.ExtractPoster失败：输出图片文件不存在");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"VideoUtil.ExtractPoster失败", ex);
        }
    }

    /// <summary>
    /// 从视频中提取海报图像
    /// </summary>
    /// <param name="videoPath"></param>
    /// <param name="imageContent"></param>
    /// <param name="screenshotTime"></param>
    /// <param name="imageWidth"></param>
    /// <param name="imageFormat"></param>
    public static void ExtractPoster(
        string videoPath,
        string screenshotTime,
        out byte[] imageContent,
        int imageWidth = 640,
        string imageFormat = "jpg")
    {
        imageContent = [];
        var tempImagePath = Path.Combine(Path.GetDirectoryName(videoPath) ?? "", $"{DateTime.Now.Ticks}.jpeg");
        ExtractPoster(FFmpegPath, videoPath, tempImagePath, screenshotTime, imageWidth, imageFormat);
        if (File.Exists(tempImagePath))
            imageContent = File.ReadAllBytes(tempImagePath);
        FileUtil.Delete(tempImagePath);
    }
}
