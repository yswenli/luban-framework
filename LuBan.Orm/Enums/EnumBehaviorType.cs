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
*文件名： EnumBehaviorType
*描述：行为对象类型枚举
*
*****************************************************************************/

namespace LuBan.Orm.Enums;

/// <summary>
/// 行为对象类型枚举
/// </summary>
[Description("行为对象类型枚举")]
public enum EnumBehaviorType
{
    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他")]
    Other = 0,

    /// <summary>
    /// 文章
    /// </summary>
    [Description("文章")]
    Article = 1,

    /// <summary>
    /// 活动
    /// </summary>
    [Description("活动")]
    Active = 2,

    /// <summary>
    /// 直播
    /// </summary>
    [Description("直播")]
    Live = 3,

    /// <summary>
    /// 课程
    /// </summary>
    [Description("课程")]
    Lesson = 4,
}