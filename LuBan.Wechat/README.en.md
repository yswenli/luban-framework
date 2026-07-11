[中文](README.md) | English

# LuBan.Wechat

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **WeChat Official Account, Mini Program, WeCom, WeChat Pay — handle the entire WeChat ecosystem with a single factory class.**

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.Wechat?

Integrating with the WeChat ecosystem involves three completely different SDKs: Official Account API, WeCom API, and WeChat Pay V3, plus tedious details like callback signature verification, XML message parsing, and AES encryption/decryption.  
LuBan.Wechat wraps a unified client factory and complete message protocol models, enabling you to complete full WeChat scenario integration with minimal code.

---

## Quick Preview

```csharp
// Get WeChat Official Account client
var client = WechatClientFactory.Create(EnumWechatType.Api);

// Get WeChat Pay client
var payClient = WechatClientFactory.Create(EnumWechatType.Pay);

// Get WeCom client
var corpClient = WechatClientFactory.Create(EnumWechatType.Corp);
```

---

## Tech Stack & Dependencies

| Category | Component | Description |
|----------|-----------|-------------|
| Runtime | .NET 8.0 | — |
| Framework Dependencies | LuBan.Web.Core, LuBan.Service, LuBan.Common | LuBan framework core |

---

## Installation

```bash
dotnet add package LuBan.Wechat
```

---

## Feature Overview

### Client Factory

`WechatClientFactory` caches client instances using `ConcurrentDictionary`, creating each type only once:

| Method | Description |
|--------|-------------|
| `Create(EnumWechatType)` | Get client by type (Api/Pay/App/Corp) |
| `CreateWechatApiClient(options)` | Create Official Account client |
| `CreateWechatAppClient(options)` | Create WeChat App client |
| `CreateWechatTenpayClient(options)` | Create WeChat Pay client |
| `CreateWechatCorpClient(options)` | Create WeCom client |

### Callback Handling

| Class | Description |
|-------|-------------|
| `WechatCallBackCall` | Official Account callback: signature verification, XML message parsing, reply construction |
| `WechatCorpCallBackCall` | WeCom callback handler |

### Message Protocol Models

Complete WeChat message send/receive models (29 model classes):

**Receive Messages:**
- `ReceiveText` — Text message
- `ReceiveImage` — Image message
- `ReceiveVoice` — Voice message
- `ReceiveVideo` — Video message
- `ReceiveLocation` — Location message
- `ReceiveLink` — Link message
- `ReceiveEvent` — Event message

**Reply Messages:**
- `ReplyText` — Reply text
- `ReplyImage` — Reply image
- `ReplyVoice` — Reply voice
- `ReplyVideo` — Reply video
- `ReplyMusic` — Reply music
- `ReplyNews` — Reply news (article)

### WeCom

| Class | Description |
|-------|-------------|
| `WechatCorpClient` | WeCom client, contains `WechatWorkClient` and `WechatCorpSuiteClient` |
| `IWechatCorpClient` | WeCom client interface |
| `WechatCorpSignature` | WeCom signature utility |
| `WechatBizMsgCryptor` | WeCom message encryption/decryption |
| `CryptographyUtil` | Cryptography utility |

### Other Utilities

| Class | Description |
|-------|-------------|
| `WechatClientInterceptor` | HTTP request interceptor with automatic logging |
| `XmlResolve` | XML message parsing utility |
| `TencentLocationServiceClient` | Tencent Location Service client |

---

## Code Examples

### Handle Official Account Callback

```csharp
public class WechatController : BaseApiController
{
    // GET - Verify signature
    [HttpGet("wechat/callback")]
    public IActionResult Verify(string signature, string timestamp, string nonce, string echostr)
    {
        var input = new ReceiveInput
        {
            token = "your_token",
            signature = signature,
            timestamp = timestamp,
            nonce = nonce
        };
        var call = new WechatCallBackCall(input);
        if (call.AccessValid())
            return Content(echostr);
        return Content("Verification failed");
    }

    // POST - Receive messages
    [HttpPost("wechat/callback")]
    public async Task<IActionResult> ReceiveMessage()
    {
        var bytes = await WebApp.HttpContext.GetRequestBodyBytesAsync();
        var result = WechatCallBackCall.XmlMsgRevolve(bytes, out string xml, out Receive receive);

        if (receive is ReceiveText textMsg)
        {
            // Reply with text message
            var reply = new ReplyText
            {
                ToUserName = textMsg.FromUserName,
                FromUserName = textMsg.ToUserName,
                Content = $"You said: {textMsg.Content}"
            };
            return Content(WechatCallBackCall.Reply(reply), "application/xml");
        }

        return Content("success");
    }
}
```

### Generate JS-SDK Signature

```csharp
var signature = WechatCallBackCall.Signature(
    jsapi_ticket: "your_ticket",
    url: "https://example.com/page",
    out string noncestr,
    out long timestamp
);
```

### Call WeChat API

```csharp
var client = WechatClientFactory.Create(EnumWechatType.Api) as WechatApiClient;

// Get user info
var request = new CreateWechatAccessTokenRequest();
var response = await client.ExecuteAsync(request);
```

---

## Configuration

Add WeChat configuration in `appsettings.json`:

```json
{
  "WechatOptions": {
    "WechatAppId": "wx_your_appid",
    "WechatAppSecret": "your_secret",
    "WxOpenAppId": "wx_open_appid",
    "WxOpenAppSecret": "wx_open_secret",
    "CorpId": "your_corp_id",
    "CorpSecret": "your_corp_secret"
  },
  "WechatPayOptions": {
    "MerchantId": "your_merchant_id",
    "MerchantV3Secret": "your_v3_secret",
    "MerchantCertificateSerialNumber": "your_cert_sn",
    "MerchantCertificatePrivateKey": "/certs/apiclient_key.pem"
  }
}
```

---

## Tips

1. **Client Caching**: `WechatClientFactory` automatically caches client instances, no need to manually manage lifecycle
2. **Interceptor**: All clients automatically inject `WechatClientInterceptor` for request/response logging
3. **WeCom**: `WechatCorpClient` supports both self-built apps (`Client`) and third-party development (`SuiteClients`)
4. **Callback Verification**: `AccessValid()` method automatically completes SHA1 signature verification
5. **XML Parsing**: `XmlMsgRevolve` automatically identifies message types and deserializes to corresponding subclasses

---

## License

Copyright (c) yswenli. All Rights Reserved.
