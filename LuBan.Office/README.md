[English](README.en.md) | 中文
# LuBan.Office

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> Office 文档自动化 —— PPT、Word、PDF 的创建、编辑与格式转换，一行代码完成。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.PdfKit](../LuBan.PdfKit/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.Office？

在企业级应用中，经常需要自动化生成 PPT 报告、批量替换 Word 模板内容、将文档转为 PDF。手动操作 Office 既低效又不可扩展。

**LuBan.Office** 提供了面向对象的文档操作 API：
- **PptDocument**：创建/打开 PPT，操作幻灯片、形状、文本、图片、表格、视频
- **WordDocument**：文本替换、书签填充、表格操作、插入图片、导出 PDF
- **PdfDocument**：文本替换、图片替换、HTML/URL 转 PDF
- 统一的许可证管理
- 支持从 HTML、PDF 等格式加载 PPT

> **注意**：LuBan.Office 仅支持 Windows 平台。

## 快速预览

```csharp
// PPT：打开模板并替换内容
using var ppt = PptDocument.Load("template.pptx");
PptDocument.ReplaceText(ppt, 0, new[] { ("{{标题}}", "月度报告"), ("{{日期}}", "2026-07") });
PptDocument.ReplaceTableText(ppt, 1, new[] { ("{{姓名}}", "张三"), ("{{成绩}}", "95") });
PptDocument.ReplaceImage(ppt, 2, "photo.jpg", "{{照片}}", deletionFile);
ppt.Save("output.pptx");

// Word：模板填充
using var word = new WordDocument("template.docx");
word.ReplaceText("{{公司名}}", "LuBan 科技");
word.ReplaceTextWithImage("{{印章}}", "seal.png");
word.ReplaceTextWithBookmark(new { Name = "张三", Age = 30 });
word.ExportToPdf("output.pdf");

// PDF：文本替换
using var pdf = new PdfDocument("document.pdf");
pdf.ReplaceText("旧文本", "新文本");
pdf.ReplacePicture("new-image.png", pageIndex: 1);
pdf.Save("output.pdf");
```

## 安装

```bash
dotnet add package LuBan.Office
```

## 功能概览

### PptDocument（PPT 操作）

| 功能 | API | 说明 |
|------|-----|------|
| 创建/打开 | `new PptDocument()` / `PptDocument.Load(path)` | 创建空白或加载已有 PPT |
| 从 HTML 加载 | `PptDocument.LoadFromHtml(path)` | HTML 转 PPT |
| 从 PDF 加载 | `PptDocument.LoadFromPdf(path)` | PDF 转 PPT |
| 添加幻灯片 | `AddSlide()` / `AddSlide(slide)` / `AddSlide(fromIndex)` | 新增/克隆幻灯片 |
| 删除幻灯片 | `RemoveSlide(index)` / `RemoveSlide(slide)` | 按索引或对象删除 |
| 获取幻灯片 | `GetSlide(id)` / `GetSlideByIndex(index)` / `GetSlideByNumber(num)` | 多种方式定位幻灯片 |
| 替换文本 | `PptDocument.ReplaceText(doc, slideIndex, replaceStrs)` | 批量替换幻灯片文本 |
| 替换表格文本 | `PptDocument.ReplaceTableText(doc, slideIndex, replaceStrs)` | 替换表格内文本 |
| 替换图片 | `PptDocument.ReplaceImage(doc, slideIndex, path, altTxt, ...)` | 按 alt 文本定位替换图片 |
| 替换视频 | `PptDocument.ReplaceVideo(doc, slideIndex, video, cover, altTxt, ...)` | 按 alt 文本定位替换视频 |
| 保存 | `Save(path)` / `SaveAsPdf(path)` / `SaveAsHtml(path)` | 多格式导出 |

### PptSlide（幻灯片操作）

| 功能 | API | 说明 |
|------|-----|------|
| 克隆 | `Clone()` | 复制当前幻灯片 |
| 背景色 | `GetBackGroundColor()` / `SetBackGroundColor(color)` | 获取/设置背景色 |
| 背景图 | `SetBackGroundImage(path)` / `GetBackGroundImage()` | 设置/获取背景图 |
| 文本形状 | `GetTextShapes()` | 获取所有文本框 |
| 表格形状 | `GeTableShapes()` | 获取所有表格 |
| 按 alt 文本查找 | `GetShapesByAltTxt(txt)` / `GetShapeByAltTxt(txt)` | 按替代文本定位形状 |
| 添加视频 | `AddVideoFrame(x, y, w, h, video, cover)` | 嵌入视频帧 |
| 添加音频 | `AddAudioFrameEmbedded(x, y, w, h, audio)` | 嵌入音频帧 |

### WordDocument（Word 操作）

| 功能 | API | 说明 |
|------|-----|------|
| 替换文本 | `ReplaceText(old, new)` | 全文替换 |
| 插入文本 | `InsertText(text, index)` | 在指定位置插入 |
| 文本替换为图片 | `ReplaceTextWithImage(regex, path)` | 支持正则匹配 |
| 插入图片 | `InserImage(path, left, top, width, height)` | 指定位置插入 |
| 表格文本替换 | `ReplaceTextWithTable(tableIndex, replaceData)` | 替换指定表格内容 |
| 书签填充 | `ReplaceTextWithBookmark<T>(data)` | 泛型对象属性自动映射到书签 |
| 导出 PDF | `ExportToPdf(path)` | Word 转 PDF |
| 获取段落数 | `GetParagraphCount()` | 获取文档总行数 |

### PdfDocument（PDF 操作）

| 功能 | API | 说明 |
|------|-----|------|
| 替换文本 | `ReplaceText(old, new)` / `ReplaceTexts(dict)` | 全文/批量替换 |
| 替换图片 | `ReplacePicture(path, pageIndex, imageIndex)` | 替换指定页图片 |
| 从 HTML 转换 | `ConvertFromHtml(path)` | HTML 转 PDF |
| 从 URL 转换 | `ConvertFromUrl(url)` | 网页转 PDF |

## 详细用法

### PPT 模板批量生成

```csharp
using var ppt = PptDocument.Load("template.pptx");

// 第 0 页替换标题和日期
PptDocument.ReplaceText(ppt, 0, new[]
{
    ("{{标题}}", "2026年Q2季度报告"),
    ("{{日期}}", "2026-07-11")
});

// 第 1 页替换表格数据
PptDocument.ReplaceTableText(ppt, 1, new[]
{
    ("{{姓名}}", "张三"),
    ("{{部门}}", "技术部"),
    ("{{业绩}}", "150%")
});

// 第 2 页替换照片（按 alt 文本定位）
PptDocument.ReplaceImage(ppt, 2, "zhangsan.jpg", "{{照片}}", deletionFile);

ppt.Save("report_q2.pptx");
```

### Word 书签填充

```csharp
public class ContractData
{
    public string PartyA { get; set; } = "LuBan 科技";
    public string PartyB { get; set; } = "合作方";
    public string Amount { get; set; } = "100,000";
    public bool IsConfidential { get; set; } = true;
}

using var word = new WordDocument("contract_template.docx");
word.ReplaceTextWithBookmark(new ContractData());
word.Save("contract_filled.docx");
```

## 使用提示

- **平台限制**：LuBan.Office 仅支持 Windows 平台（`SupportedOSPlatform("windows")`）
- 许可证文件需放置在 `Libs/` 目录下，程序启动时自动加载
- PPT 中通过 `AlternativeText`（替代文本）定位形状，建议在模板中预先设置好 alt 文本
- `PptDocument` 实现了 `IDisposable`，`Dispose` 时会自动调用 `Save()`
- Word 的 `ReplaceTextWithImage` 支持正则表达式匹配
- 书签填充时，`bool` 类型属性会自动插入复选框

## 许可证

Copyright (c) yswenli. All rights reserved.
