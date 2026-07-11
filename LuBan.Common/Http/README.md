[English](README.en.md) | 中文

# HttpClientProvider 使用指南

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

---

**Related Projects**: [LuBan.Common](../README.md) | [LuBan.Framework](../../README.md)

---

## 注册服务

在 `Program.cs` 或 `Startup.cs` 中注册：

```csharp
using LuBan.Common.Http;

// 注册 HttpClientProvider
builder.Services.AddHttpClientProvider();
```

## 注入使用

```csharp
using LuBan.Common.Http;

public class MyService
{
    private readonly IHttpClientProvider _httpClientProvider;

    public MyService(IHttpClientProvider httpClientProvider)
    {
        _httpClientProvider = httpClientProvider;
    }

    public async Task<string> GetDataAsync(string url)
    {
        var client = _httpClientProvider.Create("https://api.example.com");
        return await client.GetAsync("/data");
    }
}
```

## WebCrawlerUtil 异步方法

```csharp
// GET 请求
var html = await WebCrawlerUtil.Get_HttpAsync("https://example.com");

// POST 请求
var result = await WebCrawlerUtil.Post_HttpAsync(
    "https://api.example.com/submit",
    "key=value",
    "utf-8");

// 带 Cookie 的请求
var cookieContainer = new CookieContainer();
var html = await WebCrawlerUtil.GetHtmlAsync("https://example.com", cookieContainer);

// 带 POST 数据和 Cookie 的请求
var html = await WebCrawlerUtil.GetHtmlAsync(
    "https://api.example.com/data",
    "key=value",
    true,
    cookieContainer);

// 获取流
var stream = await WebCrawlerUtil.GetStreamAsync("https://example.com/image.png", cookieContainer);
```
