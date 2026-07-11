/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： IApplyData
*版本号： V1.0.0.0
*唯一标识：7744a842-ca82-48f2-b6aa-d3a8c68435c1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/1/8 10:44:17
*描述：轻流应用数据
*
*=================================================
*修改标记
*修改时间：2025/1/8 10:44:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：轻流应用数据
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;

/// <summary>
/// 轻流应用数据
/// </summary>
public interface IAppData
{
    /// <summary>
    /// 数据id
    /// </summary>
    int? AppDataId { get; set; }
}
