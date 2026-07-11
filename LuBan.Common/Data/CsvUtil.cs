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
*描述：CSV文件转换类
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 10:46:31
*修改人： walle.wen
*版本号： V1.0.0.0
*描述： CSV文件转换类
*
*****************************************************************************/
namespace LuBan.Common.Data;

/// <summary>
/// CSV文件转换类
/// </summary>
public static class CsvUtil
{
    /// <summary>
    /// 从datatable中导出csv流
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="splitStr">分隔符,因为逗号经常在字段中使用，建议使用</param>
    /// <param name="columnNameList">自定义列名</param>
    /// <returns></returns>
    public static Stream ExportFromDataTable(DataTable dt, string splitStr = ",", IEnumerable<string>? columnNameList = null)
    {
        if (dt == null || dt.Rows == null || dt.Rows.Count == 0) throw new ArgumentNullException("传入的Datatable不能为空");

        var memoryStream = new MemoryStream();

        if (columnNameList != null)
        {
            if (columnNameList.Count() != dt.Columns.Count) throw new ArgumentOutOfRangeException("自定义列数与数据源不一致");

            foreach (var columnName in columnNameList)
            {
                if (columnNameList.First() == columnName)
                {
                    memoryStream.Write(Encoding.UTF8.GetBytes(HandleSpecialCharacters(columnName)));
                }
                else
                {
                    memoryStream.Write(Encoding.UTF8.GetBytes(splitStr));
                    memoryStream.Write(Encoding.UTF8.GetBytes(HandleSpecialCharacters(columnName)));
                }
            }
        }
        else
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                memoryStream.Write(Encoding.UTF8.GetBytes(HandleSpecialCharacters(dt.Columns[i].ColumnName.ToString())));
                if (i < dt.Columns.Count - 1)
                {
                    memoryStream.Write(Encoding.UTF8.GetBytes(splitStr));
                }
            }
        }

        memoryStream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                memoryStream.Write(Encoding.UTF8.GetBytes(HandleSpecialCharacters(dt.Rows[i][j]?.ToString() ?? "")));
                if (j < dt.Columns.Count - 1)
                {
                    memoryStream.Write(Encoding.UTF8.GetBytes(splitStr));
                }
            }
            memoryStream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// 处理特殊字符
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string HandleSpecialCharacters(string input)
    {
        if (input.IsNullOrEmpty()) return string.Empty;
        if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
        {
            input = input.Replace("\"", "\"\"");
            input = $"\"{input}\"";
        }
        return input;
    }
    /// <summary>
    /// 写入CSV
    /// </summary>
    /// <param name="filePath">文件名</param>
    /// <param name="dt">要写入的datatable</param>
    /// <param name="splitStr">分隔符,因为逗号经常在字段中使用，建议使用\t</param>
    /// <param name="columnNameList">自定义列名</param>
    public static void ExportFromDataTable(string filePath, DataTable dt, string splitStr = ",", IEnumerable<string>? columnNameList = null)
    {
        using var stream = ExportFromDataTable(dt, splitStr, columnNameList);
        using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        stream.CopyTo(fs);
    }

    /// <summary>
    /// 从文件中读取CSV文件到DataTable
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="splitStr">分隔符,因为逗号经常在字段中使用，建议使用\t</param>
    /// <param name="columnNameList">自定义列名</param>
    /// <returns></returns>
    public static DataTable? ImportToDataTable(string filePath, string splitStr = ",", IEnumerable<string>? columnNameList = null)
    {
        if (!File.Exists(filePath)) return null;
        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
        return ImportFromStream(fs, splitStr, columnNameList);
    }

    /// <summary>
    /// 从流中读取CSV文件到DataTable
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="splitStr"></param>
    /// <param name="columnNameList"></param>
    /// <returns></returns>
    public static DataTable? ImportFromStream(Stream stream, string splitStr = ",", IEnumerable<string>? columnNameList = null)
    {
        if (stream == null || stream.Length < 1) return null;
        using StreamReader sr = new(stream, Encoding.UTF8);
        return ImportToDataTableFromString(sr.ReadToEnd(), splitStr, columnNameList);
    }

    /// <summary>
    /// 从string中读取CSV文件到DataTable
    /// </summary>
    /// <param name="csvString"></param>
    /// <param name="splitStr">分隔符,因为逗号经常在字段中使用，建议使用\t</param>
    /// <param name="columnNameList">自定义列名</param>
    /// <returns></returns>
    public static DataTable? ImportToDataTableFromString(string csvString, string splitStr = ",", IEnumerable<string>? columnNameList = null)
    {
        if (string.IsNullOrEmpty(csvString)) return null;

        var lines = csvString.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        if (lines != null && lines.Length > 2)
        {
            DataTable dt = new();

            //记录每行记录中的
            //判断，若是第一次，建立表头
            bool isFirst = true;
            foreach (var strLine in lines)
            {
                string[] arrayLine = strLine.Trim().Split(splitStr, StringSplitOptions.RemoveEmptyEntries);//分隔字符串，返回数组

                int dtColumns = arrayLine.Length;//列的个数

                if (isFirst)  //建立表头
                {
                    if (columnNameList != null)
                    {
                        if (columnNameList.Count() != dtColumns) throw new ArgumentOutOfRangeException("自定义列数与数据源不一致");

                        foreach (var columnName in columnNameList)
                        {
                            dt.Columns.Add(columnName);//每一列名称
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dtColumns; i++)
                        {
                            dt.Columns.Add(arrayLine[i]);//每一列名称
                        }
                    }

                }
                else //表内容
                {
                    DataRow dataRow = dt.NewRow();//新建一行
                    for (int j = 0; j < dtColumns; j++)
                    {
                        dataRow[j] = arrayLine[j];
                    }
                    dt.Rows.Add(dataRow);//添加一行
                }
            }

            return dt;
        }
        return null;
    }

    /// <summary>
    /// 读取CSV文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="splitStr">分隔符,因为逗号经常在字段中使用，建议使用\t</param>
    /// <param name="columnNameList">自定义列名</param>
    /// <param name="namePairs">自定义部分列名</param>
    /// <returns></returns>
    public static IList<T> ImportToEntities<T>(string filePath,
        string splitStr = ",",
        IEnumerable<string>? columnNameList = null,
        IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        var dt = ImportToDataTable(filePath, splitStr, columnNameList);
        if (dt == null) return [];
        return dt.ToList<T>(namePairs);
    }

    /// <summary>
    /// 导出到Csv
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="splitStr">分隔符,因为逗号经常在字段中使用，建议使用\t</param>
    /// <param name="columnNameList">自定义列名</param>
    /// <param name="namePairs">自定义转换列名</param>
    /// <returns></returns>
    public static Stream ExportStreamFromModels<T>(this IEnumerable<T> data, string splitStr = ",", IEnumerable<string>? columnNameList = null, IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        var dt = data.ToDataTable(namePairs);
        return ExportFromDataTable(dt, splitStr, columnNameList);
    }

    /// <summary>
    /// 从多个csv string中合并后转换成流
    /// </summary>
    /// <param name="strings"></param>
    /// <returns></returns>
    public static Stream MergeFromString(params string[] strings)
    {
        MemoryStream memoryStream = new();

        if (strings != null && strings.Length > 0)
        {
            bool isFirstBlock = true;

            foreach (var str in strings)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    //直接添加第一文本块
                    if (isFirstBlock)
                    {
                        isFirstBlock = false;
                        var buffer = Encoding.UTF8.GetBytes(str);
                        memoryStream.Write(buffer);
                    }
                    else
                    {
                        var strs = str.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                        if (strs != null && strs.Length >= 2)
                        {
                            var isFirstLine = true;
                            foreach (var strLine in strs)
                            {
                                //不读取第一行
                                if (isFirstLine)
                                {
                                    isFirstLine = false;
                                }
                                else
                                {
                                    var buffer = Encoding.UTF8.GetBytes(strLine);
                                    memoryStream.Write(buffer);
                                }
                            }
                        }
                    }
                }
            }
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// 将二进制字节转换成datatable
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static DataTable? ToDataTable(byte[] bytes)
    {
        if (bytes == null)
            return null;
        using (var ms = new MemoryStream(bytes))
        {
            return ImportFromStream(ms);
        }
    }
}
