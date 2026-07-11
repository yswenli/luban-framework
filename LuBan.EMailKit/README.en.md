[中文](README.md) | English
# LuBan.E邮件处理库

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> A one-stop email suite — SMTP sending, IMAP/POP3 receiving, all through a unified interface.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.E邮件处理库?

Sending emails is easy, but doing it right is hard: HTML bodies, attachments, CC/BCC, URL attachments, multi-account switching... Tired of wrestling with low-level email protocol APIs every time?

**LuBan.E邮件处理库** provides a clean email send/receive interface:
- Factory pattern client creation — switch between SMTP/IMAP/POP3 protocols as needed
- Multiple `SendAsync` overloads — from simple single-recipient to full CC/BCC + attachments
- URL attachment support (automatically downloads from remote)
- Complete email model: `Message`, `Header`, `UserAddress`, `Attachment`
- Configuration-driven, supports reading multiple email account configs from Nacos config center

## Quick Preview

```csharp
// Create from configuration (automatically reads the first EMailOptions config from Nacos)
using var client = EMailFactory.Create();

// Simple send
await client.SendAsync("user@example.com", "Test Email", "<h1>Hello</h1>", isHtml: true);

// Full send
await client.SendAsync(
    to: new List<(string, string)> { ("Zhang San", "zhangsan@example.com") },
    cc: new List<(string, string)> { ("Li Si", "lisi@example.com") },
    bcc: null,
    subject: "Monthly Report",
    body: "<p>Please find the attachment</p>",
    isHtml: true,
    attachments: new List<Attachment>
    {
        Attachment.Create("C:/reports/monthly.pdf")
    });

// Send using Message object
var message = new Message(
    new List<(string, string)> { ("Wang Wu", "wangwu@example.com") },
    null, null, "Notification", "Content", false, null);
await client.SendAsync(message);

// Receive emails (IMAP/POP3)
var messages = await client.RecieveAsync();
```

## Installation

```bash
dotnet add package LuBan.EMailKit
```

## Feature Overview

| Module | Core Class | Description |
|--------|------------|-------------|
| Unified Interface | `IEMailClient` | RecieveAsync / SendAsync |
| Factory | `EMailFactory` | Creates protocol-specific clients based on configuration |
| SMTP Sending | `EMailSmtp` | Email sending via SMTP protocol |
| IMAP Receiving | `EmailImap` | Email receiving via IMAP protocol |
| POP3 Receiving | `EMailPop3` | Email receiving via POP3 protocol |
| Email Model | `Message` | Complete encapsulation of email header, body, and attachments |
| Email Header | `Header` | Recipients, CC, BCC, subject |
| Address | `UserAddress` | Name + email address |
| Attachment | `Attachment` | Supports creation from file path, Stream, or byte array |
| Configuration | `EMailClientConfig` / `EMailOptions` | Host, port, SSL, credentials |

## Detailed Usage

### Configuration

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

`ClientType` enum values: `0` = SMTP, `1` = IMAP, `2` = POP3

### Manual Client Creation

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

### Attachment Handling

```csharp
// Create from local file
var att1 = Attachment.Create("C:/docs/report.pdf");

// Create from Stream
var att2 = Attachment.Create("data.csv", csvStream);

// Auto-download attachment from URL
await client.SendAsync("user@example.com", "Report", "Please check",
    isHtml: false,
    attachmentUrls: new Dictionary<string, string>
    {
        { "report.pdf", "https://storage.example.com/reports/monthly.pdf" }
    });
```

## Tips

- `IEMailClient` implements `IDisposable` — call `Dispose()` or use `using` statement when done
- SMTP clients do not support `RecieveAsync` — use IMAP or POP3 clients for receiving emails
- `EMailFactory.Create()` parameterless version automatically reads the first `EMailClientConfigs` entry from Nacos configuration
- Attachments support auto-download from remote URLs — useful for sending cloud-stored files as attachments
- Set `isHtml: true` when sending HTML emails — HTML tags in the body will be rendered correctly

## License

Copyright (c) yswenli. All rights reserved.
