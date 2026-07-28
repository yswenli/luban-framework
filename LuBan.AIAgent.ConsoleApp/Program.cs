using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.ConsoleApp.Infrastructure;
using LuBan.AIAgent.ConsoleApp.Retrieval;
using LuBan.AIAgent.ConsoleApp.Services;
using LuBan.AIAgent.ConsoleApp.UI;
using LuBan.AIAgent.Retrieval;
using LuBan.AIAgent.Sessions;
using LuBan.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Terminal.Gui;

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
        DatabaseInitializer.Initialize();

        var configuration = BuildConfiguration(args);
        var (embedder, modelManager) = await PrepareRetrievalAsync(configuration);

        using var serviceProvider = BuildServiceProvider(configuration, embedder, modelManager);

        Application.Init();
        
        try
        {
            var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();
            var configManager = serviceProvider.GetRequiredService<ConfigManager>();
            var consoleAppService = serviceProvider.GetRequiredService<ConsoleAppService>();
            
            var mainView = new MainView(sessionManager, configManager, serviceProvider, consoleAppService);
            
            Application.Run(mainView);
        }
        finally
        {
            Application.Shutdown();
        }
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
        
        Console.WriteLine("准备嵌入模型...");
        var ok = await mm.EnsureModelAsync(
            (progress) => Console.Write($"\r进度: {progress:P0}"),
            CancellationToken.None);
        
        if (!ok || !mm.IsModelReady())
        {
            Console.WriteLine();
            Console.WriteLine($"嵌入模型 {spec.ModelId} 未就绪，检索功能已禁用");
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

        services.AddScoped<IChatClient>(sp =>
        {
            var cm = sp.GetRequiredService<ConfigManager>();
            return cm.CreateChatClient();
        });

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
