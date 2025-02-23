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

        //var runner = serviceProvider.GetRequiredService<BusinessRequirementExtractor>();
        //var result =
        //    await runner.GenerateGherkin("Lavka.Api.Application.UseCases.Customer.Product.List.ListCustomerProductsUseCase", "HandleAsync",
        //        CancellationToken.None);
        
        var runner = serviceProvider.GetRequiredService<TestRunner>();
        var result = await runner.RunTest(
            @"
Gherkin test:
### Feature: List Customer Products Use Case Validation
The `ListCustomerProductsUseCase` should correctly handle product listing requests for customers, ensuring all rules and constraints are met.

#### Scenario: Successfully listing products paging through multiple pages
**Given**
  - An `IApplicationContext` with valid products where all products have a StoreId.
  - Products have non-negative inventory quantities.
  - A `CustomerListProductsQuery` with valid `Page`, `PageSize`, and `StoreId` parameters.
**When**
  - HandleAsync method is invoked with these parameters.
**Then**
  - A list of products should be returned respecting pagination and products should be sorted by `CreatedAt` descending.
  - Images URL should be built using the `ImagesLinkBuilder`.

#### Scenario: Handling invalid pagination input
**Given**
  - A customer provides a `CustomerListProductsQuery` with `Page < 1`, `PageSize < 1`, or `PageSize > 20`.
**When**
  - The `HandleAsync` method is called.
**Then**
  - A `LavkaException` is thrown with `ErrorCodes.InvalidPagination`.

#### Scenario: Handling exceptions from external data source
**Given**
  - An `IApplicationContext` throws an exception that is not a `LavkaException` during product retrieval.
**When**
  - The `HandleAsync` method is called.
**Then**
  - A `LavkaException` is thrown with `ErrorCodes.ListProductsError`, and the original exception is attached as an inner exception.

### Feature: Product Price and Quantity Constraints
Products should adhere to specified business rules concerning their prices and quantities.

#### Scenario: Editing a product to add inventory should respect existing rules
**Given**
  - A product with `InventoryQuantity == null`.
**When**
  - An attempt is made to set a non-null `InventoryQuantity` using the `Edit` method.
**Then**
  - A `LavkaException` is thrown with `ErrorCodes.ProductDoesNotHaveQuantity`.

#### Scenario: Setting a negative product price
**Given**
  - A `Price` value is initialized to `0.00` or a negative value.
**When**
  - A new price object is created with this value.
**Then**
  - A `LavkaException` is thrown with `ErrorCodes.PriceIsNegative`.

### Feature: Image URL Generation
The `ImagesLinkBuilder` should correctly build URLs.

#### Scenario: Building image links from S3 settings
**Given**
  - Valid `S3Settings` with an `ImagesRootPath`.
  - A valid product `ImageId`.
**When**
  - The `BuildImageLink` method is invoked with this `ImageId`.
**Then**
  - The returned URL accurately reflects the image path derived from the `ImagesRootPath` and `ImageId`.",
            "Lavka.Api.Application.UseCases.Customer.Product.List.ListCustomerProductsUseCase",
            "HandleAsync",
            CancellationToken.None);
        Console.WriteLine(result);
    }
}