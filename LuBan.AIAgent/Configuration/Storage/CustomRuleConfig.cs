/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： CustomRuleConfig
*版本号： V1.0.0.0
*唯一标识：67735ade-7589-47a4-a31b-dc22dcc708f1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：自定义规则配置
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：自定义规则配置
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 自定义规则配置（模式匹配型）
/// </summary>
public class CustomRuleConfig
{
    /// <summary>
    /// 规则唯一标识
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// 规则名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 规则描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 动作类型匹配模式（支持通配符）
    /// </summary>
    public string ActionTypePattern { get; set; } = "*";

    /// <summary>
    /// 目标匹配模式（支持通配符）
    /// </summary>
    public string TargetPattern { get; set; } = "*";

    /// <summary>
    /// 命中后的动作（如 allow, deny）
    /// </summary>
    public string Action { get; set; } = "deny";

    /// <summary>
    /// 优先级（数字越大优先级越高，与 IRule.Priority 语义一致）
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
