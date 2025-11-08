namespace Considition2025_CsharpStarterKit.Dtos.Response;

public class MapDto
{
    public string? Name { get; init; }
    public required int DimX { get; init; }
    public required int DimY { get; init; }
    public required List<NodeDto> Nodes { get; init; }
    public required List<EdgeDto> Edges { get; init; }
    public required List<ZoneDto> Zones { get; init; }
    public int? Ticks { get; init; }
}