/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Models
*文件名： DbLogOptions
*版本号： V1.0.0.0
*唯一标识：02ea0006-9d75-4f88-abde-457d3193b930
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/12/3 14:31:43
*描述：日志配置
*
*=================================================
*修改标记
*修改时间：2025/12/3 14:31:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志配置
*
*****************************************************************************/
namespace LuBan.Orm.Models;

/// <summary>
/// 日志配置
/// </summary>
public class DbLogOptions
{
    /// <summary>
    /// api日志最大条数
    /// </summary>
    public int ApiLogMaxSize { get; set; } = 1024 * 10;

    /// <summary>
    /// api日志过期时间，单位秒
    /// </summary>
    public int ApiLogExpiredSeconds { get; set; } = 60 * 60 * 24 * 7;
    /// <summary>
    /// 错误日志最大条数
    /// </summary>
    public int ErrorLogMaxSize { get; set; } = 1024;
    /// <summary>
    /// 错误日志过期时间，单位秒
    /// </summary>
    public int ErrorLogExpiredSeconds { get; set; } = 60 * 60 * 24 * 7;
}
