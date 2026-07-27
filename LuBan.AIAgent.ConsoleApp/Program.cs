using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.ConsoleApp.Infrastructure;
using LuBan.AIAgent.ConsoleApp.Retrieval;
using LuBan.AIAgent.ConsoleApp.Services;
using LuBan.AIAgent.Retrieval;
using LuBan.AIAgent.Sessions;
using LuBan.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.ConsoleApp;

/// <summary>
/// 程序入口
/// </summary>
class Program
{
    /// <summary>
    /// 程序主入口
    /// </summary>
    static async Task Main(string[] args)
    {
        ConsoleUtil.PrintName();

        DatabaseInitializer.Initialize();

        var configuration = BuildConfiguration(args);
        var (embedder, modelManager) = await PrepareRetrievalAsync(configuration);
        using var serviceProvider = BuildServiceProvider(configuration, embedder, modelManager);

        var appService = serviceProvider.GetRequiredService<ConsoleAppService>();
        await appService.RunAsync();
    }

    private static IConfiguration BuildConfiguration(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
        return builder.Build();
    }

    private static async Task<(OnnxEmbeddingGenerator? embedder, ModelManager? modelManager)> PrepareRetrievalAsync(IConfiguration configuration)
    {
        var retrieval = configuration.GetSection("LuBanAgent:Tools:Retrieval").Get<RetrievalToolOptions>() ?? new RetrievalToolOptions();
        if (!retrieval.Enabled) return (null, null);
        var spec = EmbeddingModelCatalog.Find(retrieval.ModelId);
        if (spec == null)
        {
            Console.WriteLine($"未知的嵌入模型：{retrieval.ModelId}，检索功能已禁用");
            return (null, null);
        }
        var mm = new ModelManager(spec);
        if (mm.IsModelReady()) return (new OnnxEmbeddingGenerator(mm.ModelDirectory, spec), mm);
        if (!retrieval.AutoDownload)
        {
            Console.WriteLine($"嵌入模型 {spec.ModelId} 未就绪（运行 rag model download 手动下载），检索功能已禁用");
            return (null, null);
        }
        var ok = await ConsoleUtil.RunWithStatusAsync<bool>(
            async (update, ct) => await mm.EnsureModelAsync(update, ct),
            "检查嵌入模型…");
        if (!ok || !mm.IsModelReady())
        {
            Console.WriteLine();
            Console.WriteLine($"嵌入模型 {spec.ModelId} 下载失败，检索功能已禁用（不影响其他功能）");
            Console.WriteLine();
            Console.WriteLine("如需使用检索功能，请手动下载以下文件并放到指定目录：");
            Console.WriteLine($"目标目录: {mm.ModelDirectory}");
            Console.WriteLine();
            foreach (var file in spec.Files)
            {
                var url = spec.MirrorBase + file.RemotePath;
                Console.WriteLine($"  - {file.LocalName}");
                Console.WriteLine($"    下载地址: {url}");
            }
            Console.WriteLine();
            return (null, null);
        }
        return (new OnnxEmbeddingGenerator(mm.ModelDirectory, spec), mm);
    }

    private static ServiceProvider BuildServiceProvider(IConfiguration configuration, OnnxEmbeddingGenerator? embedder, ModelManager? modelManager)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        var configPath = ConfigManager.GetDefaultConfigPath();
        var configManager = new ConfigManager(configPath);
        configManager.Load();
        services.AddSingleton(configManager);

        services.AddLuBanAgent(configuration);

        services.AddSingleton<ISessionManager, SessionManager>();

        if (embedder != null)
        {
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(embedder);
            services.AddSingleton<IVectorStore, SqliteVectorStore>();
            services.AddSingleton<IRetrievalService>(sp => new RetrievalService(
                sp.GetRequiredService<IVectorStore>(),
                sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(),
                sp.GetRequiredService<IOptions<LuBanAgentOptions>>()));
            if (modelManager != null) services.AddSingleton(modelManager);
        }

        services.AddSingleton<ConsoleAppService>();
        return services.BuildServiceProvider();
    }
}
