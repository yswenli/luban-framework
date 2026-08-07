/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Planner
*文件名： TaskGraphTemplate
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：任务图谱模板，存储在 Templates/*.json 中
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Planner;

/// <summary>
/// 任务图谱模板，存储在 Templates/*.json 中。
/// </summary>
public class TaskGraphTemplate
{
    /// <summary>
    /// 获取或设置模板标识。
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// 获取或设置模板名称。
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 获取或设置模板描述，供 LLM 匹配。
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 获取或设置关键词集合。
    /// </summary>
    public string[] Keywords { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 获取或设置原型图谱，含 {param:xxx} 占位符。
    /// </summary>
    public TaskGraph Prototype { get; set; } = new();

    /// <summary>
    /// 获取或设置参数列表。
    /// </summary>
    public List<TemplateParameter> Parameters { get; set; } = new();

    /// <summary>
    /// 根据参数实例化图谱。
    /// </summary>
    /// <param name="parameters">参数字典。</param>
    /// <returns>填充后的 TaskGraph。</returns>
    public TaskGraph Instantiate(Dictionary<string, string> parameters)
    {
        var json = Prototype.ToJson();
        foreach (var (k, v) in parameters)
            json = json.Replace($"{{param:{k}}}", v);
        var graph = json.ToObject<TaskGraph>()!;
        graph.Source = "template";
        return graph;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };
}

/// <summary>
/// 模板参数定义。
/// </summary>
public class TemplateParameter
{
    /// <summary>
    /// 获取或设置参数名。
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 获取或设置参数描述。
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 获取或设置是否必填。
    /// </summary>
    public bool Required { get; set; } = true;
}
