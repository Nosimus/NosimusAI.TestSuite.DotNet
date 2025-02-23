using System.ComponentModel.DataAnnotations;

namespace NosimusAI;

public class NosimusSettings
{
    public const string Section = "Nosimus";
    
    [Required(ErrorMessage = "Open ai key is required", AllowEmptyStrings = false)]
    public string OpenAiKey { get; init; }
    
    [Required(ErrorMessage = "Solution path is required", AllowEmptyStrings = false)]
    public string SolutionPath { get; init; }

    [Required(ErrorMessage = "Open AI model is required", AllowEmptyStrings = false)]
    public string OpenAiModel { get; init; } = "gpt-4o";
}