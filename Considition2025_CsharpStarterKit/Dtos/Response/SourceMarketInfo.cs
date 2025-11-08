namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record struct SourceMarketInfo
{
    public float Production { get; init; }
    
    public float PricePerMWh { get; init; }
    
    public float Revenue { get; init; }
    
    public bool IsGreen { get; init; }
}