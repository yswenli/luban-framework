/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： TaskPlanningException
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：任务规划异常，在 DAG 生成或校验失败时抛出
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// 任务规划异常，在 DAG 生成或校验失败时抛出。
/// </summary>
public class TaskPlanningException : Exception
{
    /// <summary>
    /// 获取校验错误信息列表。
    /// </summary>
    public List<string> ValidationErrors { get; }

    /// <summary>
    /// 创建 TaskPlanningException 实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    /// <param name="validationErrors">校验错误信息列表。</param>
    public TaskPlanningException(string message, List<string>? validationErrors = null)
        : base(message)
    {
        ValidationErrors = validationErrors ?? new();
    }
}
