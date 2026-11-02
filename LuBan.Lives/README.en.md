[中文](README.md) | English

# LuBan.Lives

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> Unified multi-platform live streaming SDK — integrate four major live streaming platforms with a single codebase: TalkMed, VZan, YiBai, and HuiChang (a Vhall client also ships but is not wired into the factory).

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.Lives?

Enterprise live streaming scenarios often require integration with multiple platforms: Client A uses TalkMed, Client B uses VZan, internal meetings use HuiChang... Each platform has different APIs, making switching extremely costly.

**LuBan.Lives** abstracts platform differences through a unified `ILiveClient` interface:
- Factory pattern to create live streaming clients, switch platforms via enum
- Unified configuration model `LiveOption` to manage AppId, AppSecret, authorization URLs, etc.
- Base class `BaseLiveClient` encapsulates common HTTP requests (GET/POST/DELETE)
- Multi-platform support: TalkMed, VZan, YiBai, HuiChang; the Vhall client (`VHLiveClient`) is implemented but not added to `EnumLive` / `LiveFactory`

## Quick Preview

```csharp
// Create TalkMed live streaming client
var liveOption = new LiveOption
{
    AppId = "your-app-id",
    AppSecret = "your-app-secret",
    Url = "https://apimeeting.talkmed.com",
    AuthorizeUrl = "https://meeting.talkmed.com",   // the client appends /oauth/authorize itself
    AuthSecret = "your-auth-secret",
    UserName = "admin",
    Password = "password",
    Salt = "your-salt"
};

var liveClient = LiveFactory.Create(EnumLive.TalkMed, liveOption);

// Build a viewing URL (TalkMed's GetLiveUrl is not implemented; use Authorize)
var liveUrl = ((TMLiveClient)liveClient).Authorize(
    authToken: "user-auth-token",
    roomId: "room-001",
    role: 3);
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| LuBan.Common | Base utility library (HTTP client, serialization, etc.) |
| .NET | net10.0 |

## Installation

```bash
dotnet add package LuBan.Lives
```

## Feature Overview

| Module | Core Class | Description |
|--------|------------|-------------|
| Unified Interface | `ILiveClient` | `GetLiveUrl` returns a viewing URL (not implemented for TalkMed and Vhall — throws `NotImplementedException`) |
| Factory | `LiveFactory` | Create platform-specific client based on enum |
| Base Class | `BaseLiveClient` | Encapsulates common HTTP requests and auth headers |
| TalkMed | `TMLiveClient` | TalkMed platform adapter |
| VZan | `VZLiveClient` | VZan platform adapter |
| YiBai | `YBLiveClient` | YiBai platform adapter |
| HuiChang | `HCLiveClient` | HuiChang platform adapter |
| Configuration | `LiveOption` | AppId, AppSecret, Url, authorization info, etc. |
| Platform Enum | `EnumLive` | TalkMed / YiBai / HuiChang / VZan (Vhall excluded) |

## Detailed Usage

### Configuration

```csharp
var liveOption = new LiveOption
{
    AppId = "tk62a982fd03ebf",
    AppSecret = "00e4037ccf189c4bbb16b22426359ee7",
    Url = "https://apimeeting.talkmed.com",
    AuthorizeUrl = "https://meeting.talkmed.com",   // the client appends /oauth/authorize itself
    AuthSecret = "auth-secret-value",               // required (non-empty) by the VZan and Vhall clients
    UserName = "admin",
    Password = "password",
    Salt = "salt-value"
};
```

### Switch Platforms

```csharp
// TalkMed
var tmClient = LiveFactory.Create(EnumLive.TalkMed, liveOption);

// VZan (validates AppId/AppSecret/AuthSecret/Url at construction; throws if any is missing)
var vzClient = LiveFactory.Create(EnumLive.VZan, liveOption);

// YiBai
var ybClient = LiveFactory.Create(EnumLive.YiBai, liveOption);

// HuiChang
var hcClient = LiveFactory.Create(EnumLive.HuiChang, liveOption);
```

### Get Live Streaming URL

```csharp
// HuiChang / YiBai / VZan: GetLiveUrl is implemented
var url = LiveFactory.Create(EnumLive.YiBai, liveOption).GetLiveUrl(
    channelId: "room-001",
    secret: "room-secret",
    userId: "user-1001",
    name: "Speaker",
    avatar: "https://cdn.example.com/avatar.jpg");

// TalkMed: GetLiveUrl throws NotImplementedException — use Authorize to build the viewing URL
var tmUrl = ((TMLiveClient)LiveFactory.Create(EnumLive.TalkMed, liveOption))
    .Authorize(authToken: "user-auth-token", roomId: "room-001", role: 3);

// Return the url to the frontend for embedding the live player
```

## Usage Tips

- `LiveFactory.Create(EnumLive)` without configuration reads the `LiveOption` node from the Nacos config center — configure the platform parameters first
- `BaseLiveClient` has built-in `HttpClientProxy` that automatically manages HTTP connections and logging
- Each platform has different authentication methods; `GetBaseHeaders()` is implemented by subclasses to adapt to their respective auth mechanisms
- `LiveOption.Url` is the API base URL; different platforms have different production/test environment URLs
- When `LiveFactory.Create(EnumLive)` has no match, it defaults to returning `TMLiveClient`

## License

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
