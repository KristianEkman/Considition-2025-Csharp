using System.Numerics;
using System.Text.Json.Serialization;

namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record ChargingStationDto : TargetDto
{
    public required int AmountOfAvailableChargers { get; init; }
    public required int TotalAmountOfBrokenChargers { get; init; }
    public required float ChargeSpeedPerCharger { get; init; }
    public required int TotalAmountOfChargers { get; init; }
}
public record CustomerDto
{
    public required string Id { get; init; }
    public required string Type { get; init; }
    public required string Persona { get; init; }
    public required string FromNode { get; init; }
    public required string ToNode { get; init; }
    public int DepartureTick { get; set; }
    public float ChargeRemaining { get; set; }
    public float MaxCharge { get; set; }

    public float EnergyConsumptionPerKm { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerState State { get; set; }
}
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

public enum CustomerMood
{
    Happy,
    Pleased,
    Content,
    SlightlyMiffed,
    Angry
}


public enum CustomerState
{
    Home,
    WaitingForCharger,
    Charging,
    DoneCharging,
    Traveling,
    TransitioningToNode,
    TransitioningToEdge,
    DestinationReached,
    FailedToCharge,
    RanOutOfJuice
}

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

public record EdgeDto
{
    public required string FromNode { get; init; }
    public required string ToNode { get; init; }
    public required float Length { get; init; }
    public required List<CustomerDto> Customers { get; init; }
}

public record EnergySourceDto
{
    public required string Type { get; init; }
    public required float GenerationCapacity { get; init; }

}

public enum EnergySourceType
{
    Coal,
    NaturalGas,
    Nuclear,
    Solar,
    Wind,
    Hydro,
    Battery
}
public record EnergyStorageDto
{
    public required float CapacityMWh { get; init; }
    public required float Efficiency { get; init; }
    public required float MaxChargePowerMw { get; init; }
    public required float MaxDischargePowerMw { get; init; }
}

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


public record NodeDto
{
    public required string Id { get; init; }
    public required float PosX { get; init; }
    public required float PosY { get; init; }
    public required string? ZoneId { get; init; }
    public required List<CustomerDto> Customers { get; init; }
    public required TargetDto? Target { get; init; }

    internal float GetScore(GameResponseDto gameResponse)
    {
        var station = Target as ChargingStationDto;
        if (station == null)
            return 0;

        var zone = gameResponse.ZoneLogs.Last().Zones.Single(z => z.ZoneId == ZoneId);

        //var price = zone.Sourceinfo.Average(s => s.Value.PricePerMWh);
        //zone.StorageInfo.Sum(z => z.)
        if (zone.TotalProduction - zone.TotalDemand < 0)
            return 0;
                
        //var score = station.AmountOfAvailableChargers * station.ChargeSpeedPerCharger;
        var score = (station.TotalAmountOfChargers - station.TotalAmountOfBrokenChargers) * station.ChargeSpeedPerCharger;

        return score;
    }
}

public record NullTargetDto : TargetDto
{

}


public enum PersonaType
{
    Stressed,
    CostSensitive,
    EcoConscious,
    Neutral,
    DislikesDriving,
}


public record struct SourceMarketInfo
{
    public float Production { get; init; }

    public float PricePerMWh { get; init; }

    public float Revenue { get; init; }

    public bool IsGreen { get; init; }
}

public record struct StorageInfo
{
    public float CapacityMWh { get; init; }
    public float CurrentChargeMWh { get; init; }
    public float StateOfChargePercent { get; init; }
    public float PowerMw { get; init; }
    public int Action { get; init; }
    public float StoredEnergyPrice { get; init; }
    public float StoredGreenPercent { get; init; }
}


[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(ChargingStationDto), "ChargingStation")]
[JsonDerivedType(typeof(NullTargetDto), "Null")]
public abstract record TargetDto
{
}

public enum VehicleType
{
    None,
    Car,
    Truck
}

public enum WeatherType
{
    Clear,
    PartlyCloudy,
    Cloudy,
    Overcast,
    Windy,
    Storm
}

public record ZoneDto
{
    public required string Id { get; init; }
    public required float TopLeftX { get; init; }
    public required float TopLeftY { get; init; }
    public required float BottomRightX { get; init; }
    public required float BottomRightY { get; init; }
    public required List<EnergySourceDto> EnergySources { get; init; }
    public required List<EnergyStorageDto> EnergyStorages { get; init; }
}

public record ZoneLogDto
{
    public string ZoneId { get; set; }
    public float TotalProduction { get; init; }
    public float TotalDemand { get; init; }
    public float TotalRevenue { get; init; }
    public WeatherType WeatherType { get; set; }
    public Vector2 TopRight { get; set; }
    public Vector2 TopLeft { get; set; }
    public Vector2 BottomRight { get; set; }
    public Vector2 BottomLeft { get; set; }

    public Dictionary<EnergySourceType, SourceMarketInfo> Sourceinfo { get; init; }

    public List<StorageInfo> StorageInfo { get; init; }
}

public record ZoneLogsDto
{
    public int Tick { get; set; }
    public List<ZoneLogDto> Zones { get; set; } = [];
}