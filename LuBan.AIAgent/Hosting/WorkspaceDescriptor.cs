/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： WorkspaceDescriptor
*版本号： V1.0.0.0
*唯一标识：463e091f-163c-4849-a357-80d6af84a4e0
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：工作区描述模型（Id / Name / RootPath / Type）
*
*****************************************************************************/namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 工作区的中立描述（框架不依赖产品的工作区模型）。
/// </summary>
/// <param name="Id">工作区标识。</param>
/// <param name="Name">工作区名称。</param>
/// <param name="RootPath">工作区根目录绝对路径。</param>
/// <param name="Type">工作区类型（产品自定义，如 <c>normal</c> / <c>llmwiki</c>）。</param>
public sealed record WorkspaceDescriptor(string Id, string Name, string RootPath, string Type);