using Microsoft.Extensions.DependencyInjection;

namespace LuBan.AIAgent.Extensions.FileSystem.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于添加文件系统工具到依赖注入容器
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加AgileAI文件系统工具到服务集合
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAgileAIFileSystemTools(this IServiceCollection services, Action<FileSystemToolOptions> configure)
        => services.AddFileSystemTools(configure);

    /// <summary>
    /// 添加AgileAI文件系统工具到服务集合
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="options">文件系统工具选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAgileAIFileSystemTools(this IServiceCollection services, FileSystemToolOptions options)
        => services.AddFileSystemTools(options);

    /// <summary>
    /// 添加文件系统工具到服务集合
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddFileSystemTools(this IServiceCollection services, Action<FileSystemToolOptions> configure)
    {
        var options = new FileSystemToolOptions();
        configure(options);
        return services.AddFileSystemTools(options);
    }

    /// <summary>
    /// 添加文件系统工具到服务集合
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="options">文件系统工具选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddFileSystemTools(this IServiceCollection services, FileSystemToolOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<FileSystemPathGuard>();
        services.AddScoped<ListDirectoryTool>();
        services.AddScoped<SearchFilesTool>();
        services.AddScoped<ReadFileTool>();
        services.AddScoped<ReadFilesBatchTool>();
        services.AddScoped<WriteFileTool>();
        services.AddScoped<CreateDirectoryTool>();
        services.AddScoped<MoveFileTool>();
        services.AddScoped<PatchFileTool>();
        services.AddScoped<DeleteFileTool>();
        services.AddScoped<DeleteDirectoryTool>();
        services.AddScoped<FileSystemToolRegistryFactory>();
        return services;
    }
}
