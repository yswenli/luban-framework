[English](README.en.md) | 中文

# LuBan.VideoKit

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 视频处理极简工具 —— 一行代码从视频中提取封面缩略图。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.CloudStorage](../LuBan.CloudStorage/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.VideoKit？

在内容管理平台中，视频上传后自动生成封面图是基本需求。手动处理需要调用 视频处理工具，参数复杂、跨平台兼容性差、错误处理繁琐。

**LuBan.VideoKit** 封装了 视频处理工具 调用，提供极简的视频缩略图提取能力：
- 支持指定截取时间点（精确到秒）
- 可配置输出宽度（高度自动按比例缩放）
- 支持 JPG/PNG 等格式输出
- 内置图片质量控制（`-q:v 2` 高品质）
- 提供两种调用方式：输出到文件路径 或 直接获取字节数组
- 支持从命令行参数解析 视频处理工具 路径

## 快速预览

```csharp
// 设置 FFmpeg 路径（从命令行参数读取）
VideoUtil.SetFFmpegPath(args); // --ffmpeg /usr/bin/ffmpeg

// 方式一：提取封面到文件
VideoUtil.ExtractPoster(
    ffmpegPath: "/usr/bin/ffmpeg",
    videoPath: "/videos/lecture.mp4",
    outputImagePath: "/covers/lecture.jpg",
    screenshotTime: "00:00:03",
    imageWidth: 640,
    imageFormat: "jpg");

// 方式二：直接获取字节数组（使用全局 FFmpegPath）
VideoUtil.ExtractPoster(
    videoPath: "/videos/lecture.mp4",
    screenshotTime: "00:00:01",
    imageContent: out byte[] imageBytes,
    imageWidth: 640,
    imageFormat: "jpg");

// imageBytes 可直接用于上传到云存储
```

## 技术栈

| 组件 | 说明 |
|------|------|
| LuBan.Common | 基础工具库 |
| .NET | 8.0 |

## 安装

```bash
dotnet add package LuBan.VideoKit
```

> 需要系统已安装 视频处理工具。可通过 `VideoUtil.Set视频处理工具Path(args)` 或手动设置 `VideoUtil.视频处理工具Path` 指定路径。

## 功能概览

| 功能 | API | 说明 |
|------|-----|------|
| 设置 视频处理工具 路径 | `VideoUtil.Set视频处理工具Path(args)` | 从命令行参数解析 视频处理工具 路径 |
| 提取封面到文件 | `VideoUtil.ExtractPoster(视频处理工具Path, videoPath, outputPath, ...)` | 指定完整参数提取 |
| 提取封面到内存 | `VideoUtil.ExtractPoster(videoPath, time, out bytes, ...)` | 直接获取字节数组 |

## 详细用法

### 配置 视频处理工具 路径

```csharp
// 方式一：从命令行参数读取
// 启动命令：dotnet app.dll --ffmpeg /usr/local/bin/ffmpeg
VideoUtil.SetFFmpegPath(args);

// 方式二：直接设置
VideoUtil.FFmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe";

// 方式三：如果 FFmpeg 已在系统 PATH 中
VideoUtil.FFmpegPath = "ffmpeg";
```

### 在文件上传中自动生成封面

```csharp
// 与 LuBan.CloudStorage 的 FileHandler 集成
// FileHandler.HandleUploadVideoFileAsync 会自动调用 VideoUtil.ExtractPoster
// 只需确保 VideoUtil.FFmpegPath 已正确设置
```

### 视频处理工具 命令参数说明

内部生成的 视频处理工具 命令：
```
ffmpeg -ss 00:00:03 -i "video.mp4" -vframes 1 -q:v 2 -vf scale=640:-1 -y "output.jpg"
```

| 参数 | 说明 |
|------|------|
| `-ss` | 截图时间点（放在 `-i` 前可快速跳转） |
| `-vframes 1` | 只提取 1 帧 |
| `-q:v 2` | 图片质量（1-31，值越小质量越高） |
| `-vf scale=640:-1` | 宽度 640px，高度自动等比缩放 |
| `-y` | 覆盖已存在的输出文件 |

## 使用提示

- 必须先设置 `VideoUtil.视频处理工具Path`，否则调用会失败
- 如果 视频处理工具 已添加到系统 PATH 环境变量，设置 `视频处理工具Path = "视频处理工具"` 即可
- `screenshotTime` 格式为 `HH:mm:ss`，如 `"00:01:30"` 表示第 1 分 30 秒
- `imageWidth` 设为 0 或负数时不指定缩放，保持原始分辨率
- 输出格式由文件后缀决定（`.jpg`、`.png` 等）
- 内存模式（`out byte[]`）会创建临时文件后读取并删除，适合小文件场景

## 许可证

Copyright (c) yswenli. All rights reserved.
