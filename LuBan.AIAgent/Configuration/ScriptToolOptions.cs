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
    /// Shell 程序
    /// </summary>
    public string Shell { get; set; } = "pwsh";

    /// <summary>
    /// Lua 解释器路径
    /// </summary>
    public string LuaPath { get; set; } = "lua";

    /// <summary>
    /// Python 解释器路径
    /// </summary>
    public string PythonPath { get; set; } = "python";

    /// <summary>
    /// 默认超时时间（毫秒）
    /// </summary>
    public int DefaultTimeout { get; set; } = 120000;
}