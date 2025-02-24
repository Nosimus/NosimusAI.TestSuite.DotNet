namespace NosimusAI.Models;

public sealed class ControllerWithEndpointsResult
{
    public required Type Controller { get; init; }
    public required IReadOnlyCollection<string> Endpoints { get; init; }
}