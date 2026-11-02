/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.Orm.Enums
*文件名： EnumBehaviorLogSource
*描述：行为日志来源枚举
*
*****************************************************************************/

namespace LuBan.Orm.Enums;

/// <summary>
/// 行为日志来源枚举
/// </summary>
[Description("行为日志来源枚举")]
public enum EnumBehaviorLogSource
{
    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他")]
    Other = 0,

    /// <summary>
    /// 管理后台
    /// </summary>
    [Description("管理后台")]
    Admin = 1,

    /// <summary>
    /// 移动端
    /// </summary>
    [Description("移动端")]
    Mobile = 2,

    /// <summary>
    /// 开放接口
    /// </summary>
    [Description("开放接口")]
    Open = 3,

    /// <summary>
    /// 系统
    /// </summary>
    [Description("系统")]
    System = 4,
}