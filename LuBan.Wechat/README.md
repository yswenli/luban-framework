[English](README.en.md) | 中文

# LuBan.Wechat

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **微信公众号、小程序、企业微信、微信支付——一个工厂类搞定全部微信生态对接。**

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.Wechat？

微信生态对接涉及公众号 API、企业微信 API、微信支付 V3 三套完全不同的 SDK，加上回调签名验证、XML 消息解析、AES 加解密等繁琐细节，往往让开发者疲于应付。  
LuBan.Wechat 封装了统一的客户端工厂和完整的消息协议模型，让你用最少的代码完成微信全场景集成。

---

## 快速预览

```csharp
// 获取微信公众号客户端
var client = WechatClientFactory.Create(EnumWechatType.Api);

// 获取微信支付客户端
var payClient = WechatClientFactory.Create(EnumWechatType.Pay);

// 获取企业微信客户端
var corpClient = WechatClientFactory.Create(EnumWechatType.Corp);
```

---

## 技术栈 & 依赖

| 类别 | 组件 | 说明 |
|------|------|------|
| 运行时 | .NET 8.0 | — |
| 框架依赖 | LuBan.Web.Core, LuBan.Service, LuBan.Common | LuBan 框架核心 |

---

## 安装

```bash
dotnet add package LuBan.Wechat
```

---

## 功能全景

### 客户端工厂

`WechatClientFactory` 基于 `ConcurrentDictionary` 缓存客户端实例，同一类型只创建一次：

| 方法 | 说明 |
|------|------|
| `Create(EnumWechatType)` | 按类型获取客户端（Api/Pay/App/Corp） |
| `CreateWechatApiClient(options)` | 创建公众号客户端 |
| `CreateWechatAppClient(options)` | 创建微信 App 客户端 |
| `CreateWechatTenpayClient(options)` | 创建微信支付客户端 |
| `CreateWechatCorpClient(options)` | 创建企业微信客户端 |

### 回调处理

| 类 | 说明 |
|----|------|
| `WechatCallBackCall` | 公众号回调：签名验证、XML 消息解析、回复构建 |
| `WechatCorpCallBackCall` | 企业微信回调处理 |

### 消息协议模型

完整的微信消息收发模型（29 个模型类）：

**接收消息（Receive）：**
- `ReceiveText` — 文本消息
- `ReceiveImage` — 图片消息
- `ReceiveVoice` — 语音消息
- `ReceiveVideo` — 视频消息
- `ReceiveLocation` — 位置消息
- `ReceiveLink` — 链接消息
- `ReceiveEvent` — 事件消息

**回复消息（Reply）：**
- `ReplyText` — 回复文本
- `ReplyImage` — 回复图片
- `ReplyVoice` — 回复语音
- `ReplyVideo` — 回复视频
- `ReplyMusic` — 回复音乐
- `ReplyNews` — 回复图文

### 企业微信

| 类 | 说明 |
|----|------|
| `WechatCorpClient` | 企业微信客户端，包含 `WechatWorkClient` 和 `WechatCorpSuiteClient` |
| `IWechatCorpClient` | 企业微信客户端接口 |
| `WechatCorpSignature` | 企业微信签名工具 |
| `WechatBizMsgCryptor` | 企业微信消息加解密 |
| `CryptographyUtil` | 密码学工具 |

### 其他工具

| 类 | 说明 |
|----|------|
| `WechatClientInterceptor` | HTTP 请求拦截器，自动日志记录 |
| `XmlResolve` | XML 消息解析工具 |
| `TencentLocationServiceClient` | 腾讯位置服务客户端 |

---

## 代码示例

### 处理公众号回调

```csharp
public class WechatController : BaseApiController
{
    // GET 验证签名
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
        return Content("验证失败");
    }

    // POST 接收消息
    [HttpPost("wechat/callback")]
    public async Task<IActionResult> ReceiveMessage()
    {
        var bytes = await WebApp.HttpContext.GetRequestBodyBytesAsync();
        var result = WechatCallBackCall.XmlMsgRevolve(bytes, out string xml, out Receive receive);

        if (receive is ReceiveText textMsg)
        {
            // 回复文本消息
            var reply = new ReplyText
            {
                ToUserName = textMsg.FromUserName,
                FromUserName = textMsg.ToUserName,
                Content = $"你说的是：{textMsg.Content}"
            };
            return Content(WechatCallBackCall.Reply(reply), "application/xml");
        }

        return Content("success");
    }
}
```

### 生成 JS-SDK 签名

```csharp
var signature = WechatCallBackCall.Signature(
    jsapi_ticket: "your_ticket",
    url: "https://example.com/page",
    out string noncestr,
    out long timestamp
);
```

### 调用微信 API

```csharp
var client = WechatClientFactory.Create(EnumWechatType.Api) as WechatApiClient;

// 获取用户信息
var request = new CreateWechatAccessTokenRequest();
var response = await client.ExecuteAsync(request);
```

---

## 配置

在 `appsettings.json` 中添加微信配置：

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

## 小贴士

1. **客户端缓存**：`WechatClientFactory` 自动缓存客户端实例，无需手动管理生命周期
2. **拦截器**：所有客户端自动注入 `WechatClientInterceptor`，记录请求/响应日志
3. **企业微信**：`WechatCorpClient` 同时支持自建应用（`Client`）和第三方代开发（`SuiteClients`）
4. **回调验证**：`AccessValid()` 方法自动完成 SHA1 签名校验
5. **XML 解析**：`XmlMsgRevolve` 自动识别消息类型并反序列化为对应子类

---

## 许可证

Copyright (c) yswenli. All Rights Reserved.
