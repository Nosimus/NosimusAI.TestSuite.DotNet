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

        var runner = serviceProvider.GetRequiredService<TestRunner>();
        
        var result = await runner.RunTest(
            "Must borrow the book. Must ensure that abonent did not book more than 3 books.",
            "MyBooks.Book",
            "ExecuteAsync",
            CancellationToken.None);
        Console.WriteLine(result);
    }
}