[English](README.en.md) | 中文

# LuBan.Lives

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 多平台直播 SDK 统一接口 —— 拓麦、微赞、一百、会畅，一套代码对接四大直播平台。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.Lives？

企业直播场景经常需要对接多个直播平台：客户 A 用拓麦，客户 B 用微赞，内部会议用会畅…… 每个平台的 API 各不相同，切换成本极高。

**LuBan.Lives** 通过统一的 `ILiveClient` 接口屏蔽平台差异：
- 工厂模式创建直播客户端，枚举切换平台
- 统一配置模型 `LiveOption`，管理 AppId、AppSecret、授权地址等
- 基类 `BaseLiveClient` 封装通用 HTTP 请求（GET/POST/DELETE）
- 支持多平台：拓麦（TalkMed）、微赞（VZan）、一百（YiBai）、会畅（HuiChang）

## 快速预览

```csharp
// 创建拓麦直播客户端
var liveOption = new LiveOption
{
    AppId = "your-app-id",
    AppSecret = "your-app-secret",
    Url = "https://apimeeting.talkmed.com",
    AuthorizeUrl = "https://meeting.talkmed.com/oauth/authorize",
    UserName = "admin",
    Password = "password",
    Salt = "your-salt"
};

var liveClient = LiveFactory.Create(EnumLive.TalkMed, liveOption);

// 获取直播地址
var liveUrl = liveClient.GetLiveUrl(
    channelId: "channel-001",
    secret: "channel-secret",
    userId: "user-1001",
    name: "张三",
    avatar: "https://example.com/avatar.jpg");
```

## 技术栈

| 组件 | 说明 |
|------|------|
| LuBan.Common | 基础工具库（HTTP 客户端、序列化等） |
| .NET | 8.0 |

## 安装

```bash
dotnet add package LuBan.Lives
```

## 功能概览

| 功能模块 | 核心类 | 说明 |
|---------|--------|------|
| 统一接口 | `ILiveClient` | `GetLiveUrl` 获取直播地址 |
| 工厂 | `LiveFactory` | 根据枚举创建对应平台客户端 |
| 基类 | `BaseLiveClient` | 封装通用 HTTP 请求和认证头 |
| 拓麦直播 | `TMLiveClient` | TalkMed 平台适配 |
| 微赞直播 | `VZLiveClient` | 微赞平台适配 |
| 一百直播 | `YBLiveClient` | 一百直播平台适配 |
| 会畅直播 | `HCLiveClient` | 会畅直播平台适配 |
| 配置 | `LiveOption` | AppId、AppSecret、Url、授权信息等 |
| 平台枚举 | `EnumLive` | TalkMed / YiBai / HuiChang / VZan |

## 详细用法

### 配置

```csharp
var liveOption = new LiveOption
{
    AppId = "tk62a982fd03ebf",
    AppSecret = "00e4037ccf189c4bbb16b22426359ee7",
    Url = "https://apimeeting.talkmed.com",
    AuthorizeUrl = "https://meeting.talkmed.com/oauth/authorize",
    UserName = "admin",
    Password = "password",
    Salt = "salt-value"
};
```

### 切换平台

```csharp
// 拓麦
var tmClient = LiveFactory.Create(EnumLive.TalkMed, liveOption);

// 微赞
var vzClient = LiveFactory.Create(EnumLive.VZan, liveOption);

// 一百
var ybClient = LiveFactory.Create(EnumLive.YiBai, liveOption);

// 会畅
var hcClient = LiveFactory.Create(EnumLive.HuiChang, liveOption);
```

### 获取直播地址

```csharp
var url = liveClient.GetLiveUrl(
    channelId: "room-001",
    secret: "room-secret",
    userId: "user-1001",
    name: "演讲者",
    avatar: "https://cdn.example.com/avatar.jpg");

// 将 url 返回给前端用于嵌入直播播放器
```

## 使用提示

- `LiveFactory.Create(EnumLive)` 无配置版本使用默认构造，需确保各客户端内部有默认配置逻辑
- `BaseLiveClient` 内置 `HttpClientProxy`，自动管理 HTTP 连接和日志
- 各平台的认证方式不同，`GetBaseHeaders()` 由子类实现以适配各自的鉴权机制
- `LiveOption.Url` 为 API 基础地址，不同平台的正式/测试环境地址不同
- 默认 `LiveFactory.Create(EnumLive)` 未匹配时返回 `TMLiveClient`

## 许可证

Copyright (c) yswenli. All rights reserved.
