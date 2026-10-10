/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Configuration
*文件名： GitToolOptions
*创建时间：2026/10/9
*描述：Git 工具配置选项
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Git 工具配置。
/// </summary>
public class GitToolOptions
{
    /// <summary>
    /// 是否启用 Git 工具组（默认启用）。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 默认超时时间（毫秒），与脚本工具对齐。
    /// </summary>
    public int DefaultTimeout { get; set; } = 120000;
}
