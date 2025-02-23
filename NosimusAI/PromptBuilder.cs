namespace NosimusAI;

internal static class PromptBuilder
{
    public static string BuildTestPrompt(string entryPoint, string businessRequirement, IReadOnlyCollection<string> pathes)
    {
        var code = string.Join('\n', pathes);

        return @$"Act as a code quality analyzer for .NET applications. Your task is to compare the provided code snippet with the specified business requirements and determine whether the code fully satisfies all requirements.
Inputs:
Code: {code}
Entry point: {entryPoint}
Business Requirements: {businessRequirement}
Process:
Analyze the code line-by-line and identify how it addresses (or fails to address) each business requirement.
Set the Passed flag to true only if the code meets all requirements exactly as stated. If even one requirement is partially met or unmet, set Passed to false.
Provide a detailed feedback list explaining which requirements passed/failed and why";
    }
    
    public static string BuildGherkinTestExtractorPrompt(string entryPoint, IReadOnlyCollection<string> pathes)
    {
        var code = string.Join('\n', pathes);

        return @$"Act as a code quality analyzer for .NET applications. Your task is to analyse the provided code snippet and generate Gherkin test cases.
Inputs:
Code: {code}
Entry point: {entryPoint}
Process:
Analyze the code line-by-line, understand it and generate comprehensive Gherkin test cases.";
    }
}