namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 配置管理器，负责配置的加载、保存和管理
/// </summary>
public class ConfigManager
{
    private readonly string _configPath;

    /// <summary>
    /// Provider 配置列表
    /// </summary>
    public List<ProviderConfig> Providers { get; private set; } = new();

    /// <summary>
    /// 当前选择的模型
    /// </summary>
    public string? SelectedModel { get; private set; }

    /// <summary>
    /// 是否已选择模型
    /// </summary>
    public bool HasSelectedModel => !string.IsNullOrEmpty(SelectedModel);

    /// <summary>
    /// 创建配置管理器实例
    /// </summary>
    /// <param name="configPath">配置文件路径</param>
    public ConfigManager(string configPath)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
    }

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    public void Load()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json);
                if (config != null)
                {
                    Providers = config.Providers ?? new List<ProviderConfig>();
                    SelectedModel = config.SelectedModel;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ConfigManager.Load 异常: {ex}");
            Providers = new List<ProviderConfig>();
        }
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public void Save()
    {
        try
        {
            var config = new AppConfig
            {
                Providers = Providers,
                SelectedModel = SelectedModel
            };

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"保存配置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 添加或更新 Provider
    /// </summary>
    /// <param name="name">Provider 名称</param>
    /// <param name="apiKey">API 密钥</param>
    /// <param name="baseUrl">API 基础 URL（可选）</param>
    public void AddProvider(string name, string apiKey, string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider 名称不能为空", nameof(name));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API Key 不能为空", nameof(apiKey));

        name = name.ToLowerInvariant();

        var existing = Providers.FirstOrDefault(p => p.Name == name);
        if (existing != null)
        {
            existing.ApiKey = apiKey;
            existing.BaseUrl = baseUrl;
        }
        else
        {
            Providers.Add(new ProviderConfig
            {
                Name = name,
                ApiKey = apiKey,
                BaseUrl = baseUrl
            });
        }

        Save();
    }

    /// <summary>
    /// 设置当前选择的模型
    /// </summary>
    /// <param name="model">模型标识（格式: provider:model）</param>
    public void SetSelectedModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("模型不能为空", nameof(model));

        SelectedModel = model;
        Save();
    }

    /// <summary>
    /// 清除所有配置
    /// </summary>
    public void Clear()
    {
        Providers.Clear();
        SelectedModel = null;
        Save();
    }

    /// <summary>
    /// 检查 Provider 是否存在
    /// </summary>
    /// <param name="name">Provider 名称</param>
    /// <returns>是否存在</returns>
    public bool HasProvider(string name)
    {
        return Providers.Any(p => p.Name == name.ToLowerInvariant());
    }

    /// <summary>
    /// 获取指定名称的 Provider 配置
    /// </summary>
    /// <param name="name">Provider 名称</param>
    /// <returns>Provider 配置，不存在返回 null</returns>
    public ProviderConfig? GetProvider(string name)
    {
        return Providers.FirstOrDefault(p => p.Name == name.ToLowerInvariant());
    }

    /// <summary>
    /// 创建 ChatClient 实例
    /// </summary>
    /// <returns>IChatClient 实例</returns>
    public IChatClient CreateChatClient()
    {
        if (string.IsNullOrEmpty(SelectedModel))
            throw new InvalidOperationException("请先选择模型");

        var parts = SelectedModel.Split(':', 2);
        if (parts.Length != 2)
            throw new InvalidOperationException($"模型格式错误: {SelectedModel}");

        var providerName = parts[0].ToLowerInvariant();
        var modelName = parts[1];

        var provider = GetProvider(providerName);
        if (provider == null)
            throw new InvalidOperationException($"Provider '{providerName}' 不存在");

        return CreateOpenAIClient(modelName, provider.ApiKey, provider.BaseUrl);
    }

    /// <summary>
    /// 创建 OpenAI 兼容的 ChatClient
    /// </summary>
    private static IChatClient CreateOpenAIClient(string modelName, string apiKey, string? baseUrl)
    {
        var clientOptions = new OpenAI.OpenAIClientOptions();

        if (!string.IsNullOrEmpty(baseUrl))
        {
            clientOptions.Endpoint = new Uri(baseUrl);
        }

        var credential = new System.ClientModel.ApiKeyCredential(apiKey);
        var openAIClient = new OpenAI.OpenAIClient(credential, clientOptions);
        return openAIClient.GetChatClient(modelName).AsIChatClient();
    }

    /// <summary>
    /// 获取默认配置文件路径
    /// </summary>
    /// <returns>配置文件路径</returns>
    public static string GetDefaultConfigPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, "LuBan", "AIAgent", "config.json");
    }
}
