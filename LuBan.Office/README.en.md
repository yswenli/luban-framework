[中文](README.md) | English
# LuBan.Office

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> Office document automation — create, edit, and convert PPT, Word, and PDF files in a single line of code.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.PdfKit](../LuBan.PdfKit/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.Office?

In enterprise applications, you often need to automatically generate PPT reports, batch-replace Word template content, or convert documents to PDF. Manual Office operations are both inefficient and unscalable.

**LuBan.Office** provides object-oriented document manipulation APIs:
- **PptDocument**: Create/open PPTs, manipulate slides, shapes, text, images, tables, and videos
- **WordDocument**: Text replacement, bookmark filling, table operations, image insertion, PDF export
- **PdfDocument**: Text replacement, image replacement, HTML/URL to PDF conversion
- Unified license management
- Supports loading PPTs from HTML, PDF, and other formats

> **Note**: LuBan.Office only supports the Windows platform.

## Quick Preview

```csharp
// PPT: Open template and replace content
using var ppt = PptDocument.Load("template.pptx");
PptDocument.ReplaceText(ppt, 0, new[] { ("{{Title}}", "Monthly Report"), ("{{Date}}", "2026-07") });
PptDocument.ReplaceTableText(ppt, 1, new[] { ("{{Name}}", "Zhang San"), ("{{Score}}", "95") });
PptDocument.ReplaceImage(ppt, 2, "photo.jpg", "{{Photo}}", deletionFile);
ppt.Save("output.pptx");

// Word: Template filling
using var word = new WordDocument("template.docx");
word.ReplaceText("{{Company}}", "LuBan Tech");
word.ReplaceTextWithImage("{{Seal}}", "seal.png");
word.ReplaceTextWithBookmark(new { Name = "Zhang San", Age = 30 });
word.ExportToPdf("output.pdf");

// PDF: Text replacement
using var pdf = new PdfDocument("document.pdf");
pdf.ReplaceText("old text", "new text");
pdf.ReplacePicture("new-image.png", pageIndex: 1);
pdf.Save("output.pdf");
```

## Installation

```bash
dotnet add package LuBan.Office
```

## Feature Overview

### PptDocument (PPT Operations)

| Feature | API | Description |
|---------|-----|-------------|
| Create/Open | `new PptDocument()` / `PptDocument.Load(path)` | Create blank or load existing PPT |
| Load from HTML | `PptDocument.LoadFromHtml(path)` | HTML to PPT |
| Load from PDF | `PptDocument.LoadFromPdf(path)` | PDF to PPT |
| Add Slide | `AddSlide()` / `AddSlide(slide)` / `AddSlide(fromIndex)` | Add/clone slides |
| Remove Slide | `RemoveSlide(index)` / `RemoveSlide(slide)` | Remove by index or object |
| Get Slide | `GetSlide(id)` / `GetSlideByIndex(index)` / `GetSlideByNumber(num)` | Multiple ways to locate slides |
| Replace Text | `PptDocument.ReplaceText(doc, slideIndex, replaceStrs)` | Batch replace slide text |
| Replace Table Text | `PptDocument.ReplaceTableText(doc, slideIndex, replaceStrs)` | Replace text in tables |
| Replace Image | `PptDocument.ReplaceImage(doc, slideIndex, path, altTxt, ...)` | Replace image by alt text |
| Replace Video | `PptDocument.ReplaceVideo(doc, slideIndex, video, cover, altTxt, ...)` | Replace video by alt text |
| Save | `Save(path)` / `SaveAsPdf(path)` / `SaveAsHtml(path)` | Multi-format export |

### PptSlide (Slide Operations)

| Feature | API | Description |
|---------|-----|-------------|
| Clone | `Clone()` | Copy current slide |
| Background Color | `GetBackGroundColor()` / `SetBackGroundColor(color)` | Get/set background color |
| Background Image | `SetBackGroundImage(path)` / `GetBackGroundImage()` | Set/get background image |
| Text Shapes | `GetTextShapes()` | Get all text boxes |
| Table Shapes | `GeTableShapes()` | Get all tables |
| Find by Alt Text | `GetShapesByAltTxt(txt)` / `GetShapeByAltTxt(txt)` | Locate shapes by alternative text |
| Add Video | `AddVideoFrame(x, y, w, h, video, cover)` | Embed video frame |
| Add Audio | `AddAudioFrameEmbedded(x, y, w, h, audio)` | Embed audio frame |

### WordDocument (Word Operations)

| Feature | API | Description |
|---------|-----|-------------|
| Replace Text | `ReplaceText(old, new)` | Global replacement |
| Insert Text | `InsertText(text, index)` | Insert at specified position |
| Text to Image | `ReplaceTextWithImage(regex, path)` | Supports regex matching |
| Insert Image | `InserImage(path, left, top, width, height)` | Insert at specified position |
| Table Text Replace | `ReplaceTextWithTable(tableIndex, replaceData)` | Replace content in specified table |
| Bookmark Fill | `ReplaceTextWithBookmark<T>(data)` | Auto-map generic object properties to bookmarks |
| Export PDF | `ExportToPdf(path)` | Word to PDF |
| Get Paragraph Count | `GetParagraphCount()` | Get total line count of document |

### PdfDocument (PDF Operations)

| Feature | API | Description |
|---------|-----|-------------|
| Replace Text | `ReplaceText(old, new)` / `ReplaceTexts(dict)` | Global/batch replacement |
| Replace Image | `ReplacePicture(path, pageIndex, imageIndex)` | Replace image on specified page |
| Convert from HTML | `ConvertFromHtml(path)` | HTML to PDF |
| Convert from URL | `ConvertFromUrl(url)` | Web page to PDF |

## Detailed Usage

### PPT Template Batch Generation

```csharp
using var ppt = PptDocument.Load("template.pptx");

// Replace title and date on slide 0
PptDocument.ReplaceText(ppt, 0, new[]
{
    ("{{Title}}", "2026 Q2 Quarterly Report"),
    ("{{Date}}", "2026-07-11")
});

// Replace table data on slide 1
PptDocument.ReplaceTableText(ppt, 1, new[]
{
    ("{{Name}}", "Zhang San"),
    ("{{Department}}", "Engineering"),
    ("{{Performance}}", "150%")
});

// Replace photo on slide 2 (locate by alt text)
PptDocument.ReplaceImage(ppt, 2, "zhangsan.jpg", "{{Photo}}", deletionFile);

ppt.Save("report_q2.pptx");
```

### Word Bookmark Filling

```csharp
public class ContractData
{
    public string PartyA { get; set; } = "LuBan Tech";
    public string PartyB { get; set; } = "Partner";
    public string Amount { get; set; } = "100,000";
    public bool IsConfidential { get; set; } = true;
}

using var word = new WordDocument("contract_template.docx");
word.ReplaceTextWithBookmark(new ContractData());
word.Save("contract_filled.docx");
```

## Tips

- **Platform limitation**: LuBan.Office only supports Windows (`SupportedOSPlatform("windows")`)
- License file must be placed in the `Libs/` directory — it is automatically loaded at startup
- In PPTs, shapes are located via `AlternativeText` — it is recommended to pre-set alt text in templates
- `PptDocument` implements `IDisposable` — `Dispose` automatically calls `Save()`
- Word's `ReplaceTextWithImage` supports regular expression matching
- When filling bookmarks, `bool` type properties automatically insert checkboxes

## License

Copyright (c) yswenli. All rights reserved.
