using System.Numerics;
using System.Text.Json.Serialization;

namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record ZoneLogDto
{
    public string ZoneId { get; set; }
    public float TotalProduction { get; init; }
    public float TotalDemand { get; init; }
    public float TotalRevenue { get; init; }
    public WeatherType WeatherType { get; set; }
    public Vector2 TopRight { get; set; }
    public Vector2 TopLeft { get; set; }
    public Vector2 BottomRight { get; set; }
    public Vector2 BottomLeft { get; set; }

    public Dictionary<EnergySourceType, SourceMarketInfo> Sourceinfo { get; init; }

    public List<StorageInfo> StorageInfo { get; init; }
}