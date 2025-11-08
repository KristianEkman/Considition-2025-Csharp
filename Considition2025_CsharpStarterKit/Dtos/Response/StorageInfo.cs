namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record struct StorageInfo
{
    public float CapacityMWh { get; init; }
    public float CurrentChargeMWh { get; init; }
    public float StateOfChargePercent { get; init; }
    public float PowerMw { get; init; }
    public int Action { get; init; }
    public float StoredEnergyPrice { get; init; }
    public float StoredGreenPercent { get; init; }
}