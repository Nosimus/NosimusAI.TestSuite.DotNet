using Microsoft.Build.Locator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NosimusAI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNosimusAI(this IServiceCollection services, IConfiguration configuration)
    {
        MSBuildLocator.RegisterDefaults();
        
        services.AddOptions<NosimusSettings>()
            .Bind(configuration.GetSection(NosimusSettings.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<CallgraphExtractor>();
        services.AddSingleton<AITestRunner>();
        services.AddSingleton<BusinessRequirementExtractor>();
        services.AddSingleton<TestRunner>();

        return services;
    }
}