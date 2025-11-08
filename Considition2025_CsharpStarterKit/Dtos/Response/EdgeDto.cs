namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record EdgeDto
{
    public required string FromNode { get; init; }
    public required string ToNode { get; init; }
    public required float Length { get; init; }
    public required List<CustomerDto> Customers { get; init; }
}