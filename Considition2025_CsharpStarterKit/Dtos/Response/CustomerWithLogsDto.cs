using System.Text.Json.Serialization;

namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record CustomerWithLogsDto
{
    public required string CustomerId { get; set; }
    public string Name { get; set; }
    public float MaxCharge { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PersonaType Persona { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VehicleType VehicleType { get; set; }
    public List<CustomerLogDto> Logs { get; set; } = [];
}