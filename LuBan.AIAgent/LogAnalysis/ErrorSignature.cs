namespace LuBan.AIAgent.LogAnalysis;

/// <summary>归一化后的错误签名及其出现统计。</summary>
/// <param name="Template">归一化模板。</param>
/// <param name="Count">出现次数。</param>
/// <param name="SampleLineNumbers">样本行号（≤ SampleLinesPerSignature）。</param>
public sealed record ErrorSignature(
    string Template,
    long Count,
    IReadOnlyList<long> SampleLineNumbers);
