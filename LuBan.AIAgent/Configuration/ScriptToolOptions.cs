/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： ScriptToolOptions
*版本号： V1.0.0.0
*唯一标识：2245393e-582e-4ebd-8a1c-a06c6f21fcf1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：脚本工具配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：脚本工具配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 脚本工具配置
/// </summary>
public class ScriptToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Shell 程序。留空表示自动探测：Windows 优先 pwsh &gt; powershell &gt; cmd，
    /// 类 Unix 优先 bash &gt; sh。也可显式指定可执行文件名（如 "cmd"）或绝对路径强制使用；
    /// 指定的 shell 找不到时会回退到自动探测。
    /// </summary>
    public string Shell { get; set; } = string.Empty;

    /// <summary>
    /// Python 解释器路径
    /// </summary>
    public string PythonPath { get; set; } = "python";

    /// <summary>
    /// 默认超时时间（毫秒）
    /// </summary>
    public int DefaultTimeout { get; set; } = 120000;
}