/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.YiBai.Models
*文件名： YBDatainfo
*版本号： V1.0.0.0
*唯一标识：666f0ab7-42d8-434b-94e0-00145a2a1c21
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/18 18:20:00
*描述：YBDatainfo 模型类
*
*=================================================
*修改标记
*修改时间：2024/9/18 18:20:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：YBDatainfo 模型类
*
*****************************************************************************/
namespace LuBan.Lives.YiBai.Models;

/// <summary>
/// YBDatainfo 模型类
/// </summary>

/// <summary>
/// YiBai 直播接口的通用返回数据结构。
/// </summary>
public class YBDatainfo
{
    /// <summary>
    /// 接口响应状态码（接口字段 code）。
    /// </summary>
    public int code { get; set; }
    /// <summary>
    /// 接口响应提示信息（接口字段 message）。
    /// </summary>
    public string? message { get; set; }
    /// <summary>
    /// 接口返回的业务数据（接口字段 data）。
    /// </summary>
    public string data { get; set; }
}
