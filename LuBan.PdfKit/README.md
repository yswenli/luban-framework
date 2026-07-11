[English](README.en.md) | 中文
# LuBan.PdfKit

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> PDF 操作利器 —— 文本替换、图片替换、HTML 转 PDF，几行代码搞定。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Office](../LuBan.Office/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.PdfKit？

PDF 编辑一直是 .NET 开发的痛点：开源库功能有限，商业库价格昂贵。尤其是「在已有 PDF 中精确定位并替换文本」这个看似简单的需求，实现起来却异常复杂。

**LuBan.PdfKit** 提供了实用的 PDF 操作工具集：
- 精确文本替换：通过自定义 `TextLocationListener` 定位文本坐标，白底遮盖 + 重写实现
- 文本替换为图片：支持将 PDF 中的占位文本替换为图片（如电子印章）
- 图片替换：替换 PDF 中已有的图片
- HTML 转 PDF：内置 CJK 中文字体支持，跨平台兼容
- URL 转 PDF：直接抓取网页内容生成 PDF
- 图片合成 PDF：多张图片一键合并为 PDF 文档

## 快速预览

```csharp
// 替换单个文本
PdfUtil.ReplaceText(
    inputPath: "template.pdf",
    outputPath: "output.pdf",
    oldText: "{{姓名}}",
    newText: "张三",
    fontSize: 14f);

// 批量替换文本
PdfUtil.ReplaceTexts(
    inputPath: "template.pdf",
    outputPath: "output.pdf",
    data: new Dictionary<string, string>
    {
        { "{{姓名}}", "张三" },
        { "{{日期}}", "2026-07-11" },
        { "{{编号}}", "DOC-20260711-001" }
    });

// 将占位文本替换为图片（如电子签章）
PdfUtil.ReplaceTextWithImage(
    inputPath: "contract.pdf",
    outputPath: "signed.pdf",
    oldText: "{{签章}}",
    imagePath: "seal.png",
    imageWidth: 120,
    imageHeight: 120);

// HTML 转 PDF（自动加载中文字体）
PdfUtil.FromHtml("output.pdf", "<h1>你好世界</h1><p>支持中文</p>");

// URL 转 PDF
PdfUtil.FromUrl("page.pdf", "https://example.com/report");

// 图片合成 PDF
PdfUtil.FromImages("album.pdf", new List<string>
{
    "photo1.jpg", "photo2.jpg", "photo3.jpg"
});
```

## 安装

```bash
dotnet add package LuBan.PdfKit
```

## 功能概览

| 功能 | 方法 | 说明 |
|------|------|------|
| 替换文本 | `PdfUtil.ReplaceText` | 精确定位并替换 PDF 中的文本 |
| 批量替换文本 | `PdfUtil.ReplaceTexts` | 字典方式批量替换 |
| 文本替换为图片 | `PdfUtil.ReplaceTextWithImage` | 将占位文本替换为图片 |
| 批量文本替换为图片 | `PdfUtil.ReplaceTextsWithImages` | 批量将文本替换为图片 |
| 替换图片 | `PdfUtil.ReplaceImage` | 替换 PDF 中的图片 |
| 批量替换图片 | `PdfUtil.ReplaceImages` | 批量替换 PDF 中的图片 |
| 图片转 PDF | `PdfUtil.FromImages` | 多张图片合成 PDF |
| HTML 转 PDF | `PdfUtil.FromHtml` | HTML 内容转 PDF（支持中文） |
| URL 转 PDF | `PdfUtil.FromUrl` | 网页 URL 转 PDF |
| 文本定位 | `TextLocationListener` | 自定义文本位置监听器 |
| 文本块 | `TextChunk` | 文本坐标信息封装 |
| 矩形区域 | `PdfRectangle` | 用于调整文本遮盖区域偏移 |

## 详细用法

### 自定义字体

```csharp
// 指定自定义字体文件路径
PdfUtil.ReplaceText(
    inputPath: "input.pdf",
    outputPath: "output.pdf",
    oldText: "{{标题}}",
    newText: "自定义字体标题",
    fontFilePath: "/usr/share/fonts/simhei.ttf",
    fontSize: 18f);
```

默认字体加载策略：
- Windows：自动加载 `C:/Windows/Fonts/msyh.ttc`（微软雅黑）
- Linux：自动加载 `/usr/share/fonts/msyh.ttc`
- 如以上均不存在，使用系统默认字体

### 调整文本遮盖偏移

```csharp
// 当替换后文本位置有偏差时，可调整 PdfRectangle 偏移量
var offset = new PdfRectangle(-3, -3, 4, 8); // left, top, width, height
PdfUtil.ReplaceText("input.pdf", "output.pdf", "旧文本", "新文本", offset: offset);
```

### HTML 转 PDF（含外部资源）

```csharp
var html = @"
<html>
<head><style>body { font-family: 'Microsoft YaHei'; }</style></head>
<body>
    <h1>报告标题</h1>
    <table border='1'>
        <tr><td>姓名</td><td>张三</td></tr>
        <tr><td>日期</td><td>2026-07-11</td></tr>
    </table>
</body>
</html>";
PdfUtil.FromHtml("report.pdf", html);
```

## 使用提示

- 文本替换原理：先定位原文本坐标，用白色矩形遮盖，再在相同位置绘制新文本
- `PdfRectangle` 的四个值分别控制 left/top/width/height 的偏移量，用于微调遮盖区域
- 批量替换（`ReplaceTexts`）通过链式中间文件实现，最终自动清理临时文件
- HTML 转 PDF 时确保中文字体已正确安装或通过 `fontFilePath` 指定
- `FromUrl` 方法使用 `HttpClientProxy` 下载网页内容，支持需要认证的页面
- Linux 部署时需要手动安装微软雅黑字体文件到 `/usr/share/fonts/`

## 许可证

Copyright (c) yswenli. All rights reserved.
