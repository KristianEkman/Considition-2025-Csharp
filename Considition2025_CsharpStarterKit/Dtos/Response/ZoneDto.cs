namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record ZoneDto
{
    public required string Id { get; init; }
    public required float TopLeftX { get; init; }
    public required float TopLeftY { get; init; }
    public required float BottomRightX { get; init; }
    public required float BottomRightY { get; init; }
    public required List<EnergySourceDto> EnergySources { get; init; }
    public required List<EnergyStorageDto> EnergyStorages { get; init; }
}