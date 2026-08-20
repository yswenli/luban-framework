/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common.Data
*文件名： ExcelUtil
*版本号： V1.0.0.0
*唯一标识：fb17e581-a564-4b3d-a0a7-abc085d6a70e
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 10:46:31
*描述：Excel工具类
*
*=====================================================================
*修改标记
*修改时间：2026/8/13 10:00:00
*修改人： yswenli
*版本号： V2.0.0.0
*描述：迁移至MiniExcel，移除NPOI依赖
*
*****************************************************************************/

using MiniExcelLibs;

namespace LuBan.Common.Data;

/// <summary>
/// Excel工具类
/// </summary>
public static class ExcelUtil
{
    /// <summary>
    /// 从流中读取数据
    /// </summary>
    public static DataTable? ImportFromStream(Stream stream, string sheetName = "sheet1", int startRow = 0, bool hasHeader = true, IEnumerable<string>? columnNameList = null)
    {
        if (stream == null) return null;

        try
        {
            if (stream.CanSeek)
                stream.Position = 0;

            var rows = startRow > 0
                ? stream.Query(useHeaderRow: hasHeader, sheetName: sheetName, startCell: $"A{startRow + 1}").ToList()
                : stream.Query(useHeaderRow: hasHeader, sheetName: sheetName).ToList();

            if (rows.Count == 0) return new DataTable();

            var data = new DataTable();

            var firstRow = rows[0] as IDictionary<string, object>;
            if (firstRow == null) return data;

            var keys = firstRow.Keys.ToList();

            if (columnNameList != null && columnNameList.Any())
            {
                if (columnNameList.Count() != keys.Count) throw new ArgumentOutOfRangeException("自定义列数与数据源不一致");

                foreach (var name in columnNameList)
                    data.Columns.Add(name);
            }
            else
            {
                foreach (var key in keys)
                    data.Columns.Add(key);
            }

            foreach (IDictionary<string, object>? row in rows)
            {
                if (row == null) continue;
                var dataRow = data.NewRow();
                var values = row.Values.ToArray();
                for (int i = 0; i < keys.Count; i++)
                {
                    dataRow[i] = values[i] ?? "";
                }
                data.Rows.Add(dataRow);
            }

            return data;
        }
        catch (Exception ex)
        {
            Logger.Error("ExcelUtil.ImportFromStream", ex);
            return null;
        }
    }

    /// <summary>
    /// 从文件中读取数据
    /// </summary>
    public static DataTable? ImportFromFile(string filePath, string sheetName = "sheet1", int startRow = 0, bool hasHeader = true, IEnumerable<string>? columnNameList = null)
    {
        using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return ImportFromStream(fs, sheetName, startRow, hasHeader, columnNameList);
    }

    /// <summary>
    /// 根据列名，文件地址，导入生成DataTable
    /// </summary>
    public static DataTable? ImportToDataTable(string filePath, IEnumerable<string>? columnNameList = null, int startRow = 0)
    {
        return ImportFromFile(filePath: filePath, columnNameList: columnNameList, startRow: startRow);
    }

    /// <summary>
    /// 根据列名，文件地址，导入生成List
    /// </summary>
    public static IEnumerable<T>? ImportToModels<T>(string filePath,
        IEnumerable<string>? columnNameList = null,
        int startRow = 0,
        IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        var dt = ImportToDataTable(filePath, columnNameList, startRow);
        return dt?.ToList<T>(namePairs) ?? null;
    }

    /// <summary>
    /// 导出到excel流
    /// </summary>
    public static Stream ExportStreamFromDataTable(DataTable dataTable,
        string fileName,
        string sheetName = "sheet1",
        bool hasHeader = true,
        IEnumerable<string>? columnNameList = null)
    {
        var ms = new MemoryStream();

        try
        {
            var exportTable = PrepareExportTable(dataTable, hasHeader, columnNameList);
            MiniExcel.SaveAs(ms, exportTable, sheetName: sheetName);
            ms.Position = 0;
            return ms;
        }
        catch (Exception ex)
        {
            Logger.Error("ExcelUtil.ExportStreamFromDataTable", ex);
            ms.Dispose();
            return new MemoryStream();
        }
    }

    /// <summary>
    /// 导出到excel流
    /// </summary>
    public static Stream ExportStreamFromModels<T>(this IEnumerable<T> list, string fileName, string sheetName = "sheet1", bool hasHeader = true, IEnumerable<string>? columnNameList = null, IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        var dataTable = list.ToDataTable(namePairs);
        return ExportStreamFromDataTable(dataTable, fileName, sheetName, hasHeader, columnNameList);
    }

    /// <summary>
    /// 导出到excel文件
    /// </summary>
    public static void ExportFileFromDataTable(DataTable dataTable,
        string filePath,
        string sheetName = "sheet1",
        bool hasHeader = true,
        IEnumerable<string>? columnNameList = null)
    {
        try
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var exportTable = PrepareExportTable(dataTable, hasHeader, columnNameList);
            MiniExcel.SaveAs(filePath, exportTable, sheetName: sheetName);
        }
        catch (Exception ex)
        {
            Logger.Error("ExcelUtil.ExportFileFromDataTable", ex);
        }
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    public static void ExportFileFromModels<T>(IEnumerable<T> list,
        string filePath,
        string sheetName = "sheet1",
        bool hasHeader = true,
        IEnumerable<string>? columnNameList = null,
        IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        var dataTable = list.ToDataTable(namePairs);
        ExportFileFromDataTable(dataTable, filePath, sheetName, hasHeader, columnNameList);
    }

    /// <summary>
    /// 导出到excel的多张表内
    /// </summary>
    public static void ExportFileFromModels<TKey, T>(IEnumerable<IGrouping<TKey, T>> groupList,
       string filePath,
       bool hasHeader = true,
       IEnumerable<string>? columnNameList = null,
       IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        var sheets = new Dictionary<string, DataTable>();
        foreach (var item in groupList)
        {
            var dt = item.ToDataTable(namePairs);
            var sheetName = item.Key?.ToString() ?? $"Sheet{sheets.Count + 1}";
            if (columnNameList != null && columnNameList.Any() && columnNameList.Count() == dt.Columns.Count)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                    dt.Columns[i].ColumnName = columnNameList.ElementAt(i);
            }
            sheets[sheetName] = dt;
        }

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        try
        {
            MiniExcel.SaveAs(filePath, sheets);
        }
        catch (Exception ex)
        {
            Logger.Error("ExcelUtil.ExportFileFromModels(multi-sheet)", ex);
        }
    }

    #region 根据excel模板和参数，填充相关文件并返回

    /// <summary>
    /// 根据参数(列表数据，条形码)填充excel模板合并sheet返回excel文件流
    /// </summary>
    public static Stream GetStreamByTemplatesWithListAndBarCode(ExcelTemplateInfoCollection excelTemplateInfos)
    {
        if (excelTemplateInfos == null || excelTemplateInfos.Count < 1) throw new Exception("Template file stream cannot be null");

        using var mergedMs = new MemoryStream();

        using (var document = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Create(
            mergedMs, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
            var sheets = workbookPart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

            foreach (var templateInfo in excelTemplateInfos)
            {
                if (templateInfo.TemplateFileStream == null) throw new Exception("Template file stream cannot be null");

                templateInfo.TemplateFileStream.Position = 0;

                var templateData = new Dictionary<string, object>();

                if (templateInfo.LableValuePairs != null)
                {
                    foreach (var kvp in templateInfo.LableValuePairs)
                        templateData[kvp.Key] = kvp.Value;
                }

                if (templateInfo.List != null && templateInfo.List.Count > 0)
                    templateData["List"] = templateInfo.List;

                using var sheetMs = new MemoryStream();
                MiniExcel.SaveAsByTemplate(sheetMs, templateInfo.TemplateFileStream, templateData);
                sheetMs.Position = 0;

                var sheetName = string.IsNullOrEmpty(templateInfo.SheetName) ? $"Sheet{sheets.Count() + 1}" : templateInfo.SheetName;

                using var sheetDoc = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(sheetMs, true);
                var sheetWorkbookPart = sheetDoc.WorkbookPart!;
                var sheetPart = sheetWorkbookPart.WorksheetParts.First();

                var newSheetPart = workbookPart.AddPart<DocumentFormat.OpenXml.Packaging.WorksheetPart>(sheetPart);

                var sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() + 1;
                var relationshipId = workbookPart.GetIdOfPart(newSheetPart);

                sheets.Append(new DocumentFormat.OpenXml.Spreadsheet.Sheet()
                {
                    Id = relationshipId,
                    SheetId = (uint)sheetId,
                    Name = sheetName
                });

                if (!string.IsNullOrWhiteSpace(templateInfo.BarCodeText))
                {
                    var bytes = CodeUtil.Write(templateInfo.BarCodeText, templateInfo.BarCodeType, templateInfo.BarCodeWidth ?? 0, templateInfo.BarCodeHeight ?? 0).ToBytes();
                    if (bytes != null && bytes.Length > 0)
                    {
                        try
                        {
                            InsertImageToSheet(newSheetPart, bytes, templateInfo.colStart, templateInfo.rowStart, templateInfo.colEnd, templateInfo.rowEnd);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("ExcelUtil.InsertImage", ex);
                        }
                    }
                }
            }

            workbookPart.Workbook.Save();
        }

        mergedMs.Position = 0;
        var result = new MemoryStream();
        mergedMs.CopyTo(result);
        result.Position = 0;
        return result;
    }

    /// <summary>
    /// 根据参数填充excel模板合并sheet返回excel文件
    /// </summary>
    public static void GetFileByTemplatesForList(ExcelTemplateInfoCollection excelTemplateInfos, string fileName)
    {
        using var stream = GetStreamByTemplatesWithListAndBarCode(excelTemplateInfos);
        stream.Save(fileName);
    }

    #endregion

    #region 合并excel

    /// <summary>
    /// 将不同的excel文件合并成一个文件
    /// </summary>
    public static Stream MergeExcelFileByStream(Dictionary<string, Stream> excelFiles)
    {
        var mergedMs = new MemoryStream();

        using (var document = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Create(
            mergedMs, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
            var sheets = workbookPart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

            foreach (var item in excelFiles)
            {
                item.Value.Position = 0;
                using var sheetDoc = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(item.Value, false);
                var sheetWorkbookPart = sheetDoc.WorkbookPart!;
                var sheetPart = sheetWorkbookPart.WorksheetParts.First();

                var newSheetPart = workbookPart.AddPart<DocumentFormat.OpenXml.Packaging.WorksheetPart>(sheetPart);

                var sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() + 1;
                var relationshipId = workbookPart.GetIdOfPart(newSheetPart);

                var sheetName = string.IsNullOrEmpty(item.Key) ? $"Sheet{sheetId}" : item.Key;

                sheets.Append(new DocumentFormat.OpenXml.Spreadsheet.Sheet()
                {
                    Id = relationshipId,
                    SheetId = (uint)sheetId,
                    Name = sheetName
                });
            }

            workbookPart.Workbook.Save();
        }

        mergedMs.Position = 0;
        return mergedMs;
    }

    /// <summary>
    /// 将不同的excel文件合并成一个文件
    /// </summary>
    public static void MergeExcelFile(string filePath, Dictionary<string, string> excelFiles)
    {
        var streams = new List<Stream>();
        try
        {
            var streamDict = new Dictionary<string, Stream>();
            foreach (var item in excelFiles)
            {
                var stream = FileUtil.GetStream(item.Value);
                streams.Add(stream);
                streamDict[item.Key] = stream;
            }

            using var merged = MergeExcelFileByStream(streamDict);
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            merged.CopyTo(fs);
        }
        finally
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }
    }

    #endregion

    #region 辅助方法

    private static DataTable PrepareExportTable(DataTable dataTable, bool hasHeader, IEnumerable<string>? columnNameList)
    {
        var exportTable = dataTable;

        if (columnNameList != null && columnNameList.Any())
        {
            if (columnNameList.Count() != dataTable.Columns.Count) throw new ArgumentOutOfRangeException("自定义列数与数据源不一致");

            exportTable = dataTable.Copy();
            for (int i = 0; i < dataTable.Columns.Count; i++)
                exportTable.Columns[i].ColumnName = columnNameList.ElementAt(i);
        }

        if (!hasHeader)
        {
            var noHeaderTable = new DataTable();
            for (int i = 0; i < exportTable.Columns.Count; i++)
                noHeaderTable.Columns.Add(i.ToString());
            foreach (DataRow row in exportTable.Rows)
                noHeaderTable.Rows.Add(row.ItemArray);
            exportTable = noHeaderTable;
        }

        return exportTable;
    }

    #endregion

    #region OpenXML图片插入

    private static void InsertImageToSheet(DocumentFormat.OpenXml.Packaging.WorksheetPart worksheetPart, byte[] imageBytes, int colStart, int rowStart, int colEnd, int rowEnd)
    {
        var drawingsPart = worksheetPart.AddNewPart<DocumentFormat.OpenXml.Packaging.DrawingsPart>();
        drawingsPart.WorksheetDrawing = new DocumentFormat.OpenXml.Drawing.Spreadsheet.WorksheetDrawing();

        var imagePart = drawingsPart.AddNewPart<DocumentFormat.OpenXml.Packaging.ImagePart>("image/jpeg");
        using (var ms = new MemoryStream(imageBytes))
        {
            imagePart.FeedData(ms);
        }

        var relationshipId = drawingsPart.GetIdOfPart(imagePart);

        var twoCellAnchor = new DocumentFormat.OpenXml.Drawing.Spreadsheet.TwoCellAnchor();

        var fromMarker = new DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker(
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.ColumnId(colStart.ToString()),
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.ColumnOffset("0"),
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.RowId(rowStart.ToString()),
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.RowOffset("0"));

        var toMarker = new DocumentFormat.OpenXml.Drawing.Spreadsheet.ToMarker(
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.ColumnId(colEnd.ToString()),
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.ColumnOffset("0"),
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.RowId(rowEnd.ToString()),
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.RowOffset("0"));

        var picture = new DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture(
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualPictureProperties(
                new DocumentFormat.OpenXml.Drawing.NonVisualDrawingProperties() { Id = (uint)(drawingsPart.WorksheetDrawing.ChildElements.Count + 1), Name = "Image" },
                new DocumentFormat.OpenXml.Drawing.NonVisualPictureDrawingProperties()),
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.BlipFill(
                new DocumentFormat.OpenXml.Drawing.Blip() { Embed = relationshipId },
                new DocumentFormat.OpenXml.Drawing.Stretch(
                    new DocumentFormat.OpenXml.Drawing.FillRectangle())),
            new DocumentFormat.OpenXml.Drawing.Spreadsheet.ShapeProperties(
                new DocumentFormat.OpenXml.Drawing.Transform2D(
                    new DocumentFormat.OpenXml.Drawing.Offset() { X = 0, Y = 0 },
                    new DocumentFormat.OpenXml.Drawing.Extents() { Cx = 0, Cy = 0 }),
                new DocumentFormat.OpenXml.Drawing.PresetGeometry(new DocumentFormat.OpenXml.Drawing.AdjustValueList()) { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle }));

        twoCellAnchor.Append(fromMarker);
        twoCellAnchor.Append(toMarker);
        twoCellAnchor.Append(picture);
        twoCellAnchor.Append(new DocumentFormat.OpenXml.Drawing.Spreadsheet.ClientData());

        drawingsPart.WorksheetDrawing.Append(twoCellAnchor);
        drawingsPart.WorksheetDrawing.Save();
    }

    #endregion
}
