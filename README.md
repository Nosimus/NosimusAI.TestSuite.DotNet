# NOSIMUS AI FOR .NET

This package allows users to test their code against business requirements. Nosimus AI collects call graph for the specified class and entry point (method) and tests it against given business requirements using GPT.

### How to use

1) Install Nosimus AI package and CSharp code analysis packages

```shell
dotnet add package NosimusAI.TestSuite
dotnet add package Microsoft.CodeAnalysis.CSharp
dotnet add package Microsoft.CodeAnalysis.CSharp.Workspaces
```

2) Add the following section to your configuration (either in appsettings.json or code config)

```json
{
  "Nosimus": {
    "OpenAiKey": "",
    "SolutionPath": "Full path to .sln file"
  }
}
```
3) Register Nosimus AI services

```csharp
serviceCollection.AddNosimusAI(configuration);
```

4) Use it in any test framework of your choice

```csharp
using NosimusAI;

[Test]
public async Task ShouldPassBusinessRequirement()
{
    var service = GlobalTestSetup.ServiceProvider!.GetRequiredService<TestRunner>();
    var result = await service.RunTest(
        @"Must borrow the book. 
          Must ensure that book was not borrowed more than 30 days ago.
          Must ensure that abonent did not borrow more than 3 books.",
        typeof(Book), // Class (entry point)
        "Borrow", // Method (entry point)
        CancellationToken.None);
    Assert.That(result.Passed, Is.EqualTo(true));
}
```
