using Microsoft.Extensions.DependencyInjection;

namespace MaxRunSoftware.Utilities.Common;

public static class LoggerForwarderProviderExtensions
{
    public static IServiceCollection AddLoggerForwarderProvider(this IServiceCollection services)
    {
        var descriptor = new ServiceDescriptor(typeof(ILoggerProvider), typeof(LoggerForwarderProvider), ServiceLifetime.Singleton);
        services.Add(descriptor);
        return services;
    }
}
