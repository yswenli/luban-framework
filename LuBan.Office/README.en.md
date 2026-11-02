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
| Add Slide | `AddSlide(int index = -1)` (inserts blank slide) / `AddSlide(PptSlide slide, int index = -1)` / `AddSlide(int fromIndex, int index = -1)` | Add a blank slide, or clone by object/index |
| Remove Slide | `RemoveSlide(index)` / `RemoveSlide(slide)` | Remove by index or object |
| Get Slide | `GetSlide(id)` / `GetSlideByIndex(index)` / `GetSlideByNumber(num)` | Multiple ways to locate slides |
| Replace Text | `PptDocument.ReplaceText(doc, slideIndex, replaceStrs)` | Batch replace slide text |
| Replace Table Text | `PptDocument.ReplaceTableText(doc, slideIndex, replaceStrs)` | Replace text in tables |
| Replace Image | `PptDocument.ReplaceImage(doc, slideIndex, path, altTxt, ...)` | Replace image by alt text |
| Replace Video | `PptDocument.ReplaceVideo(doc, slideIndex, video, cover, altTxt, ...)` | Replace video by alt text |
| Save | `Save(path)` / `SaveAsPdf(path)` / `SaveAsHtml(path)` | Multi-format export |

> **Overload binding note**: the single-argument `AddSlide(int index = -1)` **inserts a blank slide**; to clone by index you must call `AddSlide(fromIndex: 2)` (the two-argument overload). `AddSlide(2)` binds to the single-argument overload and yields a blank slide instead of a clone.

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
| Replace Image | `ReplacePicture(path, pageIndex, oldPictureIndex)` | Replace the specified image on the specified page |
| Convert from HTML | `ConvertFromHtml(path)` | HTML to PDF |
| Convert from URL | `ConvertFromUrl(url)` | Web page to PDF |

## Detailed Usage

### PPT Template Batch Generation

```csharp
using var ppt = PptDocument.Load("template.pptx");

// Clone slide 2 by index: a named argument is required to reach the two-argument overload; the single-argument AddSlide(2) inserts a blank slide
var copy = ppt.AddSlide(fromIndex: 2);

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
- The license file (`Aspose.Total.lic`) must be placed in the `Libs/` directory; each document class calls the matching `License.Instance.SetXxxLicense()` when a document instance is created, rather than loading the license at application startup
- In PPTs, shapes are located via `AlternativeText` — it is recommended to pre-set alt text in templates
- `PptDocument` implements `IDisposable` and `Dispose` automatically calls `Save()`: when `Save()` receives no path it falls back to the original file path used at load time, so a template opened with `using` is written to again on release — even after `Save("output.pptx")` — overwriting the source template; to leave the source file untouched, copy the template first or load it with `new PptDocument(stream)` (no original path in that case, so `Save()` creates a new timestamp-named file)
- Word's `ReplaceTextWithImage` supports regular expression matching
- When filling bookmarks, `bool` type properties automatically insert checkboxes

## License

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
