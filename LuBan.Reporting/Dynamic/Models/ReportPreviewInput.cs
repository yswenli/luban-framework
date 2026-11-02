/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Dynamic.Models
*文件名： ReportPreviewInput
*版本号： V1.0.0.0
*唯一标识：6cfba44b-93a9-4583-a759-e5a96df98ff8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：预览请求参数
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：预览请求参数
*
*****************************************************************************/

namespace LuBan.Reporting.Dynamic.Models;

/// <summary>
/// 预览请求参数
/// </summary>
public class ReportPreviewInput
{
    /// <summary>
    /// 报表配置ID
    /// </summary>
    public long ReportConfigId { get; set; }

    /// <summary>
    /// SQL 参数
    /// </summary>
    public Dictionary<string, object>? SqlParameters { get; set; }

    /// <summary>
    /// 预览行数（覆盖默认值）
    /// </summary>
    public int? PreviewRows { get; set; }
}
