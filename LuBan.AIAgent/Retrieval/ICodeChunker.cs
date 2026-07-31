/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval
*文件名： ICodeChunker
*版本号： V1.0.0.0
*唯一标识：2f1ce740-1ff0-42d6-b458-0a2511f1c7fb
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：代码切块器接口
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：代码切块器接口
*
*****************************************************************************/
namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 文本语义切块器接口
/// </summary>
public interface ICodeChunker
{
    /// <summary>
    /// 语言标识，如 csharp、html
    /// </summary>
    string Language { get; }

    /// <summary>
    /// 支持的扩展名（含点，如 .cs）
    /// </summary>
    IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 切块
    /// </summary>
    IReadOnlyList<CodeChunk> Chunk(string filePath, string content);
}
