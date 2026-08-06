namespace LuBan.AIAgent.Configuration;

public interface IAppConfigReader
{
    List<ProviderConfigData> Providers { get; }
    string? SelectedModel { get; }
    List<CustomSkillConfig> CustomSkills { get; }
    List<CustomRuleConfig> CustomRules { get; }
    List<McpServerConfig> McpServers { get; }
    List<string> DisabledBuiltinSkills { get; }
    List<string> DisabledBuiltinRules { get; }
    List<string> DisabledBuiltinMcpClients { get; }
}

public class ProviderConfigData
{
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string? Endpoint { get; set; }
    public List<string> Models { get; set; } = new();
}
