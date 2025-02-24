namespace NosimusAI.Models;

public record GherkinTest
{
    public required string TestText { get; init; }
}

public record ControllerEndpointGherkinTest
{
    public required Type ControllerType { get; init; }
    
    public required string Endpoint { get; init; }
    
    public required string TestText { get; init; }
}