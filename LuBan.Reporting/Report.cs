/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting
*文件名： Report
*版本号： V1.0.0.0
*唯一标识：8648645f-560f-4ccc-bc6b-319a3ff7a9b5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/24 10:49:32
*描述：报告
*
*=================================================
*修改标记
*修改时间：2025/2/24 10:49:32
*修改人： yswenli
*版本号： V1.0.0.0
*描述：报告
*
*****************************************************************************/


namespace LuBan.Reporting;

/// <summary>
/// 报告
/// </summary>
/// <typeparam name="T"></typeparam>
public class Report<T> : IDisposable where T : class, new()
{
    List<T> _data;

    /// <summary>
    /// 报告
    /// </summary>
    public Report(List<T> data)
    {
        if (data == null || data.Count < 1) throw new Exception("data is null or data.count<1");
        _data = data;
    }


    /// <summary>
    /// 导出到文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public void Export(string filePath)
    {
        if (filePath.EndsWith(".csv"))
        {
            new ReportCsv<T>(_data).Save(filePath);
            return;
        }
        if (filePath.EndsWith(".xlsx") || filePath.EndsWith(".xls"))
        {
            new ReportExcel<T>(_data).Save(filePath);
            return;
        }
        throw new Exception("file type is not support");
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Dispose()
    {
        _data.Clear();
    }
}
