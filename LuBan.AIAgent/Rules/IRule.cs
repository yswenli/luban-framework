/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules
*文件名： IRule
*版本号： V1.0.0.0
*唯一标识：38cc0f68-8e77-4ccc-a6d6-fa17f71ee519
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：规则接口定义
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：规则接口定义
*
*****************************************************************************/

namespace LuBan.AIAgent.Rules;

/// <summary>
/// 规则接口 - 定义执行条件和行为
/// </summary>
public interface IRule
{
    /// <summary>
    /// 规则 ID
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 规则名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 规则描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 规则优先级（数字越大优先级越高）
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// 规则是否启用
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// 检查规则是否适用
    /// </summary>
    /// <param name="context">规则上下文</param>
    /// <returns>是否适用</returns>
    bool IsApplicable(RuleContext context);

    /// <summary>
    /// 执行规则
    /// </summary>
    /// <param name="context">规则上下文</param>
    /// <returns>规则执行结果</returns>
    Task<RuleResult> ExecuteAsync(RuleContext context);
}

/// <summary>
/// 规则上下文
/// </summary>
public class RuleContext
{
    /// <summary>
    /// 操作类型（如：tool-call, file-write, script-execute）
    /// </summary>
    public string ActionType { get; set; } = "";

    /// <summary>
    /// 目标对象（如：工具名称、文件路径）
    /// </summary>
    public string Target { get; set; } = "";

    /// <summary>
    /// 参数
    /// </summary>
    public Dictionary<string, object?> Arguments { get; set; } = new();

    /// <summary>
    /// 用户输入
    /// </summary>
    public string? UserInput { get; set; }

    /// <summary>
    /// Agent 实例
    /// </summary>
    public LuBanAgent? Agent { get; set; }

    /// <summary>
    /// 扩展属性
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// 规则执行结果
/// </summary>
public class RuleResult
{
    /// <summary>
    /// 是否允许继续执行
    /// </summary>
    public bool Allow { get; set; } = true;

    /// <summary>
    /// 是否需要修改参数
    /// </summary>
    public bool Modified { get; set; }

    /// <summary>
    /// 修改后的参数（如果 Modified 为 true）
    /// </summary>
    public Dictionary<string, object?>? ModifiedArguments { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 执行的其他操作
    /// </summary>
    public List<string> Actions { get; set; } = new();

    /// <summary>
    /// 需要注入到上下文的文本列表（context-build 使用）
    /// </summary>
    public List<string> Inject { get; set; } = new();

    /// <summary>
    /// 创建允许结果
    /// </summary>
    public static RuleResult AllowResult(string? message = null) => new() { Allow = true, Message = message };

    /// <summary>
    /// 创建拒绝结果
    /// </summary>
    public static RuleResult DenyResult(string message) => new() { Allow = false, Message = message };

    /// <summary>
    /// 创建修改参数结果
    /// </summary>
    public static RuleResult ModifyResult(Dictionary<string, object?> arguments, string? message = null)
        => new() { Allow = true, Modified = true, ModifiedArguments = arguments, Message = message };
}
