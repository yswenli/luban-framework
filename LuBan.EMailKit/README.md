[English](README.en.md) | 中文
# LuBan.E邮件处理库

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 邮件收发一站式套件 —— SMTP 发送、IMAP/POP3 收取，统一接口轻松搞定。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.E邮件处理库？

发邮件谁都会，但写好却不容易：HTML 正文、附件处理、抄送密送、URL 附件下载、多账户切换…… 每次都要和底层邮件协议 API 搏斗？

**LuBan.E邮件处理库** 封装了简洁的邮件收发接口：
- 工厂模式创建客户端，SMTP/IMAP/POP3 三种协议按需切换
- 多种 `SendAsync` 重载，从简单单收件人到完整抄送密送+附件一应俱全
- 支持 URL 附件（自动从远程下载）
- 完整的邮件模型：`Message`、`Header`、`UserAddress`、`Attachment`
- 配置驱动，支持从 配置中心读取多组邮件账户

## 快速预览

```csharp
// 从配置创建（自动读取 Nacos 中的 EMailOptions 第一组配置）
using var client = EMailFactory.Create();

// 简单发送
await client.SendAsync("user@example.com", "测试邮件", "<h1>你好</h1>", isHtml: true);

// 完整发送
await client.SendAsync(
    to: new List<(string, string)> { ("张三", "zhangsan@example.com") },
    cc: new List<(string, string)> { ("李四", "lisi@example.com") },
    bcc: null,
    subject: "月度报告",
    body: "<p>请查收附件</p>",
    isHtml: true,
    attachments: new List<Attachment>
    {
        Attachment.Create("C:/reports/monthly.pdf")
    });

// 使用 Message 对象发送
var message = new Message(
    new List<(string, string)> { ("王五", "wangwu@example.com") },
    null, null, "通知", "内容", false, null);
await client.SendAsync(message);

// 接收邮件（IMAP/POP3）
var messages = await client.RecieveAsync();
```

## 安装

```bash
dotnet add package LuBan.EMailKit
```

## 功能概览

| 功能模块 | 核心类 | 说明 |
|---------|--------|------|
| 统一接口 | `IEMailClient` | RecieveAsync / SendAsync |
| 工厂 | `EMailFactory` | 根据配置创建对应协议的客户端 |
| SMTP 发送 | `EMailSmtp` | 基于 SMTP 协议的邮件发送 |
| IMAP 收取 | `EmailImap` | 基于 IMAP 协议的邮件接收 |
| POP3 收取 | `EMailPop3` | 基于 POP3 协议的邮件接收 |
| 邮件模型 | `Message` | 邮件头、正文、附件的完整封装 |
| 邮件头 | `Header` | 收件人、抄送、密送、主题 |
| 地址 | `UserAddress` | 姓名 + 邮箱地址 |
| 附件 | `Attachment` | 支持文件路径、Stream、字节数组创建 |
| 配置 | `EMailClientConfig` / `EMailOptions` | 主机、端口、SSL、凭证 |

## 详细用法

### 配置

```json
{
  "EMailOptions": {
    "EMailClientConfigs": [
      {
        "ClientType": 0,
        "Host": "smtp.example.com",
        "Port": 465,
        "UseSsl": true,
        "UserName": "noreply@example.com",
        "Password": "your-password"
      }
    ]
  }
}
```

`ClientType` 枚举值：`0` = SMTP，`1` = IMAP，`2` = POP3

### 手动创建客户端

```csharp
var config = new EMailClientConfig
{
    ClientType = EnumClientType.SMTP,
    Host = "smtp.gmail.com",
    Port = 587,
    UseSsl = true,
    UserName = "your@gmail.com",
    Password = "app-password"
};

using var client = EMailFactory.Create(config);
await client.SendAsync("recipient@example.com", "Hello", "World");
```

### 附件处理

```csharp
// 从本地文件创建
var att1 = Attachment.Create("C:/docs/report.pdf");

// 从 Stream 创建
var att2 = Attachment.Create("data.csv", csvStream);

// 从 URL 自动下载附件
await client.SendAsync("user@example.com", "报告", "请查收",
    isHtml: false,
    attachmentUrls: new Dictionary<string, string>
    {
        { "report.pdf", "https://storage.example.com/reports/monthly.pdf" }
    });
```

## 使用提示

- `IEMailClient` 实现了 `IDisposable`，使用完毕后请调用 `Dispose()` 或使用 `using` 语句
- SMTP 客户端不支持 `RecieveAsync`，接收邮件请使用 IMAP 或 POP3 客户端
- `EMailFactory.Create()` 无参版本自动读取 Nacos 配置中的第一组 `EMailClientConfigs`
- 附件支持从远程 URL 自动下载，适用于云存储中的文件作为附件发送
- 发送 HTML 邮件时设置 `isHtml: true`，正文中的 HTML 标签将被正确渲染

## 许可证

Copyright (c) yswenli. All rights reserved.
