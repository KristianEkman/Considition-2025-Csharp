namespace Considition2025_CsharpStarterKit.Dtos.Request;

public record CustomerRecommendationDto
{
    public string? CustomerId { get; set; }
    public List<ChargingRecommendationDto> ChargingRecommendations { get; set; } = [];

    override public string ToString()
    {
        var stringBuilder = new System.Text.StringBuilder();
        foreach (var recommendation in ChargingRecommendations)
        {
            stringBuilder.AppendLine("  Node: " + recommendation.NodeId + " ChargeTo: " + recommendation.ChargeTo);
        }
        return stringBuilder.ToString();
    }
}

public record ChargingRecommendationDto
{
    public required string NodeId { get; init; }
    public float ChargeTo { get; init; }
}