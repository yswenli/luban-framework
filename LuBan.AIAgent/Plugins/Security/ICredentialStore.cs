/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： ICredentialStore
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/16
*描述：凭据存储契约（PRD §3.7 / §4.9）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 凭据存储：宿主提供的凭据读写口，插件只能经 <c>credential</c> 声明后间接取用（PRD §3.7）。
/// </summary>
/// <remarks>
/// <para>
/// <b>绝不入 DB 明文</b>（PRD §3.7）。具体落盘方式由宿主决定：桌面宿主沿用其现有配置存储基线
/// （与 LLM ApiKey 同基线，PRD §4.9.3 放宽裁决）；框架仅内置一个「配置节点 + 环境变量兜底」的默认实现
/// <see cref="ConfigurationCredentialStore"/>，供最小宿主与测试使用。
/// </para>
/// <para>
/// 键名按 <see cref="CredentialKeys.Normalize"/> 归一化（如 <c>codeup:pat</c> 与 <c>yunxiao_pat</c> 等价）。
/// </para>
/// </remarks>
public interface ICredentialStore
{
    /// <summary>
    /// 读取凭据。
    /// </summary>
    /// <param name="key">凭据键（会自动归一化）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>凭据值；未配置时返回 <c>null</c>。</returns>
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入或删除凭据。
    /// </summary>
    /// <param name="key">凭据键（会自动归一化）。</param>
    /// <param name="value">凭据值；为 <c>null</c>/空时删除该键。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task SetAsync(string key, string? value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 判断凭据是否存在且非空。
    /// </summary>
    /// <param name="key">凭据键。</param>
    /// <returns>存在返回 <c>true</c>。</returns>
    bool Contains(string key);

    /// <summary>
    /// 已登记的凭据键（归一化后），供设置页展示。
    /// </summary>
    IReadOnlyCollection<string> Keys { get; }
}