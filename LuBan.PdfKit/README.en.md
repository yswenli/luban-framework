[中文](README.md) | English
# LuBan.PdfKit

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> A powerful PDF toolkit — text replacement, image replacement, HTML to PDF, all in just a few lines of code.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Office](../LuBan.Office/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.PdfKit?

PDF editing has always been a pain point in .NET development: open-source libraries have limited functionality, and commercial libraries are expensive. Even the seemingly simple requirement of "precisely locating and replacing text in an existing PDF" is surprisingly complex to implement.

**LuBan.PdfKit** provides practical PDF manipulation tools:
- Precise text replacement: locates text coordinates via custom `TextLocationListener`, uses white overlay + redraw approach
- Text-to-image replacement: replaces placeholder text in PDFs with images (e.g., electronic seals)
- Image replacement: replaces existing images in PDFs
- HTML to PDF: relies on the system CJK font `msyh.ttc` (Windows: `C:/Windows/Fonts/msyh.ttc`, Linux: `/usr/share/fonts/msyh.ttc`); if missing it falls back to the iText default font, so Chinese text may not render correctly
- URL to PDF: directly captures web page content to generate PDFs
- Images to PDF: merges multiple images into a single PDF document

## Quick Preview

```csharp
// Replace single text
PdfUtil.ReplaceText(
    inputPath: "template.pdf",
    outputPath: "output.pdf",
    oldText: "{{Name}}",
    newText: "Zhang San",
    fontSize: 14f);

// Batch replace text
PdfUtil.ReplaceTexts(
    inputPath: "template.pdf",
    outputPath: "output.pdf",
    data: new Dictionary<string, string>
    {
        { "{{Name}}", "Zhang San" },
        { "{{Date}}", "2026-07-11" },
        { "{{DocNo}}", "DOC-20260711-001" }
    });

// Replace placeholder text with image (e.g., electronic seal)
PdfUtil.ReplaceTextWithImage(
    inputPath: "contract.pdf",
    outputPath: "signed.pdf",
    oldText: "{{Seal}}",
    imagePath: "seal.png",
    imageWidth: 120,
    imageHeight: 120);

// HTML to PDF (loads the system Microsoft YaHei font; falls back to the default font if missing)
PdfUtil.FromHtml("output.pdf", "<h1>Hello World</h1><p>Chinese support</p>");

// URL to PDF
PdfUtil.FromUrl("page.pdf", "https://example.com/report");

// Images to PDF
PdfUtil.FromImages("album.pdf", new List<string>
{
    "photo1.jpg", "photo2.jpg", "photo3.jpg"
});
```

## Installation

```bash
dotnet add package LuBan.PdfKit
```

Runtime dependencies (pulled in with the package):

| Dependency | Version | Purpose |
|------------|---------|---------|
| `itext7` | 9.8.0 | Core PDF read/write library |
| `itext7.bouncy-castle-adapter` | 9.8.0 | Encryption/signing (Bouncy Castle adapter) |
| `itext7.pdfhtml` | 6.3.4 | HTML to PDF |

> **Licensing notice**: the iText7 family is licensed under **AGPL v3**. Closed-source commercial distribution/deployment requires a commercial iText license; otherwise the relevant code must be released under AGPL.

## Feature Overview

| Feature | Method | Description |
|---------|--------|-------------|
| Replace Text | `PdfUtil.ReplaceText` | Precisely locate and replace text in PDF |
| Batch Replace Text | `PdfUtil.ReplaceTexts` | Batch replacement via dictionary |
| Text to Image | `PdfUtil.ReplaceTextWithImage` | Replace placeholder text with image |
| Batch Text to Image | `PdfUtil.ReplaceTextsWithImages` | Batch replace text with images |
| Replace Image | `PdfUtil.ReplaceImage` | Replace images in PDF |
| Batch Replace Images | `PdfUtil.ReplaceImages` | Batch replace images in PDF |
| Images to PDF | `PdfUtil.FromImages` | Merge multiple images into PDF |
| HTML to PDF | `PdfUtil.FromHtml` | HTML content to PDF (Chinese text relies on the system msyh.ttc font) |
| URL to PDF | `PdfUtil.FromUrl` | Web page URL to PDF |
| Text Locator | `TextLocationListener` | Custom text position listener |
| Custom Font Path | `PdfUtil.CustomFontPath` | Static property for the default font file; when empty, the OS default path is used |
| Text Chunk | `TextChunk` | Text coordinate information encapsulation |
| Rectangle Area | `PdfRectangle` | Adjusts text overlay area offset |

## Detailed Usage

### Custom Fonts

```csharp
// Specify custom font file path
PdfUtil.ReplaceText(
    inputPath: "input.pdf",
    outputPath: "output.pdf",
    oldText: "{{Title}}",
    newText: "Custom Font Title",
    fontFilePath: "/usr/share/fonts/simhei.ttf",
    fontSize: 18f);
```

Default font loading strategy:
- Windows: automatically loads `C:/Windows/Fonts/msyh.ttc` (Microsoft YaHei)
- Linux: automatically loads `/usr/share/fonts/msyh.ttc`
- The static property `PdfUtil.CustomFontPath` can specify the default font file, taking precedence over the OS paths above
- If none of them exists, the system default font is used (the iText default font has no Chinese glyphs)

### Adjusting Text Overlay Offset

```csharp
// When replaced text position is off, adjust PdfRectangle offset
var offset = new PdfRectangle(-3, -3, 4, 8); // left, top, width, height
PdfUtil.ReplaceText("input.pdf", "output.pdf", "old text", "new text", offset: offset);
```

### HTML to PDF (with External Resources)

```csharp
var html = @"
<html>
<head><style>body { font-family: 'Microsoft YaHei'; }</style></head>
<body>
    <h1>Report Title</h1>
    <table border='1'>
        <tr><td>Name</td><td>Zhang San</td></tr>
        <tr><td>Date</td><td>2026-07-11</td></tr>
    </table>
</body>
</html>";
PdfUtil.FromHtml("report.pdf", html);
```

## Tips

- Text replacement principle: locate original text coordinates, cover with white rectangle, then draw new text at the same position
- The four values of `PdfRectangle` control left/top/width/height offsets for fine-tuning the overlay area
- Batch replacement (`ReplaceTexts`) uses chained intermediate files, with automatic cleanup of temporary files
- When converting HTML to PDF, ensure Chinese fonts are properly installed or specify via `fontFilePath`
- `FromUrl` downloads the page directly via `HttpClientProxy.DownloadBytes(url)` and only supports anonymous access — there is no request-header/credential entry point (for pages requiring authentication, download the HTML yourself and use `FromHtml`)
- When deploying on Linux, manually install Microsoft YaHei font files to `/usr/share/fonts/`

## License

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
