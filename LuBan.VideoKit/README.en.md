[中文](README.md) | English

# LuBan.VideoKit

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> Ultra-simple video processing — extract cover thumbnails from videos in one line of code.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.CloudStorage](../LuBan.CloudStorage/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.VideoKit?

In content management platforms, auto-generating cover images after video upload is a basic requirement. Manual processing requires calling video processing tool, with complex parameters, poor cross-platform compatibility, and tedious error handling.

**LuBan.VideoKit** wraps video processing calls to provide ultra-simple video thumbnail extraction:
- Supports specifying the capture time point (accurate to seconds)
- Configurable output width (height auto-scales proportionally)
- Supports JPG/PNG and other format outputs
- Built-in image quality control (`-q:v 2` high quality)
- Provides two invocation methods: output to file path or directly get byte array
- Supports parsing 视频处理工具 path from command-line arguments

## Quick Preview

```csharp
// Set FFmpeg path (read from command-line arguments)
VideoUtil.SetFFmpegPath(args); // --ffmpeg /usr/bin/ffmpeg

// Method 1: Extract cover to file
VideoUtil.ExtractPoster(
    ffmpegPath: "/usr/bin/ffmpeg",
    videoPath: "/videos/lecture.mp4",
    outputImagePath: "/covers/lecture.jpg",
    screenshotTime: "00:00:03",
    imageWidth: 640,
    imageFormat: "jpg");

// Method 2: Directly get byte array (using global FFmpegPath)
VideoUtil.ExtractPoster(
    videoPath: "/videos/lecture.mp4",
    screenshotTime: "00:00:01",
    imageContent: out byte[] imageBytes,
    imageWidth: 640,
    imageFormat: "jpg");

// imageBytes can be directly used for uploading to cloud storage
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| LuBan.Common | Base utility library |
| .NET | 8.0 |

## Installation

```bash
dotnet add package LuBan.VideoKit
```

> 视频处理工具 must be installed on the system. You can specify the path via `VideoUtil.Set视频处理工具Path(args)` or manually set `VideoUtil.视频处理工具Path`.

## Feature Overview

| Feature | API | Description |
|---------|-----|-------------|
| Set 视频处理工具 path | `VideoUtil.Set视频处理工具Path(args)` | Parse 视频处理工具 path from command-line arguments |
| Extract cover to file | `VideoUtil.ExtractPoster(视频处理工具Path, videoPath, outputPath, ...)` | Extract with full parameters |
| Extract cover to memory | `VideoUtil.ExtractPoster(videoPath, time, out bytes, ...)` | Directly get byte array |

## Detailed Usage

### Configure 视频处理工具 Path

```csharp
// Method 1: Read from command-line arguments
// Startup command: dotnet app.dll --ffmpeg /usr/local/bin/ffmpeg
VideoUtil.SetFFmpegPath(args);

// Method 2: Set directly
VideoUtil.FFmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe";

// Method 3: If FFmpeg is already in system PATH
VideoUtil.FFmpegPath = "ffmpeg";
```

### Auto-generate Cover During File Upload

```csharp
// Integrate with LuBan.CloudStorage's FileHandler
// FileHandler.HandleUploadVideoFileAsync will automatically call VideoUtil.ExtractPoster
// Just make sure VideoUtil.FFmpegPath is set correctly
```

### 视频处理工具 Command Parameters

Internally generated 视频处理工具 command:
```
ffmpeg -ss 00:00:03 -i "video.mp4" -vframes 1 -q:v 2 -vf scale=640:-1 -y "output.jpg"
```

| Parameter | Description |
|-----------|-------------|
| `-ss` | Screenshot time point (placed before `-i` for fast seeking) |
| `-vframes 1` | Extract only 1 frame |
| `-q:v 2` | Image quality (1-31, lower value = higher quality) |
| `-vf scale=640:-1` | Width 640px, height auto-scales proportionally |
| `-y` | Overwrite existing output file |

## Usage Tips

- `VideoUtil.视频处理工具Path` must be set first, otherwise calls will fail
- If 视频处理工具 is added to the system PATH environment variable, just set `视频处理工具Path = "视频处理工具"`
- `screenshotTime` format is `HH:mm:ss`, e.g., `"00:01:30"` means 1 minute 30 seconds
- Setting `imageWidth` to 0 or negative disables scaling, keeping original resolution
- Output format is determined by file extension (`.jpg`, `.png`, etc.)
- Memory mode (`out byte[]`) creates a temporary file then reads and deletes it, suitable for small file scenarios

## License

Copyright (c) yswenli. All rights reserved.
