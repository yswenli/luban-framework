/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
*修改时间：2022/7/14 10:46:31
*修改人： walle.wen
*版本号： V1.0.0.0
*描述： Excel工具类
*
*****************************************************************************/

namespace LuBan.Common.Data;

/// <summary>
/// Excel工具类
/// </summary>
public static class ExcelUtil
{

    static readonly List<short> _dateTimeFormatList;

    /// <summary>
    /// Excel工具类
    /// </summary>
    static ExcelUtil()
    {
        _dateTimeFormatList = new List<short>()
        {
            0xe,
            0xf,
            0x10,
            0x11,
            0x12,
            0x13,
            0x14,
            0x15,
            0x16,
            0x1f,
            0x2d,
            0x2e,
            0x2f,
            0x39,
            0x3a,
            0xb1
        };
    }

    /// <summary>
    /// 从流中读取数据
    /// </summary>
    /// <param name="excelstream"></param>
    /// <param name="v2003"></param>
    /// <param name="sheetName"></param>
    /// <param name="startRow"></param>
    /// <param name="hasHeader"></param>
    /// <param name="columnNameList">自定义全列名</param>
    /// <returns></returns>
    public static DataTable? ImportFromStream(Stream excelstream, bool v2003 = true, string sheetName = "sheet1", int startRow = 0, bool hasHeader = true, IEnumerable<string>? columnNameList = null)
    {
        if (excelstream == null) return null;

        using var workbook = new ExcelBook(v2003, excelstream);

        var sheet = workbook.GetSheet(sheetName);

        var data = new DataTable();

        try
        {
            if (sheet != null)
            {
                IRow firstRow = sheet.GetRow(startRow);
                int cellCount = firstRow.LastCellNum;
                if (hasHeader)
                {
                    if (columnNameList != null && columnNameList.Any())
                    {
                        if (columnNameList.Count() != cellCount) throw new ArgumentOutOfRangeException("自定义列数与数据源不一致");

                        foreach (var columnName in columnNameList)
                        {
                            DataColumn column = new DataColumn(columnName);
                            data.Columns.Add(column);
                        }
                    }
                    else
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                    }
                    startRow = sheet.FirstRowNum + 1;
                }
                else
                {
                    for (int i = 0; i < cellCount; ++i)
                    {
                        DataColumn column = new DataColumn(i.ToString());
                        data.Columns.Add(column);
                    }
                    startRow = sheet.FirstRowNum;
                }

                //最后一列的标号
                int rowCount = sheet.LastRowNum;

                for (int i = startRow; i <= rowCount; ++i)
                {
                    IRow row = sheet.GetRow(i);

                    if (row == null || row.FirstCellNum == -1) continue; //没有数据的行默认是null

                    DataRow dataRow = data.NewRow();

                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                    {

                        if (row.GetCell(j) != null)
                        {
                            var cell = row.GetCell(j);
                            try
                            {
                                switch (cell.CellType)
                                {
                                    case CellType.Blank:
                                        dataRow[j] = "";
                                        break;
                                    case CellType.Numeric:
                                        short format = cell.CellStyle.DataFormat;
                                        if (_dateTimeFormatList.Contains(format))
                                        {
                                            dataRow[j] = DateTime.FromOADate(cell.NumericCellValue).ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                        else
                                            dataRow[j] = cell.NumericCellValue;
                                        break;
                                    case CellType.String:
                                        dataRow[j] = cell.StringCellValue;
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"The value passed in or the value type is incorrect,type:{cell.CellType},value:{cell.StringCellValue}", ex);
                            }
                        }
                        else
                        {
                            dataRow[j] = "";
                        }
                    }
                    data.Rows.Add(dataRow);
                }
            }

            return data;
        }
        catch (Exception ex)
        {
            Logger.Error("ExcelUtil.ExcelToDataTable", ex);
            return null;
        }
    }

    /// <summary>
    /// 从文件中读取数据
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sheetName"></param>
    /// <param name="startRow"></param>
    /// <param name="hasHeader"></param>
    /// <param name="columnNameList">自定义全列名</param>
    /// <returns></returns>
    public static DataTable? ImportFromFile(string fileName, string sheetName = "sheet1", int startRow = 0, bool hasHeader = true, IEnumerable<string>? columnNameList = null)
    {
        using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            if (fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return ImportFromStream(fs, false, sheetName, startRow, hasHeader, columnNameList);
            }
            else
            {
                return ImportFromStream(fs, true, sheetName, startRow, hasHeader, columnNameList);
            }
        }
    }


    /// <summary>
    /// 根据列名，文件地址，导入生成DataTable
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="columnNameList">自定义全列名</param>
    /// <param name="startRow"></param>
    /// <returns></returns>
    public static DataTable? ImportToDataTable(string fileName, IEnumerable<string>? columnNameList = null, int startRow = 0)
    {
        return ImportFromFile(fileName: fileName, columnNameList: columnNameList, startRow: startRow);
    }

    /// <summary>
    /// 根据列名，文件地址，导入生成List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <param name="columnNameList">自定义全列名</param>
    /// <param name="startRow"></param>
    /// <param name="namePairs"></param>
    /// <returns></returns>
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
    /// <param name="dataTable"></param>
    /// <param name="fileName"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasHeader"></param>
    /// <param name="columnNameList">自定义全列名</param>
    /// <returns></returns>
    public static Stream ExportStreamFromDataTable(DataTable dataTable,
        string fileName,
        string sheetName = "sheet1",
        bool hasHeader = true,
        IEnumerable<string>? columnNameList = null)
    {
        MemoryStream ms = new MemoryStream();
        var tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Templates), fileName);
        ExportFileFromDataTable(dataTable, tempPath, sheetName, hasHeader, columnNameList);
        using (var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            fs.CopyTo(ms);
        }
        FileUtil.Remove(tempPath);
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// 导出到excel流
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="fileName"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasHeader"></param>
    /// <param name="columnNameList">自定义全列名</param>
    /// <param name="namePairs">自定义部分转换列名</param>
    public static Stream ExportStreamFromModels<T>(this IEnumerable<T> list, string fileName, string sheetName = "sheet1", bool hasHeader = true, IEnumerable<string>? columnNameList = null, IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        var dataTable = list.ToDataTable(namePairs);

        return ExportStreamFromDataTable(dataTable, fileName, sheetName, hasHeader, columnNameList);
    }

    /// <summary>
    /// 导出到excel文件
    /// </summary>
    /// <param name="dataTable"></param>
    /// <param name="filePath"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasHeader"></param>
    /// <param name="columnNameList">自定义全列名</param>
    /// <returns></returns>
    public static void ExportFileFromDataTable(DataTable dataTable,
        string filePath,
        string sheetName = "sheet1",
        bool hasHeader = true,
        IEnumerable<string>? columnNameList = null)
    {
        int i = 0;

        int count = 0;

        ISheet sheet;

        var fileName = Path.GetFileName(filePath);

        using var workbook = new ExcelBook(fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase));

        using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        try
        {
            if (workbook != null)
            {
                sheet = workbook.CreateSheet(sheetName)!;
            }
            else
            {
                return;
            }

            if (hasHeader == true) //写入DataTable的列名
            {
                IRow row = sheet.CreateRow(0);

                if (columnNameList != null && columnNameList.Any())
                {
                    if (columnNameList.Count() != dataTable.Columns.Count) throw new ArgumentOutOfRangeException("自定义列数与数据源不一致");

                    var j = 0;
                    foreach (var columnName in columnNameList)
                    {
                        row.CreateCell(j).SetCellValue(columnName);
                        j += 1;
                    }
                }
                else
                {
                    for (var j = 0; j < dataTable.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(dataTable.Columns[j].ColumnName);
                    }
                }
                count = 1;
            }
            else
            {
                count = 0;
            }

            for (i = 0; i < dataTable.Rows.Count; ++i)
            {
                IRow row = sheet.CreateRow(count);
                for (var j = 0; j < dataTable.Columns.Count; ++j)
                {
                    row.CreateCell(j).SetCellValue(dataTable.Rows[i][j].ToString());
                }
                ++count;
            }

            workbook.Save(fs);
        }
        catch (Exception ex)
        {
            Logger.Error("ExcelUtil.ExportExcelStream", ex);
        }
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="filePath"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasHeader"></param>
    /// <param name="columnNameList">自定义全列名</param>
    /// <param name="namePairs">自定义部分列名</param>
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
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="groupList"></param>
    /// <param name="filePath"></param>
    /// <param name="hasHeader"></param>
    /// <param name="columnNameList"></param>
    /// <param name="namePairs"></param>
    public static void ExportFileFromModels<Tkey, T>(IEnumerable<IGrouping<Tkey, T>> groupList,
       string filePath,
       bool hasHeader = true,
       IEnumerable<string>? columnNameList = null,
       IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        foreach (var item in groupList)
        {
            ExportFileFromModels(item, filePath, item.Key?.ToString() ?? "", hasHeader, columnNameList, namePairs);
        }
    }

    #region 根据excel模板和参数，填充相关文件并返回       


    /// <summary>
    /// 根据参数(列表数据，条形码)填充excel模板合并sheet返回excel文件流
    /// </summary>
    /// <param name="excelTemplateInfos">模板文件集合替换标签和内容集合</param>
    /// <param name="is2007"></param>
    /// <returns></returns>
    public static Stream GetStreamByTemplatesWithListAndBarCode(ExcelTemplateInfoCollection excelTemplateInfos, bool is2007 = true)
    {
        if (excelTemplateInfos == null || excelTemplateInfos.Count < 1) throw new Exception("Template file stream cannot be null");

        using var mergeWorkBook = new ExcelBook(!is2007, null);

        foreach (var templatInfo in excelTemplateInfos)
        {
            if (templatInfo.TemplateFileStream == null) throw new Exception("Template file stream cannot be null");

            using (var workbook = new ExcelBook(!templatInfo.Is2007, templatInfo.TemplateFileStream))
            {
                var sheet = workbook.GetSheet();

                var rowsNumber = sheet.LastRowNum;

                var namePairs = templatInfo.LableValuePairs;

                var dataListRowStartIndex = templatInfo.DataListRowStartIndex;

                //标签替换

                var hasNamePairs = (namePairs != null && namePairs.Count > 0);

                for (int i = 0; i <= rowsNumber; i++)
                {
                    var row = sheet.GetRow(i);

                    if (row == null) continue;

                    var columnNumber = row.LastCellNum;

                    for (int j = 0; j < columnNumber; j++)
                    {
                        var cell = row.GetCell(j);
                        if (cell == null) continue;
                        var templateName = "";
                        try
                        {
                            templateName = cell.StringCellValue ?? "";
                        }
                        catch { }

                        if (string.IsNullOrEmpty(templateName)) continue;

                        //替换标签中的值
                        if (hasNamePairs && namePairs!.ContainsKey(templateName))
                        {
                            cell.SetCellValue(namePairs[templateName]);
                        }
                        //未指定列表行时加载默认列表行
                        if (dataListRowStartIndex == -1)
                        {
                            if (templateName.Contains("List.")) dataListRowStartIndex = i;
                        }
                    }
                }


                // 设置带列表填充的数据
                if (dataListRowStartIndex > 0 && templatInfo.List != null && templatInfo.List.Count > 0)
                {
                    // 复制列表的行
                    var count = templatInfo.List.Count;

                    for (var i = 1; i < count; i++)
                    {
                        sheet.CopyRow(dataListRowStartIndex, dataListRowStartIndex + i);
                    }

                    for (int j = 0; j < templatInfo.List.Count; j++)
                    {
                        var row = sheet.GetRow(dataListRowStartIndex + j);

                        if (row == null) continue;

                        var columnNumber = row.LastCellNum;

                        var dic = templatInfo.List[j];

                        for (int k = 0; k < columnNumber; k++)
                        {
                            var cell = row.GetCell(k + templatInfo.DataListColumnStartIndex);

                            if (cell == null) continue;

                            var listColumnName = "";

                            try
                            {
                                listColumnName = cell.StringCellValue ?? "";
                            }
                            catch { }

                            if (!string.IsNullOrEmpty(listColumnName) && dic.ContainsKey(listColumnName))
                            {
                                cell.SetCellValue(dic[listColumnName]);
                            }
                        }
                    }
                }

                // 插入条形码
                if (!string.IsNullOrWhiteSpace(templatInfo.BarCodeText))
                {
                    var bytes = CodeUtil.Write(templatInfo.BarCodeText, templatInfo.BarCodeType, templatInfo.BarCodeWidth ?? 0, templatInfo.BarCodeHeight ?? 0).ToBytes();

                    if (bytes != null && bytes.Length > 0)

                        workbook.InsertImage(sheet, bytes, templatInfo.colStart, templatInfo.rowStart, templatInfo.colEnd, templatInfo.rowEnd);
                }

                mergeWorkBook.MergeSheet(sheet, templatInfo.SheetName);
            }
        }
        return mergeWorkBook.SaveAsStream();
    }

    /// <summary>
    /// 根据参数填充excel模板合并sheet返回excel文件
    /// </summary>
    /// <param name="excelTemplateInfos"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static void GetFileByTemplatesForList(ExcelTemplateInfoCollection excelTemplateInfos, string fileName)
    {
        using (var stream = GetStreamByTemplatesWithListAndBarCode(excelTemplateInfos, Path.GetExtension(fileName) == ".xlsx" ? true : false))
        {
            stream.Save(fileName);
        }
    }

    /// <summary>
    /// 根据参数填充excel模板合并sheet返回excel文件流
    /// </summary>
    /// <param name="excelTemplateInfos">模板文件集合替换标签和内容集合</param>
    /// <param name="is2007">默认xlsx</param>
    /// <returns></returns>
    [Obsolete("调整名称为GetStreamByTemplatesWithListAndBarCode")]
    public static Stream GetStreamByTemplates(ExcelTemplateInfoCollection excelTemplateInfos, bool is2007 = true)
    {
        return GetStreamByTemplatesWithListAndBarCode(excelTemplateInfos, is2007);
    }

    /// <summary>
    /// 根据参数填充excel模板合并sheet返回excel文件
    /// </summary>
    /// <param name="excelTemplateInfos"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [Obsolete("调整名称为GetFileByTemplatesForList")]
    public static void GetFileByTemplates(ExcelTemplateInfoCollection excelTemplateInfos, string fileName)
    {
        using (var stream = GetStreamByTemplates(excelTemplateInfos, Path.GetExtension(fileName) == ".xlsx" ? true : false))
        {
            stream.Save(fileName);
        }
    }

    /// <summary>
    /// 根据参数(列表数据，条形码)填充excel模板合并sheet返回excel文件流
    /// </summary>
    /// <param name="excelTemplateInfos"></param>
    /// <param name="is2007"></param>
    /// <returns></returns>
    [Obsolete("调整名称为GetStreamByTemplatesWithListAndBarCode")]
    public static Stream GetStreamByTemplatesAndListBarCode(ExcelTemplateInfoCollection excelTemplateInfos, bool is2007 = true)
    {
        return GetStreamByTemplatesWithListAndBarCode(excelTemplateInfos, is2007);
    }
    #endregion

    #region 合并excel

    /// <summary>
    /// 将不同的excel文件合并成一个文件
    /// </summary>
    /// <param name="excelFiles"></param>
    /// <param name="v2003"></param>
    /// <returns></returns>
    public static Stream MergeExcelFileByStream(Dictionary<string, Stream> excelFiles, bool v2003 = false)
    {
        using (var excel = new ExcelBook(v2003))
        {
            foreach (var item in excelFiles)
            {
                using (var subExcel = new ExcelBook(v2003, item.Value))
                {
                    var sheet = subExcel.GetSheet(item.Key);
                    excel.MergeSheet(sheet, item.Key);
                }
            }
            return excel.SaveAsStream();
        }
    }

    /// <summary>
    /// 将不同的excel文件合并成一个文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="excelFiles">dic,key:sheetName,value:filePath</param>
    /// <returns></returns>
    public static void MergeExcelFile(string filePath, Dictionary<string, string> excelFiles)
    {
        bool v2003 = filePath.EndsWith(".xls");

        using (var excel = new ExcelBook(v2003))
        {
            foreach (var item in excelFiles)
            {
                var subV2003 = item.Value.EndsWith(".xls");
                var fileStream = FileUtil.GetStream(item.Value);
                using (var subExcel = new ExcelBook(subV2003, fileStream))
                {
                    var sheet = subExcel.GetSheet(item.Key);
                    excel.MergeSheet(sheet, item.Key);
                }
            }
            excel.Save(filePath);
        }
    }

    #endregion
}

#region excel工作薄
/// <summary>
/// excel工作薄
/// </summary>
public class ExcelBook : IDisposable
{
    IWorkbook _book;

    bool _v2003 = true;

    /// <summary>
    /// excel工作薄
    /// </summary>
    /// <param name="v2003"></param>
    /// <param name="stream"></param>
    public ExcelBook(bool v2003, Stream? stream = null)
    {
        _v2003 = v2003;

        if (_v2003)
        {
            if (stream != null)
                _book = new HSSFWorkbook(stream);
            else
                _book = new HSSFWorkbook();
        }
        else
        {
            if (stream != null)
                _book = new XSSFWorkbook(stream);
            else
                _book = new XSSFWorkbook();
        }
        if (_book == null) throw new Exception("Invalid stream");
    }

    /// <summary>
    /// 创建表格
    /// </summary>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public ISheet? CreateSheet(string sheetName)
    {
        if (!string.IsNullOrEmpty(sheetName))
            return _book.CreateSheet(sheetName);
        return null;
    }

    /// <summary>
    /// 获取表格
    /// </summary>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public ISheet GetSheet(string sheetName = "")
    {
        ISheet sheet;
        if (!string.IsNullOrEmpty(sheetName))
        {
            sheet = _book.GetSheet(sheetName);
        }
        else
        {
            sheet = _book.GetSheetAt(0);
        }

        if (sheet == null) throw new Exception("Template sheet cannot be null");

        return sheet;
    }

    /// <summary>
    /// 插入图片
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="bytes"></param>
    /// <param name="colStart"></param>
    /// <param name="rowStart"></param>
    /// <param name="colEnd"></param>
    /// <param name="rowEnd"></param>
    public void InsertImage(ISheet sheet, byte[] bytes, int colStart, int rowStart, int colEnd, int rowEnd)
    {
        var patriarch = sheet.CreateDrawingPatriarch();

        int pictureIdx = _book.AddPicture(bytes, PictureType.JPEG);
        //把图片插到相应的位置
        if (!_v2003) // 2007版本
        {
            XSSFClientAnchor anchor = new XSSFClientAnchor(0, 0, 0, 0, colStart, rowStart, colEnd, rowEnd);
            var pict = (XSSFPicture)patriarch.CreatePicture(anchor, pictureIdx);
        }
        else // 2003版本
        {

            HSSFClientAnchor anchor = new HSSFClientAnchor(0, 0, 0, 0, colStart, rowStart, colEnd, rowEnd);
            var pict = (HSSFPicture)patriarch.CreatePicture(anchor, pictureIdx);
        }
    }

    /// <summary>
    /// 合并表格
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="sheetName"></param>
    public void MergeSheet(ISheet sheet, string sheetName = "")
    {
        if (_book == null) throw new Exception("Template book cannot be null");

        if (sheet == null) throw new Exception("Template sheet cannot be null");

        if (string.IsNullOrEmpty(sheetName))
        {
            sheetName = sheet.SheetName;
        }
        if (string.IsNullOrEmpty(sheetName))
        {
            sheetName = "sheet1";
        }
        sheet.CopyTo(_book, sheetName, true, true);
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="fs"></param>
    public void Save(FileStream fs)
    {
        _book.Write(fs);
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="filePath"></param>
    public void Save(string filePath)
    {
        using (var fs = FileUtil.GetStream(filePath))
        {
            Save(fs);
        }
    }

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public Stream SaveAsStream(string filePath = "")
    {
        if (string.IsNullOrEmpty(filePath))
            filePath = FileUtil.GetCurrentFile($"{DateTimeUtil.Now:yyyyMMddHHmmssfff}{(!_v2003 ? ".xlsx" : ".xls")}");

        using (var fs1 = FileUtil.GetStream(filePath))
        {
            _book.Write(fs1);
        }
        return FileUtil.GetMemoryStream(filePath, true);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _book.Close();
    }
}

#endregion
