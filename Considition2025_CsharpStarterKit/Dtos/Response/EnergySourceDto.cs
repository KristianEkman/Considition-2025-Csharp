namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record EnergySourceDto
{
    public required string Type { get; init; }
    public required float GenerationCapacity { get; init; }

}