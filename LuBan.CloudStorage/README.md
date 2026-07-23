[English](README.en.md) | 中文
# LuBan.CloudStorage

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 一套接口，三大云商 —— 云存储、云存储、对象存储 无缝切换，代码零改动。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.VideoKit](../LuBan.VideoKit/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.CloudStorage？

项目要对接云存储，但选型总在变：今天用多云存储。每次切换都要重写上传下载逻辑？

**LuBan.CloudStorage** 通过统一的 `ICloudStorageClient` 接口屏蔽底层差异：
- 工厂模式 + 缓存实例，切换云商只需改一个枚举值
- 上传支持本地文件路径和 Stream 两种方式
- 下载区分大文件（流式）和小文件（字节数组）
- SAS 临时 URI 生成，安全分享私有资源
- `FileHandler` 集成文件验证、MD5 去重、视频缩略图自动生成
- `UrlUpdateHelper` + `NewUrlAttribute` 自动刷新过期的临时 URL

## 快速预览

```csharp
// 从配置创建（自动读取 Nacos 中的 UploadOptions）
var client = CloudStorageClientFactory.Create();

// 或手动指定配置
var client = CloudStorageClientFactory.Create(new CloudStorageOptions
{
    SupplierType = EnumSupplierType.Aliyun,
    Id = "your-access-key-id",
    Key = "your-access-key-secret",
    ContainerName = "my-bucket",
    Endpoint = "https://oss-cn-shenzhen.aliyuncs.com"
});

// 上传文件
await client.UploadAsync("documents/report.pdf", localFilePath);
await client.UploadAsync("images/logo.png", imageStream);

// 下载文件
using var stream = await client.DownloadAsync("documents/report.pdf");
var bytes = await client.DownloadContentAsync("config/app.json");

// 生成临时访问链接
var sasUri = await client.GetSasUri("private/contract.pdf", DateTimeOffset.Now.AddHours(2));

// 判断文件是否存在
var exists = await client.ExistAsync("documents/report.pdf");
```

## 安装

```bash
dotnet add package LuBan.CloudStorage
```

## 功能概览

| 功能模块 | 核心类 | 说明 |
|---------|--------|------|
| 统一接口 | `ICloudStorageClient` | Upload/Download/Delete/GetSasUri/Exist |
| 工厂 | `CloudStorageClientFactory` | 带缓存的实例创建，支持自动读取配置 |
| 云存储实现 | `AliyunStorageClient` | 云存储 适配 |
| Azure 实现 | `AzureStorageClient` | 云存储 Storage 适配 |
| 对象存储 实现 | `对象存储StorageClient` | 对象存储适配 |
| 文件处理器 | `FileHandler` | 文件上传/下载/验证/MD5去重/视频缩略图 |
| 文件流信息 | `FStream` | 封装文件名、ContentType、Stream |
| URL 刷新 | `UrlUpdateHelper` | 自动刷新标记了 `NewUrlAttribute` 的过期 URL |
| URL 标记 | `NewUrlAttribute` | 标记需要自动刷新 SAS URL 的字段 |
| 上传配置 | `UploadOptions` | 路径、大小限制、扩展名、云存储开关 |
| 云存储配置 | `CloudStorageOptions` | 供应商类型、凭证、容器、缓存策略 |

## 详细用法

### 配置示例

```json
{
  "UploadOptions": {
    "Path": "upload/{yyyy}/{MM}/{dd}",
    "MaxSize": 134217728,
    "ExtensionNames": [".jpg", ".png", ".pdf", ".docx", ".mp4"],
    "EnableMd5": true,
    "EnableCloudStorage": true,
    "CloudStorageOptions": {
      "Id": "your-access-key-id",
      "Key": "your-access-key-secret",
      "SupplierType": 1,
      "ContainerName": "my-bucket",
      "Endpoint": "https://oss-cn-shenzhen.aliyuncs.com",
      "EnableDownloadCache": false,
      "EnableUploadCache": false
    }
  }
}
```

`SupplierType` 枚举值：`1` = 云存储，`2` = Azure，`3` = 对象存储

### FileHandler 文件上传

```csharp
// 注入使用（IScoped 生命周期）
public class FileController
{
    readonly FileHandler _fileHandler;

    // 上传小文件（字节数组）
    var dbFile = await _fileHandler.HandleUploadFileAsync(
        domain: "https://api.example.com",
        rootPath: "/data",
        bytes: fileBytes,
        fileName: "report.pdf",
        length: fileBytes.Length,
        savePath: null);

    // 上传大文件（流式，自动计算 MD5）
    var dbFile = await _fileHandler.HandleUploadFileAsync(
        domain: "https://api.example.com",
        rootPath: "/data",
        stream: requestStream,
        fileName: "video.mp4",
        length: contentLength,
        savePath: null);

    // 上传视频（自动生成封面缩略图）
    var dbFile = await _fileHandler.HandleUploadVideoFileAsync(
        domain: "https://api.example.com",
        rootPath: "/data",
        videoContentStream: videoStream,
        fileName: "lecture.mp4",
        length: contentLength,
        savePath: null,
        posterTime: "00:00:03");
}
```

### NewUrlAttribute 自动刷新 URL

```csharp
public class CourseDto
{
    public string Title { get; set; }

    [NewUrl(60)]
    public string CoverUrl { get; set; }
}

// 使用时自动刷新过期的 SAS URL
await UrlUpdateHelper.UpdateUrlsAsync(courseDto);
```

## 使用提示

- `CloudStorageClientFactory` 内部使用 `ConcurrentDictionary` 缓存实例，相同供应商类型不会重复创建
- 启用 `EnableUploadCache` 会先写入本地再上传，适合网络不稳定的环境
- 启用 `EnableDownloadCache` 会将远程文件缓存到本地，后续请求直接读取本地文件
- `FileHandler` 内置 MD5 去重机制，相同文件不会重复上传
- 上传路径支持日期模板，如 `upload/{yyyy}/{MM}/{dd}` 会自动替换为当天日期
- `FileHandler.ComputeMd5StreamAsync` 采用流式计算 MD5，避免大文件占用过多内存

## 许可证

Copyright (c) yswenli. All rights reserved.
