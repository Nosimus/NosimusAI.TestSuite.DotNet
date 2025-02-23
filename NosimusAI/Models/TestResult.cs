namespace NosimusAI.Models;

public record TestResult
{
    public bool Passed { get; init; }
    public required string? ProblemExplanation { get; init; }
    public required string? FixSuggestion { get; init; }
}