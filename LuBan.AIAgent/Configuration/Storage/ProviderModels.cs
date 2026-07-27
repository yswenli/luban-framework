namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 预定义的 Provider 模型配置
/// </summary>
public static class ProviderModels
{
    /// <summary>
    /// 获取 Provider 支持的模型列表
    /// </summary>
    /// <param name="providerName">Provider 名称</param>
    /// <returns>模型列表</returns>
    public static List<string> GetModels(string providerName)
    {
        return _models.TryGetValue(providerName.ToLowerInvariant(), out var models)
            ? models
            : new List<string>();
    }

    /// <summary>
    /// 获取 Provider 显示名称
    /// </summary>
    /// <param name="providerName">Provider 名称</param>
    /// <returns>显示名称</returns>
    public static string GetDisplayName(string providerName)
    {
        return _displayNames.TryGetValue(providerName.ToLowerInvariant(), out var name)
            ? name
            : providerName;
    }

    private static readonly Dictionary<string, List<string>> _models = new()
    {
        ["openai"] = new List<string>
        {
            "gpt-4.1",
            "gpt-4.1-mini",
            "gpt-4.1-nano",
            "gpt-4o",
            "gpt-4o-mini",
            "gpt-4-turbo",
            "gpt-4",
            "gpt-3.5-turbo",
            "o1",
            "o1-mini",
            "o3-mini"
        },
        ["azure"] = new List<string>
        {
            "gpt-4o",
            "gpt-4o-mini",
            "gpt-4-turbo",
            "gpt-4",
            "gpt-35-turbo"
        },
        ["deepseek"] = new List<string>
        {
            "deepseek-chat",
            "deepseek-coder",
            "deepseek-reasoner"
        },
        ["kimi"] = new List<string>
        {
            "k3",
            "k3-256k",
            "kimi-for-coding",
            "kimi-for-coding-highspeed"
        },
        ["glm"] = new List<string>
        {
            "glm-4-plus",
            "glm-4-0520",
            "glm-4-air",
            "glm-4-airx",
            "glm-4-flash",
            "glm-3-turbo"
        },
        ["qwen"] = new List<string>
        {
            "qwen-turbo",
            "qwen-plus",
            "qwen-max",
            "qwen-max-longcontext"
        },
        ["doubao"] = new List<string>
        {
            "doubao-pro-4k",
            "doubao-pro-32k",
            "doubao-pro-128k",
            "doubao-lite-4k"
        },
        ["claude"] = new List<string>
        {
            "claude-3-5-sonnet-20241022",
            "claude-3-5-haiku-20241022",
            "claude-3-opus-20240229",
            "claude-3-sonnet-20240229",
            "claude-3-haiku-20240307"
        },
        ["gemini"] = new List<string>
        {
            "gemini-2.0-flash",
            "gemini-1.5-pro",
            "gemini-1.5-flash",
            "gemini-1.5-flash-8b"
        },
        ["ollama"] = new List<string>
        {
            "llama3.1",
            "llama3.2",
            "qwen2.5",
            "deepseek-coder-v2",
            "codellama"
        }
    };

    private static readonly Dictionary<string, string> _displayNames = new()
    {
        ["openai"] = "OpenAI",
        ["azure"] = "Azure OpenAI",
        ["deepseek"] = "DeepSeek",
        ["kimi"] = "Kimi",
        ["glm"] = "智谱 GLM",
        ["qwen"] = "通义千问",
        ["doubao"] = "豆包",
        ["claude"] = "Claude",
        ["gemini"] = "Google Gemini",
        ["ollama"] = "Ollama (本地)"
    };
}