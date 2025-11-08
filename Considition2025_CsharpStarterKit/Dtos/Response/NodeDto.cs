namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record NodeDto
{
    public required string Id { get; init; }
    public required float PosX { get; init; }
    public required float PosY { get; init; }
    public required string? ZoneId { get; init; }
    public required List<CustomerDto> Customers { get; init; }
    public required TargetDto? Target { get; init; }
}