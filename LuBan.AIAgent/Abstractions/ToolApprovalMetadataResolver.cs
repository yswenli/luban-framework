namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具审批元数据解析器，用于解析工具的审批模式
/// </summary>
public static class ToolApprovalMetadataResolver
{
    /// <summary>
    /// 解析工具的审批模式
    /// </summary>
    /// <param name="tool">工具实例</param>
    /// <returns>工具审批模式</returns>
    public static ToolApprovalMode ResolveApprovalMode(ITool tool)
    {
        if (tool is IApprovalAwareTool approvalAwareTool)
        {
            return approvalAwareTool.ApprovalMode;
        }

        var attribute = Attribute.GetCustomAttribute(tool.GetType(), typeof(NeedApprovalAttribute), inherit: true) as NeedApprovalAttribute;
        return attribute?.ApprovalMode ?? ToolApprovalMode.None;
    }
}
