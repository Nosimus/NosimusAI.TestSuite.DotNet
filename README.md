# NOSIMUS AI FOR .NET

This package allows users to 
- test their code against business requirements using AI
- generate Gherkin test cases using AI
- generate Gherkin test cases for AspNet controllers

Under the hood it uses call graph from entry point as a context

### How to setup

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
    "SolutionPath": "Full path to .sln file",
    "OpenAiModel": "gpt-4o"
  }
}
```
3) Register Nosimus AI services

```csharp
serviceCollection.AddNosimusAI(configuration);
```

#### How to write a test

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

#### How to generate Gherkin test case

```csharp
var generator = serviceProvider.GetRequiredService<BusinessRequirementExtractor>();
var tests = await generator.GenerateGherkin(typeof(Book), "Borrow", CancellationToken.None);
```

#### How to generate Gherkin test cases for AspNet controllers

```csharp
var generator = serviceProvider.GetRequiredService<BusinessRequirementExtractor>();
var assembly = Assembly.Load(...);
var tests = await generator.GenerateGherkinForAspNetControllers(assembly, CancellationToken.None);
```
