/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Core
*文件名： ReportCsv
*版本号： V1.0.0.0
*唯一标识：8e58663a-a3da-4d5c-acac-b6bb4bd3b5f2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/24 11:27:28
*描述：
*
*=================================================
*修改标记
*修改时间：2025/2/24 11:27:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Reporting.Core;

/// <summary>
/// CSV格式的报告类
/// </summary>
/// <typeparam name="T">报告数据类型</typeparam>
internal class ReportCsv<T> : ReportBase<T> where T : class, new()
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="data">报告数据</param>
    public ReportCsv(List<T> data) : base(data)
    {

    }


    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="filePath"></param>
    public override void Save(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var path = Path.GetDirectoryName(filePath);
        if (path.IsNotNullOrEmpty() && !Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        using (var stream = SaveStream())
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fs);
                fs.Position = 0;
                fs.Flush();
            }
        }

    }

    /// <summary>
    /// 保存报告到流
    /// </summary>
    /// <returns></returns>
    public override Stream SaveStream()
    {
        var dataTable = GetDataTable();
        return CsvUtil.ExportFromDataTable(dataTable);
    }
}
