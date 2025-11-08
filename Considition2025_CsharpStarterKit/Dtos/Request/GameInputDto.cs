using System.ComponentModel;

namespace Considition2025_CsharpStarterKit.Dtos.Request;

public record GameInputDto
{
    [Description("The name of the map to play")]
    public string? MapName { get; set; }

    [Description("The ticks of the game")]
    public List<TickDto> Ticks { get; set; } = [];

    [Description("Optional. Tick index to play to. If the map has 100 ticks and this value is lower the game engine will stop at this lower value. This value is not allowed when submitting maps for final judgment")]
    public int? PlayToTick { get; set; }

    public override string ToString()
    {
        var stringBuilder = new System.Text.StringBuilder();
        stringBuilder.AppendLine("MapName: " + MapName);
        foreach (var tick in Ticks)
        {
            stringBuilder.AppendLine(tick.ToString());
        }
        return stringBuilder.ToString();
    }
}