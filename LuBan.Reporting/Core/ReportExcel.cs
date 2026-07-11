/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Core
*文件名： ReportExcel
*版本号： V1.0.0.0
*唯一标识：38d2befe-8e08-4232-88a4-258bd1a92f57
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/24 14:22:50
*描述：
*
*=================================================
*修改标记
*修改时间：2025/2/24 14:22:50
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Reporting.Core;

/// <summary>
/// ReportExcel
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ReportExcel<T> : ReportBase<T> where T : class, new()
{
    /// <summary>
    /// ReportExcel
    /// </summary>
    /// <param name="data"></param>
    public ReportExcel(List<T> data) : base(data)
    {
        Data = data;
    }


    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="filePath"></param>
    public override void Save(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var is2003 = fileName.EndsWith(".xls");
        var path = Path.GetDirectoryName(filePath);
        if (path.IsNotNullOrEmpty() && !Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var dataTable = GetDataTable();
        if (is2003)
        {
            ExcelUtil.ExportFileFromDataTable(dataTable, filePath);
        }
        else
        {
            ExcelUtil.ExportFileFromDataTable(dataTable, filePath);
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <returns></returns>
    public override Stream SaveStream()
    {
        return SaveStream(false);
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="is2003"></param>
    /// <returns></returns>
    Stream SaveStream(bool is2003)
    {
        var dataTable = GetDataTable();
        if (is2003)
        {
            return ExcelUtil.ExportStreamFromDataTable(dataTable, $"{typeof(T).Name}-{DateTime.Now:yyyyMMddHHmmss}.xls");
        }
        return ExcelUtil.ExportStreamFromDataTable(dataTable, $"{typeof(T).Name}-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }
}
