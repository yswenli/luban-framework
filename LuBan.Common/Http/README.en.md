[中文](README.md) | English

# HttpClientProvider Usage Guide

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

---

**Related Projects**: [LuBan.Common](../README.md) | [LuBan.Framework](../../README.md)

---

## Register Service

Register in `Program.cs` or `Startup.cs`:

```csharp
using LuBan.Common.Http;

// Register HttpClientProvider
builder.Services.AddHttpClientProvider();
```

## Inject and Use

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

## WebCrawlerUtil Async Methods

```csharp
// GET request
var html = await WebCrawlerUtil.Get_HttpAsync("https://example.com");

// POST request
var result = await WebCrawlerUtil.Post_HttpAsync(
    "https://api.example.com/submit",
    "key=value",
    "utf-8");

// Request with Cookie
var cookieContainer = new CookieContainer();
var html = await WebCrawlerUtil.GetHtmlAsync("https://example.com", cookieContainer);

// Request with POST data and Cookie
var html = await WebCrawlerUtil.GetHtmlAsync(
    "https://api.example.com/data",
    "key=value",
    true,
    cookieContainer);

// Get stream
var stream = await WebCrawlerUtil.GetStreamAsync("https://example.com/image.png", cookieContainer);
```
