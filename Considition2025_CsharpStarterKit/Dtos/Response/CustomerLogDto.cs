using System.Text.Json.Serialization;

namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record CustomerLogDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerState State { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerMood Mood { get; set; }
    public int Tick { get; set; }
    public float? PosX { get; set; }
    public float? PosY { get; set; }
    public float ChargeRemaining { get; set; }
    public int? TicksSpentWaiting { get; set; }
    public List<string>? Path { get; set; }
    public int? TicksSpentCharging { get; set; }

    public string? Edge { get; set; }
    public string? Node { get; set; }
}