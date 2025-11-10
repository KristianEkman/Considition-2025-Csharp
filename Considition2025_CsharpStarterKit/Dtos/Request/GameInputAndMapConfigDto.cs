using System.ComponentModel;
using Considition2025_CsharpStarterKit.Dtos.Response;

namespace Considition2025_CsharpStarterKit.Dtos.Request;

public record GameInputAndMapConfigDto
{
    public GameInputDto? GameInput { get; set; }
    public MapConfigDto? MapConfig { get; set; }

    public static GameInputAndMapConfigDto Test1()
    {
        var dto = new GameInputAndMapConfigDto
        {
            GameInput = new GameInputDto { MapName = "Test1" },
            MapConfig = new MapConfigDto
            {
                Name = "Test1",
                Customers = 10,
                PersonaDifficulty = 5,
                ChargingStations = 4,
                Cars = new VehicleConfigDto2
                {
                    MinBaseSpeed = 40,
                    MaxBaseSpeed = 60,
                    MinBatteryCapacity = 50,
                    MaxBatteryCapacity = 100,
                    MinConsumptionPerKm = 0.15f,
                    MaxConsumptionPerKm = 0.25f,
                },
                Trucks = new VehicleConfigDto
                {
                    MinBaseSpeed = 30,
                    MaxBaseSpeed = 50,
                    MinBatteryCapacity = 150,
                    MaxBatteryCapacity = 300,
                    MinConsumptionPerKm = 0.4f,
                    MaxConsumptionPerKm = 0.6f,
                },
                DimX = 30,
                DimY = 30,
                Ticks = 200,
                PercentageHoles = 0.1f,
                PercentageBridges = 0.05f,
                EnergyMix = new EnergyMixDto
                {
                    EnergySources = new List<EnergySourceDto>
                    {
                        new EnergySourceDto { Type = "Solar", GenerationCapacity = 50 },
                        new EnergySourceDto { Type = "Wind", GenerationCapacity = 30 },
                    },
                    EnergyStorages = new List<EnergyStorageDto>
                    {
                        new EnergyStorageDto
                        {
                            CapacityMWh = 100,
                            Efficiency = 0.9f,
                            MaxChargePowerMw = 20,
                            MaxDischargePowerMw = 20,
                        },
                    },
                },
            },
        };
        return dto;
    }
}

public record MapConfigDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int DimX { get; set; }
    public int DimY { get; set; }
    public string? Seed { get; set; }
    public int Ticks { get; set; }
    public int ChargingStations { get; set; }
    public int Events { get; set; }
    public int Customers { get; set; }
    public float? PercentageHoles { get; set; }
    public float? PercentageBridges { get; set; }
    public int? MinimumIslandSize { get; set; }
    public int? ZoneCountX { get; set; }
    public int? ZoneCountY { get; set; }

    public EnergyMixDto? EnergyMix { get; set; }

    public int PersonaDifficulty { get; set; }
    public float? EdgeLengthModifier { get; set; }
    public int? MaxChargersPerStation { get; set; }
    public int? MinChargerSpeed { get; set; }
    public int? MaxChargerSpeed { get; set; }
    public float? MaxBrokenChargerPercentage { get; set; }
    public float? MinBrokenChargerPercentage { get; set; }
    public int? NewCustomerMinWait { get; set; }
    public int? NewCustomerMaxWait { get; set; }
    public float? NewCustomerMinChargeRemaining { get; set; }
    public float? PercentageTrucks { get; set; }
    public float? CloudVolatility { get; set; }
    public float? CloudOffset { get; set; }
    public float? WindVolatility { get; set; }
    public float? WindOffset { get; set; }

    public VehicleConfigDto? Trucks { get; set; }
    public VehicleConfigDto2? Cars { get; set; }

    public List<PersonaConfigDto>? Personas { get; set; } = [];

    public bool? AchievementsEnabled { get; set; }
}

public record EnergyMixDto
{
    public List<EnergySourceDto>? EnergySources { get; set; } = [];
    public List<EnergyStorageDto>? EnergyStorages { get; set; } = [];
}

public record VehicleConfigDto
{
    public float? MinBaseSpeed { get; set; }
    public float? MaxBaseSpeed { get; set; }
    public float? MinBatteryCapacity { get; set; }
    public float? MaxBatteryCapacity { get; set; }
    public float? MinConsumptionPerKm { get; set; }
    public float? MaxConsumptionPerKm { get; set; }
}

public record VehicleConfigDto2
{
    public float? MinBaseSpeed { get; set; }
    public float? MaxBaseSpeed { get; set; }
    public float? MinBatteryCapacity { get; set; }
    public float? MaxBatteryCapacity { get; set; }
    public float? MinConsumptionPerKm { get; set; }
    public float? MaxConsumptionPerKm { get; set; }
}

public record PersonaConfigDto
{
    public int Persona { get; set; }
    public float Percentage { get; set; }
}
