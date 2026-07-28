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
    /// 自定义 Skill 列表
    /// </summary>
    public List<CustomSkillConfig> CustomSkills { get; private set; } = new();

    /// <summary>
    /// 自定义规则列表
    /// </summary>
    public List<CustomRuleConfig> CustomRules { get; private set; } = new();

    /// <summary>
    /// 外部 MCP 服务器列表
    /// </summary>
    public List<McpServerConfig> McpServers { get; private set; } = new();

    /// <summary>
    /// 内置 Skill 禁用列表（按 Id）
    /// </summary>
    public List<string> DisabledBuiltinSkills { get; private set; } = new();

    /// <summary>
    /// 内置规则禁用列表（按 Id）
    /// </summary>
    public List<string> DisabledBuiltinRules { get; private set; } = new();

    /// <summary>
    /// 内置 MCP 客户端禁用列表（按 Name）
    /// </summary>
    public List<string> DisabledBuiltinMcpClients { get; private set; } = new();

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
                    CustomSkills = config.CustomSkills ?? new List<CustomSkillConfig>();
                    CustomRules = config.CustomRules ?? new List<CustomRuleConfig>();
                    McpServers = config.McpServers ?? new List<McpServerConfig>();
                    DisabledBuiltinSkills = config.DisabledBuiltinSkills ?? new List<string>();
                    DisabledBuiltinRules = config.DisabledBuiltinRules ?? new List<string>();
                    DisabledBuiltinMcpClients = config.DisabledBuiltinMcpClients ?? new List<string>();
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
                SelectedModel = SelectedModel,
                CustomSkills = CustomSkills,
                CustomRules = CustomRules,
                McpServers = McpServers,
                DisabledBuiltinSkills = DisabledBuiltinSkills,
                DisabledBuiltinRules = DisabledBuiltinRules,
                DisabledBuiltinMcpClients = DisabledBuiltinMcpClients
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
        CustomSkills.Clear();
        CustomRules.Clear();
        McpServers.Clear();
        DisabledBuiltinSkills.Clear();
        DisabledBuiltinRules.Clear();
        DisabledBuiltinMcpClients.Clear();
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

    // ===== 自定义 Skill =====

    /// <summary>
    /// 添加自定义 Skill
    /// </summary>
    /// <param name="skill">Skill 配置</param>
    public void AddCustomSkill(CustomSkillConfig skill)
    {
        ArgumentNullException.ThrowIfNull(skill);
        if (string.IsNullOrWhiteSpace(skill.Id))
            throw new ArgumentException("Skill Id 不能为空", nameof(skill));
        skill.Id = skill.Id.ToLowerInvariant();
        if (CustomSkills.Any(s => s.Id == skill.Id))
            throw new InvalidOperationException($"自定义 Skill '{skill.Id}' 已存在");
        CustomSkills.Add(skill);
        Save();
    }

    /// <summary>
    /// 更新自定义 Skill
    /// </summary>
    /// <param name="skill">Skill 配置</param>
    public void UpdateCustomSkill(CustomSkillConfig skill)
    {
        ArgumentNullException.ThrowIfNull(skill);
        var existing = CustomSkills.FirstOrDefault(s => s.Id == skill.Id.ToLowerInvariant());
        if (existing == null)
            throw new InvalidOperationException($"自定义 Skill '{skill.Id}' 不存在");
        existing.Name = skill.Name;
        existing.Description = skill.Description;
        existing.Category = skill.Category;
        existing.PromptTemplate = skill.PromptTemplate;
        existing.Examples = skill.Examples;
        Save();
    }

    /// <summary>
    /// 删除自定义 Skill
    /// </summary>
    /// <param name="id">Skill Id</param>
    public void RemoveCustomSkill(string id)
    {
        var removed = CustomSkills.RemoveAll(s => s.Id == id.ToLowerInvariant());
        if (removed > 0) Save();
    }

    /// <summary>
    /// 设置自定义 Skill 启用状态
    /// </summary>
    /// <param name="id">Skill Id</param>
    /// <param name="enabled">是否启用</param>
    public void SetCustomSkillEnabled(string id, bool enabled)
    {
        var skill = CustomSkills.FirstOrDefault(s => s.Id == id.ToLowerInvariant());
        if (skill == null)
            throw new InvalidOperationException($"自定义 Skill '{id}' 不存在");
        skill.Enabled = enabled;
        Save();
    }

    /// <summary>
    /// 设置内置 Skill 启用状态
    /// </summary>
    /// <param name="id">Skill Id</param>
    /// <param name="enabled">是否启用</param>
    public void SetBuiltinSkillEnabled(string id, bool enabled)
    {
        id = id.ToLowerInvariant();
        if (enabled) DisabledBuiltinSkills.Remove(id);
        else if (!DisabledBuiltinSkills.Contains(id)) DisabledBuiltinSkills.Add(id);
        Save();
    }

    // ===== 自定义规则 =====

    /// <summary>
    /// 添加自定义规则
    /// </summary>
    /// <param name="rule">规则配置</param>
    public void AddCustomRule(CustomRuleConfig rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        if (string.IsNullOrWhiteSpace(rule.Id))
            throw new ArgumentException("规则 Id 不能为空", nameof(rule));
        rule.Id = rule.Id.ToLowerInvariant();
        if (CustomRules.Any(r => r.Id == rule.Id))
            throw new InvalidOperationException($"自定义规则 '{rule.Id}' 已存在");
        CustomRules.Add(rule);
        Save();
    }

    /// <summary>
    /// 更新自定义规则
    /// </summary>
    /// <param name="rule">规则配置</param>
    public void UpdateCustomRule(CustomRuleConfig rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        var existing = CustomRules.FirstOrDefault(r => r.Id == rule.Id.ToLowerInvariant());
        if (existing == null)
            throw new InvalidOperationException($"自定义规则 '{rule.Id}' 不存在");
        existing.Name = rule.Name;
        existing.Description = rule.Description;
        existing.ActionTypePattern = rule.ActionTypePattern;
        existing.TargetPattern = rule.TargetPattern;
        existing.Action = rule.Action;
        existing.Priority = rule.Priority;
        Save();
    }

    /// <summary>
    /// 删除自定义规则
    /// </summary>
    /// <param name="id">规则 Id</param>
    public void RemoveCustomRule(string id)
    {
        var removed = CustomRules.RemoveAll(r => r.Id == id.ToLowerInvariant());
        if (removed > 0) Save();
    }

    /// <summary>
    /// 设置自定义规则启用状态
    /// </summary>
    /// <param name="id">规则 Id</param>
    /// <param name="enabled">是否启用</param>
    public void SetCustomRuleEnabled(string id, bool enabled)
    {
        var rule = CustomRules.FirstOrDefault(r => r.Id == id.ToLowerInvariant());
        if (rule == null)
            throw new InvalidOperationException($"自定义规则 '{id}' 不存在");
        rule.Enabled = enabled;
        Save();
    }

    /// <summary>
    /// 设置内置规则启用状态
    /// </summary>
    /// <param name="id">规则 Id</param>
    /// <param name="enabled">是否启用</param>
    public void SetBuiltinRuleEnabled(string id, bool enabled)
    {
        id = id.ToLowerInvariant();
        if (enabled) DisabledBuiltinRules.Remove(id);
        else if (!DisabledBuiltinRules.Contains(id)) DisabledBuiltinRules.Add(id);
        Save();
    }

    // ===== 外部 MCP 服务器 =====

    /// <summary>
    /// 添加外部 MCP 服务器
    /// </summary>
    /// <param name="server">服务器配置</param>
    public void AddMcpServer(McpServerConfig server)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (string.IsNullOrWhiteSpace(server.Name))
            throw new ArgumentException("服务器名称不能为空", nameof(server));
        server.Name = server.Name.ToLowerInvariant();
        if (McpServers.Any(s => s.Name == server.Name))
            throw new InvalidOperationException($"MCP 服务器 '{server.Name}' 已存在");
        McpServers.Add(server);
        Save();
    }

    /// <summary>
    /// 更新外部 MCP 服务器
    /// </summary>
    /// <param name="server">服务器配置</param>
    public void UpdateMcpServer(McpServerConfig server)
    {
        ArgumentNullException.ThrowIfNull(server);
        var existing = McpServers.FirstOrDefault(s => s.Name == server.Name.ToLowerInvariant());
        if (existing == null)
            throw new InvalidOperationException($"MCP 服务器 '{server.Name}' 不存在");
        existing.Description = server.Description;
        existing.Command = server.Command;
        existing.Args = server.Args;
        Save();
    }

    /// <summary>
    /// 删除外部 MCP 服务器
    /// </summary>
    /// <param name="name">服务器名称</param>
    public void RemoveMcpServer(string name)
    {
        var removed = McpServers.RemoveAll(s => s.Name == name.ToLowerInvariant());
        if (removed > 0) Save();
    }

    /// <summary>
    /// 设置外部 MCP 服务器启用状态
    /// </summary>
    /// <param name="name">服务器名称</param>
    /// <param name="enabled">是否启用</param>
    public void SetMcpServerEnabled(string name, bool enabled)
    {
        var server = McpServers.FirstOrDefault(s => s.Name == name.ToLowerInvariant());
        if (server == null)
            throw new InvalidOperationException($"MCP 服务器 '{name}' 不存在");
        server.Enabled = enabled;
        Save();
    }

    /// <summary>
    /// 设置内置 MCP 客户端启用状态
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <param name="enabled">是否启用</param>
    public void SetBuiltinMcpClientEnabled(string name, bool enabled)
    {
        name = name.ToLowerInvariant();
        if (enabled) DisabledBuiltinMcpClients.Remove(name);
        else if (!DisabledBuiltinMcpClients.Contains(name)) DisabledBuiltinMcpClients.Add(name);
        Save();
    }

    /// <summary>
    /// 添加自定义模型到指定 Provider
    /// </summary>
    public void AddCustomModel(string providerName, string modelName)
    {
        var provider = GetProvider(providerName);
        if (provider == null)
            throw new InvalidOperationException($"Provider '{providerName}' 不存在");

        modelName = modelName.Trim();
        if (string.IsNullOrWhiteSpace(modelName))
            throw new ArgumentException("模型名称不能为空", nameof(modelName));

        if (!provider.CustomModels.Contains(modelName))
        {
            provider.CustomModels.Add(modelName);
            Save();
        }
    }

    /// <summary>
    /// 更新自定义模型名称
    /// </summary>
    public void UpdateCustomModel(string providerName, string oldModelName, string newModelName)
    {
        var provider = GetProvider(providerName);
        if (provider == null)
            throw new InvalidOperationException($"Provider '{providerName}' 不存在");

        var index = provider.CustomModels.IndexOf(oldModelName);
        if (index >= 0)
        {
            provider.CustomModels[index] = newModelName.Trim();
            
            if (SelectedModel == $"{providerName}:{oldModelName}")
            {
                SelectedModel = $"{providerName}:{newModelName.Trim()}";
            }
            
            Save();
        }
    }

    /// <summary>
    /// 删除自定义模型
    /// </summary>
    public void RemoveCustomModel(string providerName, string modelName)
    {
        var provider = GetProvider(providerName);
        if (provider == null)
            throw new InvalidOperationException($"Provider '{providerName}' 不存在");

        if (provider.CustomModels.Remove(modelName))
        {
            if (SelectedModel == $"{providerName}:{modelName}")
            {
                SelectedModel = null;
            }
            Save();
        }
    }

    /// <summary>
    /// 获取 Provider 的所有模型（预定义 + 自定义）
    /// </summary>
    public List<string> GetAllModels(string providerName)
    {
        var provider = GetProvider(providerName);
        if (provider == null)
            return new List<string>();

        var models = ProviderModels.GetModels(providerName);
        var customModels = provider.CustomModels ?? new List<string>();
        
        var allModels = new List<string>(models);
        foreach (var custom in customModels)
        {
            if (!allModels.Contains(custom))
                allModels.Add(custom);
        }
        
        return allModels;
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
