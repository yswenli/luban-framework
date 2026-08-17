namespace LuBan.Common.Errors;

using Microsoft.Extensions.DependencyInjection;

public static class ErrorCodeServiceCollectionExtensions
{
    public static IServiceCollection AddErrorCodes(this IServiceCollection services, IEnumerable<ErrorDescriptor> descriptors)
    {
        services.AddSingleton<ErrorCodeRegistry>(sp =>
        {
            var registry = new ErrorCodeRegistry();
            registry.Register(descriptors);
            return registry;
        });
        return services;
    }

    public static IServiceCollection AddErrorCodes(this IServiceCollection services)
    {
        services.AddSingleton<ErrorCodeRegistry>();
        return services;
    }
}