/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval.Chunkers
*文件名： SlidingWindowChunker
*版本号： V1.0.0.0
*唯一标识：ca1e9fbd-c767-42c5-9216-b916a8cad9fa
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：滑动窗口切块器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：滑动窗口切块器
*
*****************************************************************************/
namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 滑动窗口切块器（兜底策略）
/// </summary>
public class SlidingWindowChunker : CodeChunkerBase
{
    private readonly string _language;

    /// <summary>
    /// 创建滑窗切块器
    /// </summary>
    public SlidingWindowChunker(string language = "text") => _language = language;

    /// <inheritdoc />
    public override string Language => _language;

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions => Array.Empty<string>();

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
        => AssignIndices(WindowAll(filePath, content.Replace("\r\n", "\n")));
}
