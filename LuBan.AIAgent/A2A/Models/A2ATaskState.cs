/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Models
*文件名： A2ATaskState
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 任务状态九态，与协议 TaskState 对齐，SDK 无关
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A.Models;

/// <summary>
/// A2A 任务状态。取值与官方协议 <c>TaskState</c> 对齐（序列化名 <c>TASK_STATE_*</c>），
/// 但本枚举为框架原生定义，不依赖 A2A SDK。
/// </summary>
public enum A2ATaskState
{
    /// <summary>未指定。</summary>
    Unspecified = 0,

    /// <summary>已提交，尚未开始。</summary>
    Submitted = 1,

    /// <summary>执行中。</summary>
    Working = 2,

    /// <summary>已成功完成（终态）。</summary>
    Completed = 3,

    /// <summary>失败（终态）。</summary>
    Failed = 4,

    /// <summary>已取消（终态）。</summary>
    Canceled = 5,

    /// <summary>需要补充输入（中断态）。</summary>
    InputRequired = 6,

    /// <summary>被拒绝（终态）。</summary>
    Rejected = 7,

    /// <summary>需要认证（中断态）。</summary>
    AuthRequired = 8
}
