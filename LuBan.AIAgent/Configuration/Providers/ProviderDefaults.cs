/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Company：Walle
*命名空间：LuBan.AIAgent.Configuration.Providers
*文件名： ProviderDefaults
*版本号： V1.0.0.0
*唯一标识：3ae1ada5-f557-4002-bfb7-a821ff972a51
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：内置 Provider 的默认 BaseUrl 与模型清单（纯只读静态数据，无配置绑定与刷新状态）
*
*****************************************************************************/

namespace LuBan.AIAgent.Configuration.Providers;

/// <summary>内置 Provider 的默认端点与模型清单（纯只读数据）。</summary>
public static class ProviderDefaults
{
    private static readonly IReadOnlyDictionary<string, string> DefaultBaseUrls =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["openai"] = "https://api.openai.com/v1/",
            ["deepseek"] = "https://api.deepseek.com/v1/",
            ["kimi"] = "https://api.moonshot.cn/v1/",
            ["glm"] = "https://open.bigmodel.cn/api/paas/v4/",
            ["qwen"] = "https://dashscope.aliyuncs.com/compatible-mode/v1/",
            ["doubao"] = "https://ark.cn-beijing.volces.com/api/v3/",
            ["ollama"] = "http://localhost:11434/v1/",
            ["ernie"] = "https://qianfan.baidubce.com/v2/",
            ["minimax"] = "https://api.minimax.chat/v1/",
            ["hunyuan"] = "https://api.hunyuan.cloud.tencent.com/v1/",
            ["mimo"] = "https://api.xiaomi.com/v1/",
            ["xai"] = "https://api.x.ai/v1/",
            ["qianfan"] = "https://qianfan.baidubce.com/v2/",
            ["tencent-ti"] = "https://api.lkeap.cloud.tencent.com/v1/",
            ["huawei-pangu"] = "https://pangu.huaweicloud.com/v1/",
            ["bedrock"] = "https://bedrock-runtime.us-east-1.amazonaws.com/",
            ["openrouter"] = "https://openrouter.ai/api/v1/"
        });

    private static readonly IReadOnlyDictionary<string, string[]> DefaultModels =
        new ReadOnlyDictionary<string, string[]>(new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["openai"] = ["gpt-4.1", "gpt-4.1-mini", "gpt-4.1-nano", "gpt-4o", "gpt-4o-mini", "o1", "o1-mini", "o1-pro", "o3", "o3-mini", "o3-pro", "o4-mini"],
            ["azure"] = ["gpt-4o", "gpt-4o-mini", "gpt-4-turbo", "gpt-4", "gpt-35-turbo"],
            ["deepseek"] = ["deepseek-flash", "deepseek-v4-pro"],
            ["kimi"] = ["kimi-k3", "kimi-k2.7-code", "kimi-k2.7-code-highspeed", "kimi-k2.6"],
            ["glm"] =
            [
                "glm-5.3", "glm-5.3-flash", "glm-5.2", "glm-5.1", "glm-5", "glm-5-turbo",
                "glm-4.7", "glm-4.7-flash", "glm-4.6", "glm-4.5-air", "glm-4.5-flash",
                "glm-4-long", "glm-4-flash-250414",
                "glm-5v-turbo", "glm-4.6v", "glm-4.6v-flash",
                "glm-4.1v-thinking-flash", "glm-4.1v-thinking-flashx", "glm-4v-flash", "glm-ocr"
            ],
            ["qwen"] =
            [
                "qwen3.8-max", "qwen3.8-flash", "qwen3.7-plus", "qwen3.7-flash",
                "qwen3.6-plus", "qwen3.6-flash", "qwen3.5-plus", "qwen3.5-flash",
                "qwen3.5-omni-plus", "qwen3-vl-plus", "qwen3-vl-flash",
                "qwen-vl-max", "qwen-vl-plus", "qvq-max", "qvq-plus"
            ],
            ["doubao"] =
            [
                "doubao-1.5-vision-pro", "doubao-1.5-vision-lite",
                "doubao-vision-pro", "doubao-vision-lite",
                "Seed-Evolving", "deepseek-v4.1-flash"
            ],
            ["claude"] =
            [
                "claude-opus-4-20250514", "claude-sonnet-4-20250514",
                "claude-3-7-sonnet-20250219", "claude-3-5-sonnet-20241022",
                "claude-3-5-haiku-20241022", "claude-3-haiku-20240307"
            ],
            ["gemini"] =
            [
                "gemini-2.5-pro", "gemini-2.5-flash", "gemini-2.0-flash",
                "gemini-2.0-flash-lite", "gemini-1.5-pro", "gemini-1.5-flash"
            ],
            ["ollama"] = ["llama3.1", "llama3.2", "qwen2.5", "deepseek-coder-v2", "codellama"],
            ["ernie"] =
            [
                "ernie-4.5", "ernie-4.0-turbo", "ernie-4.0-vision",
                "ernie-3.5-8k", "ernie-3.5-vision", "ernie-speed-pro", "ernie-lite-pro"
            ],
            ["minimax"] =
            [
                "MiniMax-M3", "MiniMax-M2.7", "MiniMax-M2.7-highspeed",
                "MiniMax-M2.5", "MiniMax-M2.5-highspeed", "MiniMax-H3"
            ],
            ["hunyuan"] =
            [
                "hunyuan-turbos-vision", "hunyuan-t1-vision",
                "hunyuan-turbos-vision-video", "tencent-hy-vision-1.5-instruct",
                "hunyuan-turbo", "hunyuan-pro", "hunyuan-standard", "hunyuan-lite"
            ],
            ["mimo"] = ["mimo-v2.5-pro"],
            ["xai"] = ["grok-4", "grok-3", "grok-3-mini", "grok-3-fast", "grok-2-vision"],
            ["qianfan"] =
            [
                "ernie-4.5", "ernie-4.0-turbo", "ernie-4.0-vision",
                "ernie-3.5-8k", "ernie-3.5-vision", "ernie-speed-pro", "ernie-lite-pro"
            ],
            ["tencent-ti"] =
            [
                "hunyuan-turbos-vision", "hunyuan-t1-vision",
                "hunyuan-turbo", "hunyuan-pro", "hunyuan-standard", "hunyuan-lite"
            ],
            ["huawei-pangu"] = ["pangu-7b", "pangu-13b", "pangu-52b"],
            ["bedrock"] =
            [
                "anthropic.claude-opus-4-20250515-v1:0", "anthropic.claude-sonnet-4-20250515-v1:0",
                "anthropic.claude-3-5-sonnet-20241022-v1:0", "anthropic.claude-3-7-sonnet-20250219-v1:0",
                "meta.llama3-8b-instruct-v1:0", "meta.llama4-maverick-v1:0"
            ],
            ["openrouter"] =
            [
                "deepseek/deepseek-v4.1-flash", "deepseek/deepseek-pro-latest",
                "openai/gpt-4.1", "openai/o3",
                "anthropic/claude-opus-4", "anthropic/claude-sonnet-4",
                "google/gemini-2.5-pro", "google/gemini-2.5-flash",
                "meta/llama-4-maverick"
            ]
        });

    /// <summary>全部内置 Provider 标识（按模型表声明顺序）。</summary>
    public static IReadOnlyList<string> ProviderIds { get; } = [.. DefaultModels.Keys];

    /// <summary>取默认 BaseUrl；未知 Provider 返回 null。</summary>
    /// <param name="providerId">Provider 标识（大小写不敏感）。</param>
    public static string? GetDefaultBaseUrl(string providerId)
        => string.IsNullOrWhiteSpace(providerId)
            ? null
            : DefaultBaseUrls.TryGetValue(providerId, out var url) ? url : null;

    /// <summary>取默认模型清单；未知 Provider 返回空集合。</summary>
    /// <param name="providerId">Provider 标识（大小写不敏感）。</param>
    public static IReadOnlyList<string> GetDefaultModels(string providerId)
        => string.IsNullOrWhiteSpace(providerId)
            ? Array.Empty<string>()
            : DefaultModels.TryGetValue(providerId, out var models) ? models : Array.Empty<string>();
}