[English](README.en.md) | 中文

# LuBan.Speech

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 语音识别极简集成 —— 语音识别 API 一行代码接入，音频流转文字如此简单。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.Speech？

语音识别功能需求越来越多，但对接各大语音平台 API 总是繁琐的：OAuth 令牌管理、音频格式转换、Base64 编码、结果解析……

**LuBan.Speech** 提供了统一的语音识别接口：
- `ISpeechService` 统一接口，支持扩展多种语音引擎
- 工厂模式创建服务实例，配置驱动
- 内置语音识别实现（`语音识别服务SpeechService`）
- 自动管理 OAuth 2.0 令牌获取
- 支持 PCM 等音频格式的流式识别
- 单例模式，全局复用

## 快速预览

```csharp
// 从配置创建（自动读取配置文件中的 SpeechConfig）
var speechService = SpeechFactory.Create();

// 或手动指定配置
var speechService = SpeechFactory.Create(new SpeechConfig
{
    SpeechType = EnumSpeechType.Baidu,
    ClientId = "your-baidu-app-id",
    ClientSecret = "your-baidu-app-secret"
});

// 语音识别
using var audioStream = File.OpenRead("speech.pcm");
var result = await speechService.RecognitionAsync(audioStream, "pcm");

Console.WriteLine($"识别结果: {result}");
```

## 技术栈

| 组件 | 说明 |
|------|------|
| LuBan.Service | 服务基类 |
| LuBan.Common | 基础工具库 |
| .NET | 8.0 |

## 安装

```bash
dotnet add package LuBan.Speech
```

## 功能概览

| 功能模块 | 核心类 | 说明 |
|---------|--------|------|
| 统一接口 | `ISpeechService` | SetConfig / GetToken / RecognitionAsync |
| 工厂 | `SpeechFactory` | 根据配置创建语音服务实例 |
| 语音识别服务实现 | `语音识别服务SpeechService` | 语音识别（单例，OAuth 令牌） |
| 配置 | `SpeechConfig` | 语音类型、ClientId、ClientSecret |
| 类型枚举 | `EnumSpeechType` | 语音识别服务 等 |

## 详细用法

### 配置

```json
{
  "SpeechConfig": {
    "SpeechType": 0,
    "ClientId": "your-baidu-app-id",
    "ClientSecret": "your-baidu-app-secret"
  }
}
```

### 获取令牌

```csharp
var speechService = SpeechFactory.Create();
var token = await speechService.GetToken();
// token 可用于前端直接调用语音识别 API
```

### 音频识别

```csharp
var speechService = SpeechFactory.Create();

// PCM 格式识别
using var pcmStream = File.OpenRead("audio.pcm");
var result = await speechService.RecognitionAsync(pcmStream, "pcm");

// 默认 PCM 格式
using var stream = GetAudioStream();
var result = await speechService.RecognitionAsync(stream);
```

### 语音识别 API 参数说明

内部调用语音识别 API 时的默认参数：

| 参数 | 值 | 说明 |
|------|-----|------|
| format | pcm | 音频格式 |
| rate | 16000 | 采样率 |
| channel | 1 | 单声道 |
| cuid | GUID | 设备唯一标识（自动生成） |

## 使用提示

- `语音识别服务SpeechService` 为单例模式（`BaseService<语音识别服务SpeechService>`），全局复用
- 音频流需为 PCM 格式，16000Hz 采样率，单声道。其他格式需先转换
- `SpeechFactory.Create()` 无参版本从 `ConfigUtil.Read<SpeechConfig>()` 读取配置
- 语音识别 API 对音频时长有限制（通常不超过 60 秒），长音频需分段处理
- 语音识别结果以逗号分隔的字符串返回（多个候选结果）
- 扩展新语音引擎只需实现 `ISpeechService` 接口并在 `SpeechFactory` 中注册

## 许可证

Copyright (c) yswenli. All rights reserved.
