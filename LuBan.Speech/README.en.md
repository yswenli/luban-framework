[中文](README.md) | English

# LuBan.Speech

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> Minimal speech recognition integration — connect to Speech Recognition API in one line of code. Audio-to-text has never been easier.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.Speech?

Speech recognition is in increasing demand, but integrating with major speech platform APIs is always tedious: OAuth token management, audio format conversion, Base64 encoding, result parsing...

**LuBan.Speech** provides a unified speech recognition interface:
- `ISpeechService` unified interface, supports extending with multiple speech engines
- Factory pattern to create service instances, configuration-driven
- Built-in Speech Recognition implementation (`语音识别服务SpeechService`)
- Automatic OAuth 2.0 token management
- Supports streaming recognition for PCM and other audio formats
- Singleton pattern, globally reusable

## Quick Preview

```csharp
// Create from configuration (automatically reads SpeechConfig from config file)
var speechService = SpeechFactory.Create();

// Or manually specify configuration
var speechService = SpeechFactory.Create(new SpeechConfig
{
    SpeechType = EnumSpeechType.Baidu,
    ClientId = "your-baidu-app-id",
    ClientSecret = "your-baidu-app-secret"
});

// Speech recognition
using var audioStream = File.OpenRead("speech.pcm");
var result = await speechService.RecognitionAsync(audioStream, "pcm");

Console.WriteLine($"Recognition result: {result}");
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| LuBan.Service | Service base class |
| LuBan.Common | Base utility library |
| .NET | 8.0 |

## Installation

```bash
dotnet add package LuBan.Speech
```

## Feature Overview

| Module | Core Class | Description |
|--------|------------|-------------|
| Unified Interface | `ISpeechService` | SetConfig / GetToken / RecognitionAsync |
| Factory | `SpeechFactory` | Create speech service instance based on configuration |
| 语音识别服务 Implementation | `语音识别服务SpeechService` | Speech Recognition (singleton, OAuth token) |
| Configuration | `SpeechConfig` | Speech type, ClientId, ClientSecret |
| Type Enum | `EnumSpeechType` | 语音识别服务, etc. |

## Detailed Usage

### Configuration

```json
{
  "SpeechConfig": {
    "SpeechType": 0,
    "ClientId": "your-baidu-app-id",
    "ClientSecret": "your-baidu-app-secret"
  }
}
```

### Get Token

```csharp
var speechService = SpeechFactory.Create();
var token = await speechService.GetToken();
// token can be used for frontend to directly call Speech Recognition API
```

### Audio Recognition

```csharp
var speechService = SpeechFactory.Create();

// PCM format recognition
using var pcmStream = File.OpenRead("audio.pcm");
var result = await speechService.RecognitionAsync(pcmStream, "pcm");

// Default PCM format
using var stream = GetAudioStream();
var result = await speechService.RecognitionAsync(stream);
```

### Speech Recognition API Parameters

Default parameters when internally calling Speech Recognition API:

| Parameter | Value | Description |
|-----------|-------|-------------|
| format | pcm | Audio format |
| rate | 16000 | Sample rate |
| channel | 1 | Mono channel |
| cuid | GUID | Unique device identifier (auto-generated) |

## Usage Tips

- `语音识别服务SpeechService` uses singleton pattern (`BaseService<语音识别服务SpeechService>`), globally reusable
- Audio stream must be PCM format, 16000Hz sample rate, mono channel. Other formats need conversion first
- `SpeechFactory.Create()` parameterless version reads configuration from `ConfigUtil.Read<SpeechConfig>()`
- Speech Recognition API has audio duration limits (usually no more than 60 seconds); long audio needs to be split
- Speech recognition results are returned as comma-separated strings (multiple candidates)
- Extending with new speech engines only requires implementing the `ISpeechService` interface and registering in `SpeechFactory`

## License

Copyright (c) yswenli. All rights reserved.
