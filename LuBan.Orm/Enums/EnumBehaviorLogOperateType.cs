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
*文件名： EnumBehaviorLogOperateType
*描述：行为日志操作类型枚举
*
*****************************************************************************/

namespace LuBan.Orm.Enums;

/// <summary>
/// 行为日志操作类型枚举
/// </summary>
[Description("行为日志操作类型枚举")]
public enum EnumBehaviorLogOperateType
{
    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他")]
    Other = 0,

    /// <summary>
    /// 点赞
    /// </summary>
    [Description("点赞")]
    Praise = 1,

    /// <summary>
    /// 阅读
    /// </summary>
    [Description("阅读")]
    Read = 2,

    /// <summary>
    /// 收藏
    /// </summary>
    [Description("收藏")]
    Collection = 3,

    /// <summary>
    /// 分享
    /// </summary>
    [Description("分享")]
    Share = 4,

    /// <summary>
    /// 预订
    /// </summary>
    [Description("预订")]
    Booking = 5,

    /// <summary>
    /// 预约
    /// </summary>
    [Description("预约")]
    Reservation = 6,

    /// <summary>
    /// 取消预约
    /// </summary>
    [Description("取消预约")]
    CancelReservation = 7,

    /// <summary>
    /// 观看进度
    /// </summary>
    [Description("观看进度")]
    Process = 8,

    /// <summary>
    /// 新增
    /// </summary>
    [Description("新增")]
    Add = 10,

    /// <summary>
    /// 修改
    /// </summary>
    [Description("修改")]
    Edit = 11,

    /// <summary>
    /// 删除
    /// </summary>
    [Description("删除")]
    Delete = 12,

    /// <summary>
    /// 授权
    /// </summary>
    [Description("授权")]
    Grant = 13,

    /// <summary>
    /// 启用
    /// </summary>
    [Description("启用")]
    Enable = 14,

    /// <summary>
    /// 停用
    /// </summary>
    [Description("停用")]
    Disable = 15,

    /// <summary>
    /// 重置密码
    /// </summary>
    [Description("重置密码")]
    ResetPassword = 16,

    /// <summary>
    /// 导入
    /// </summary>
    [Description("导入")]
    Import = 17,

    /// <summary>
    /// 导出
    /// </summary>
    [Description("导出")]
    Export = 18,

    /// <summary>
    /// 取消授权
    /// </summary>
    [Description("取消授权")]
    Revoke = 19,
}