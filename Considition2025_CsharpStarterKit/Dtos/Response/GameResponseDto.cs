namespace Considition2025_CsharpStarterKit.Dtos.Response;
public record GameResponseDto
{
    public int Tick { get; set; }
    public Guid? GameId { get; set; }
    public required MapDto Map { get; set; }
    public required int Score { get; set; }
    public int KwhRevenue { get; set; }
    public int CustomerCompletionScore { get; set; }
    public List<CustomerWithLogsDto>? CustomerLogs { get; set; }
    public List<string>? UnlockedAchievements { get; set; }
    public List<ZoneLogsDto>? ZoneLogs { get; set; }
}