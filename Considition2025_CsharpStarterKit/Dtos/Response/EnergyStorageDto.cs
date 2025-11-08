namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record EnergyStorageDto
{
    public required float CapacityMWh { get; init; }
    public required float Efficiency { get; init; }
    public required float MaxChargePowerMw { get; init; }
    public required float MaxDischargePowerMw { get; init; }
}