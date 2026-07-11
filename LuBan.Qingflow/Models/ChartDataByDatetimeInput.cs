/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： ChartDataByDatetimeInput
*版本号： V1.0.0.0
*唯一标识：6b9e0ebb-3f9e-4878-bde3-b598f6d3c8b8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/26 18:26:33
*描述：
*
*=================================================
*修改标记
*修改时间：2025/3/26 18:26:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;

/// <summary>
/// 表示按日期时间获取图表数据的输入数据。
/// </summary>
public class ChartDataByDatetimeInput
{
    /// <summary>
    /// 获取或设置页码。
    /// </summary>
    public int PageNum { get; set; }

    /// <summary>
    /// 获取或设置页面大小。
    /// </summary>
    public int PageSize { get; set; } = 200; //注意pageSize最大可填200

    /// <summary>
    /// 获取或设置图表键。
    /// </summary>
    public string ChartKey { get; set; }

    /// <summary>
    /// 获取或设置队列ID。
    /// </summary>
    public int QueId { get; set; }

    /// <summary>
    /// 获取或设置开始时间。
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 获取或设置结束时间。
    /// </summary>
    public DateTime EndTime { get; set; }
    /// <summary>
    /// 模糊搜索
    /// </summary>
    public string? QueryKey { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否包含所有数据。
    /// </summary>
    public bool IsAll { get; set; }
}
