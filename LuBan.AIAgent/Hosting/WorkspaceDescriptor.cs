namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 工作区的中立描述（框架不依赖产品的工作区模型）。
/// </summary>
/// <param name="Id">工作区标识。</param>
/// <param name="Name">工作区名称。</param>
/// <param name="RootPath">工作区根目录绝对路径。</param>
/// <param name="Type">工作区类型（产品自定义，如 <c>normal</c> / <c>llmwiki</c>）。</param>
public sealed record WorkspaceDescriptor(string Id, string Name, string RootPath, string Type);