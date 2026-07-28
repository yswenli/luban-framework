using System;
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.ConsoleApp.Retrieval;
using LuBan.AIAgent.Retrieval;
using LuBan.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// RAG 命令 - 语义检索管理
/// </summary>
public class RagCommand : CommandBase
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "rag";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "语义检索（索引/搜索/模型管理）";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public RagCommand(ConfigManager configManager, IConfiguration configuration, IServiceProvider serviceProvider)
        : base(configManager, configuration)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override async Task ExecuteAsync()
    {
        Console.WriteLine();
        Console.WriteLine("语义检索管理：");
        Console.WriteLine();
        Console.WriteLine("操作:");
        Console.WriteLine("  1. index    - 索引目录");
        Console.WriteLine("  2. search   - 语义搜索");
        Console.WriteLine("  3. stats    - 索引统计");
        Console.WriteLine("  4. model    - 模型状态");
        Console.WriteLine();
        Console.Write("请输入操作: ");
        var input = Console.ReadLine()?.Trim().ToLower();
        if (string.IsNullOrEmpty(input)) return;
        await ExecuteSubCommand(input);
    }

    /// <summary>
    /// 执行带子命令的命令
    /// </summary>
    public override async Task<bool> ExecuteAsync(string[] args)
    {
        if (args.Length == 0) return false;
        var sub = args[0].ToLower();
        var rest = args.Length > 1 ? args[1..] : Array.Empty<string>();
        await ExecuteSubCommand(sub, rest);
        return true;
    }

    private async Task ExecuteSubCommand(string input, string[]? extraArgs = null)
    {
        var service = _serviceProvider.GetService<IRetrievalService>();
        switch (input)
        {
            case "1":
            case "index":
                if (service == null) { Console.WriteLine("检索功能未启用（模型未就绪或配置 disabled）"); return; }
                var path = extraArgs?.Length > 0 ? extraArgs[0] : Prompt("请输入目录路径: ");
                var glob = extraArgs?.Length > 1 ? string.Join(' ', extraArgs[1..]) : null;
                if (string.IsNullOrWhiteSpace(glob)) glob = null;
                if (!Directory.Exists(path)) { Console.WriteLine($"目录不存在: {path}"); return; }
                var report = await ConsoleUtil.RunWithStatusAsync<IndexReport>(
                    async (update, ct) => await service.IndexDirectoryAsync(path, glob, false, ct), "索引中…");
                if (report == null) { Console.WriteLine("索引失败"); return; }
                Console.WriteLine($"索引完成：扫描 {report.ScannedFiles}，新增 {report.NewFiles}，更新 {report.UpdatedFiles}，跳过 {report.SkippedFiles}，删除 {report.DeletedFiles}");
                Console.WriteLine($"切块 {report.TotalChunks}（新嵌入 {report.EmbeddedChunks}，复用 {report.ReusedChunks}）");
                break;

            case "2":
            case "search":
                if (service == null) { Console.WriteLine("检索功能未启用"); return; }
                var query = extraArgs?.Length > 0 ? string.Join(' ', extraArgs) : Prompt("请输入搜索内容: ");
                if (string.IsNullOrWhiteSpace(query)) { Console.WriteLine("搜索内容不能为空"); return; }
                var results = await service.SearchAsync(query);
                if (results.Count == 0) { Console.WriteLine("未找到相关内容"); return; }
                foreach (var r in results)
                {
                    var symbol = r.SymbolName != null ? $" {r.SymbolName}" : "";
                    Console.WriteLine($"--- {r.FilePath}:{r.StartLine}-{r.EndLine} [{r.ChunkType}]{symbol} (相关度 {r.Score:F2}) ---");
                    Console.WriteLine(r.Content.Length > 500 ? r.Content[..500] + "\n…" : r.Content);
                    Console.WriteLine();
                }
                break;

            case "3":
            case "stats":
                if (service == null) { Console.WriteLine("检索功能未启用"); return; }
                var stats = await service.GetStatsAsync();
                Console.WriteLine($"已索引文件 {stats.TotalFiles} 个，切块 {stats.TotalChunks} 个");
                Console.WriteLine($"模型 {stats.ModelId ?? "未知"}，向量维度 {stats.VectorDimension}");
                break;

            case "4":
            case "model":
                await HandleModelCommand(extraArgs);
                break;

            default:
                Console.WriteLine($"未知操作: {input}");
                break;
        }
    }

    private Task HandleModelCommand(string[]? extraArgs)
    {
        var mm = _serviceProvider.GetService<ModelManager>();
        var config = Configuration.GetSection("LuBanAgent:Tools:Retrieval").Get<RetrievalToolOptions>() ?? new RetrievalToolOptions();
        var spec = EmbeddingModelCatalog.Find(config.ModelId);
        if (spec == null) { Console.WriteLine($"未知模型: {config.ModelId}"); return Task.CompletedTask; }
        if (mm == null) mm = new ModelManager(spec);

        Console.WriteLine($"模型: {spec.ModelId}，维度: {spec.Dimension}");
        Console.WriteLine($"目录: {mm.ModelDirectory}");
        Console.WriteLine(mm.IsModelReady() ? "状态: 就绪" : "状态: 未就绪");
        if (!mm.IsModelReady())
        {
            Console.WriteLine($"本地包: {mm.LocalZipPath}");
            Console.WriteLine(File.Exists(mm.LocalZipPath) ? "本地包: 存在" : "本地包: 不存在");
        }
        return Task.CompletedTask;
    }

    private static string Prompt(string message)
    {
        Console.Write(message);
        return Console.ReadLine()?.Trim() ?? "";
    }
}
