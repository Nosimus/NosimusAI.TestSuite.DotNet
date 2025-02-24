using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NosimusAI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        serviceCollection.AddNosimusAI(configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var runner = serviceProvider.GetRequiredService<BusinessRequirementExtractor>();
        //var result =
        //    await runner.GenerateGherkinForAspNetControllers()
        
        //Console.WriteLine(result);
    }
}