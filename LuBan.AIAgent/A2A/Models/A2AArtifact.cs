/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Models
*文件名： A2AArtifact
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 任务产出物，框架原生、SDK 无关
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A.Models;

/// <summary>
/// A2A 任务产出物。对应协议 <c>Artifact</c>。
/// </summary>
public class A2AArtifact
{
    /// <summary>产出物标识。</summary>
    public string ArtifactId { get; set; } = "";

    /// <summary>产出物名称。</summary>
    public string? Name { get; set; }

    /// <summary>产出物描述。</summary>
    public string? Description { get; set; }

    /// <summary>内容部件列表。</summary>
    public List<A2APart> Parts { get; set; } = new();

    /// <summary>是否为追加分片（流式）。</summary>
    public bool Append { get; set; }

    /// <summary>是否为最后分片（流式）。</summary>
    public bool LastChunk { get; set; }

    /// <summary>附加元数据。</summary>
    public Dictionary<string, string>? Metadata { get; set; }
}