/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： VideoUnitTest
*版本号： V1.0.0.0
*唯一标识：c73c224f-1363-4e8e-a98f-9dc7dd7dcdb5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/17 10:46:53
*描述：视频处理工具单元测试类
*
*=================================================
*修改标记
*修改时间：2025/7/17 10:46:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：视频处理工具单元测试类
*
*****************************************************************************/
using LuBan.VideoKit;

namespace LuBan.UnitTestProject;

/// <summary>
/// 视频处理工具单元测试类
/// </summary>
[TestClass]
public class VideoToolUnitTest
{
    /// <summary>
    /// 提取视频海报单元测试
    /// </summary>
    [TestMethod]
    public void TestExtractPoster()
    {
        var ffmpegPath = "C:\\ProgramData\\ZZDS\\ffmpeg\\ffmpeg.exe";
        string videoPath = @"d:\Users\yswenli\Videos\不敢看.mp4"; // 替换为实际视频路径
        string outputPath = @"d:\Users\yswenli\Videos\不敢看.jpeg"; // 替换为实际输出路径
        // Act & Assert
        try
        {
            VideoUtil.ExtractPoster(ffmpegPath, videoPath, outputPath, screenshotTime: "00:00:03");
            Assert.IsTrue(File.Exists(outputPath), "输出文件不存在");
        }
        catch (Exception ex)
        {
            Assert.Fail($"提取海报失败: {ex.Message}");
        }
    }

}
