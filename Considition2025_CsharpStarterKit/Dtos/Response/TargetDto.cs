using System.Text.Json.Serialization;

namespace Considition2025_CsharpStarterKit.Dtos.Response;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(ChargingStationDto), "ChargingStation")]
[JsonDerivedType(typeof(NullTargetDto), "Null")]
public abstract record TargetDto
{
}