/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： QResult
*版本号： V1.0.0.0
*唯一标识：5d69a06a-efb7-42ab-b1e5-8946476770a5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/23 17:40:42
*描述：返回结果
*
*=================================================
*修改标记
*修改时间：2024/12/23 17:40:42
*修改人： yswenli
*版本号： V1.0.0.0
*描述：返回结果
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;

/// <summary>
/// 返回结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class QResult<T>
{
    /// <summary>
    /// 错误码，非0表示失败
    /// </summary>
    public int ErrCode { get; set; }
    /// <summary>
    /// 错误描述
    /// </summary>
    public string? ErrMsg { get; set; }
    /// <summary>
    /// 返回结果
    /// </summary>
    public T Result { get; set; }
}
