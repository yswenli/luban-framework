[中文](README.md) | English
# LuBan.CloudStorage

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> One interface, three cloud providers — seamlessly switch between multiple cloud providers with zero code changes.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.VideoKit](../LuBan.VideoKit/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.CloudStorage?

Your project needs cloud storage integration, but the provider keeps changing: different providers for different scenarios the day after. Do you have to rewrite upload/download logic every time?

**LuBan.CloudStorage** abstracts away the differences through a unified `ICloudStorageClient` interface:
- Factory pattern + cached instances — switch providers by changing a single enum value
- Upload supports both local file paths and streams
- Download distinguishes between large files (streaming) and small files (byte arrays)
- SAS temporary URI generation for secure sharing of private resources
- `FileHandler` integrates file validation, MD5 deduplication, and automatic video thumbnail generation
- `UrlUpdateHelper` + `NewUrlAttribute` automatically refreshes expired temporary URLs

## Quick Preview

```csharp
// Create from configuration (automatically reads UploadOptions from Nacos)
var client = CloudStorageClientFactory.Create();

// Or specify configuration manually
var client = CloudStorageClientFactory.Create(new CloudStorageOptions
{
    SupplierType = EnumSupplierType.Aliyun,
    Id = "your-access-key-id",
    Key = "your-access-key-secret",
    ContainerName = "my-bucket",
    Endpoint = "https://oss-cn-shenzhen.aliyuncs.com"
});

// Upload files
await client.UploadAsync("documents/report.pdf", localFilePath);
await client.UploadAsync("images/logo.png", imageStream);

// Download files
using var stream = await client.DownloadAsync("documents/report.pdf");
var bytes = await client.DownloadContentAsync("config/app.json");

// Generate temporary access link
var sasUri = await client.GetSasUri("private/contract.pdf", DateTimeOffset.Now.AddHours(2));

// Check if file exists
var exists = await client.ExistAsync("documents/report.pdf");
```

## Installation

```bash
dotnet add package LuBan.CloudStorage
```

## Feature Overview

| Module | Core Class | Description |
|--------|------------|-------------|
| Unified Interface | `ICloudStorageClient` | Upload/Download/Delete/GetSasUri/Exist |
| Factory | `CloudStorageClientFactory` | Cached instance creation, supports auto-reading configuration |
| cloud provider | `AliyunStorageClient` | cloud provider OSS adapter |
| Azure | `AzureStorageClient` | 云存储 Storage adapter |
| 对象存储 | `对象存储StorageClient` | object storage adapter |
| File Handler | `FileHandler` | File upload/download/validation/MD5 dedup/video thumbnails |
| File Stream Info | `FStream` | Encapsulates filename, ContentType, Stream |
| URL Refresh | `UrlUpdateHelper` | Auto-refreshes expired URLs marked with `NewUrlAttribute` |
| URL Marker | `NewUrlAttribute` | Marks fields for automatic SAS URL refresh |
| Upload Config | `UploadOptions` | Path, size limits, extensions, cloud storage toggle |
| Cloud Storage Config | `CloudStorageOptions` | Provider type, credentials, container, cache strategy |

## Detailed Usage

### Configuration Example

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

`SupplierType` enum values: `1` = cloud provider, `2` = Azure, `3` = 对象存储

### FileHandler File Upload

```csharp
// Inject for use (IScoped lifetime)
public class FileController
{
    readonly FileHandler _fileHandler;

    // Upload small file (byte array)
    var dbFile = await _fileHandler.HandleUploadFileAsync(
        domain: "https://api.example.com",
        rootPath: "/data",
        bytes: fileBytes,
        fileName: "report.pdf",
        length: fileBytes.Length,
        savePath: null);

    // Upload large file (streaming, auto MD5 calculation)
    var dbFile = await _fileHandler.HandleUploadFileAsync(
        domain: "https://api.example.com",
        rootPath: "/data",
        stream: requestStream,
        fileName: "video.mp4",
        length: contentLength,
        savePath: null);

    // Upload video (auto-generate cover thumbnail)
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

### NewUrlAttribute Auto-Refresh URL

```csharp
public class CourseDto
{
    public string Title { get; set; }

    [NewUrl(60)]
    public string CoverUrl { get; set; }
}

// Automatically refreshes expired SAS URLs when used
await UrlUpdateHelper.UpdateUrlsAsync(courseDto);
```

## Tips

- `CloudStorageClientFactory` uses `ConcurrentDictionary` internally to cache instances — identical provider types won't create duplicates
- Enabling `EnableUploadCache` writes to local storage first before uploading, suitable for unstable network environments
- Enabling `EnableDownloadCache` caches remote files locally — subsequent requests read from local files directly
- `FileHandler` has built-in MD5 deduplication — identical files won't be uploaded repeatedly
- Upload paths support date templates — e.g., `upload/{yyyy}/{MM}/{dd}` is automatically replaced with the current date
- `FileHandler.ComputeMd5StreamAsync` uses streaming MD5 calculation to avoid excessive memory usage for large files

## License

Copyright (c) yswenli. All rights reserved.
