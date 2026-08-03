// LuBan.TrimTest - Trimmer analysis verification project
// This project is used to validate that the LuBan.FrameWork core libraries
// produce zero IL2xxx/IL3xxx trimmer warnings when published with PublishTrimmed=true.

using LuBan.Common;
using LuBan.DI;
using LuBan.AIAgent;
using LuBan.AIAgent.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// L1: 纯反射读取（trim-safe，无需 root）
var type = typeof(SampleEntity);
var props = type.GetProperties();
var attr = type.GetCustomAttribute<ObsoleteAttribute>();

// L2: 动态发现（触发 [RUC] 警告，需在调用点抑制或 root）
#pragma warning disable IL2026 // 已知 [RUC] 调用，测试目的
var services = new ServiceCollection();
services.AddLuBanAgent(new ConfigurationBuilder().Build());
services.AutoInjectAllCustomerServices();
#pragma warning restore IL2026

// L3: Emit 代理（触发 [RUC] 警告）
#pragma warning disable IL2026
var provider = services.BuildServiceProvider();
var registry = provider.GetService<ToolPluginRegistry>();
#pragma warning restore IL2026

Console.WriteLine(registry != null ? "OK" : "FAIL");

class SampleEntity
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}