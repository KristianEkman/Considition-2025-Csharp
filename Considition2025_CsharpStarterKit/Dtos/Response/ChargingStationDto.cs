namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record ChargingStationDto : TargetDto
{
    public required int AmountOfAvailableChargers { get; init; }
    public required int TotalAmountOfBrokenChargers { get; init; }
    public required float ChargeSpeedPerCharger { get; init; }
    public required int TotalAmountOfChargers { get; init; }
}