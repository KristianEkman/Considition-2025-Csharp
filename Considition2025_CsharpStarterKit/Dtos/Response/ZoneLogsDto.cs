namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record ZoneLogsDto
{
    public int Tick { get; set; }
    public List<ZoneLogDto> Zones { get; set; } = [];
}