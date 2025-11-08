namespace Considition2025_CsharpStarterKit.Dtos.Request;

public record TickDto
{
    public int Tick { get; set; }
    public List<CustomerRecommendationDto> CustomerRecommendations { get; set; } = [];

    override public string ToString()
    {
        var stringBuilder = new System.Text.StringBuilder();
        stringBuilder.AppendLine($"Tick: {Tick}");
        foreach (var recommendation in CustomerRecommendations)
        {
            stringBuilder.AppendLine("Customer: " + recommendation.CustomerId);
            stringBuilder.AppendLine(recommendation.ToString());
        }
        return stringBuilder.ToString();
    }
}