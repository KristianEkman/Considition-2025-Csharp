using Considition2025_CsharpStarterKit;
using Considition2025_CsharpStarterKit.Dtos.Request;
using Considition2025_CsharpStarterKit.Dtos.Response;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

var apiKey = "5eeb877d-5150-4ba6-884e-23888104341f";
var client = new ConsiditionClient("http://localhost:8181", apiKey);
var remoteClient = new ConsiditionClient("https://api.considition.com", apiKey);

const string mapName = "Turbohill";
var map = await client.GetMap(mapName);

if (map is null)
{
    Console.WriteLine("Failed to fetch map!");
    return;
}

var finalScore = 0.0f;
var goodTicks = new List<TickDto>();

// Initial input for the first tick
var currentTick = new TickDto { Tick = 0, CustomerRecommendations = [] };

var input = new GameInputDto
{
    MapName = mapName,
    Ticks = [currentTick]
};

Recommendations rec = null;

for (var i = 0; i < map.Ticks; i++)
{
    GameResponseDto? gameResponse = null;
    //Console.WriteLine($"Playing tick: {i} with input: {input}");
    gameResponse = await client.PostGame(input);

    if (gameResponse is null)
    {
        Console.WriteLine("Got no game response");
        return;
    }
    rec = new Recommendations(gameResponse.Map);

    finalScore = gameResponse.CustomerCompletionScore + gameResponse.KwhRevenue;

    Console.WriteLine($"Tick {i} Score: {gameResponse.CustomerCompletionScore} + {gameResponse.KwhRevenue} = {finalScore} {PrintCustomerInfo(gameResponse.Map)}");
    // PrintCustomers(gameResponse);

    // If we are, we save the current ticks in the list of good ticks
    goodTicks.Add(currentTick);

    // Generate new tick for next iteration
    currentTick = GenerateTick(gameResponse.Map, i + 1, rec);

    // Set new input
    input = new GameInputDto
    {
        MapName = mapName,
        PlayToTick = i + 1,
        Ticks = [.. goodTicks, currentTick]
    };
    if (i == map.Ticks - 1)
        PrintCustomers(gameResponse);
}

string PrintCustomerInfo(MapDto map)
{
    var customers = new List<CustomerDto>();
    customers.AddRange(map.Edges.SelectMany(e => e.Customers));
    customers.AddRange(map.Nodes.SelectMany(n => n.Customers));
    return $"Count: {customers.Count} Ran out: {customers.Count(c => c.State == CustomerState.RanOutOfJuice)} Reached: {customers.Count(c => c.State == CustomerState.DestinationReached)}";
}

input.PlayToTick = null;
var serverResponse = await remoteClient.PostGame(input);
Console.WriteLine($"Remote server response: {serverResponse.GameId} Score {serverResponse.CustomerCompletionScore} + {serverResponse.KwhRevenue} = {serverResponse.Score}");

void PrintCustomers(GameResponseDto response)
{
    var map = response.Map;
    var customers = new List<CustomerDto>();
    customers.AddRange(map.Edges.SelectMany(e => e.Customers));
    customers.AddRange(map.Nodes.SelectMany(n => n.Customers));

    foreach (var customer in customers.OrderBy(c => c.Id))
    {
        Console.WriteLine($"Id {customer.Id,-10}State: {customer.State,-20}Charge {customer.ChargeRemaining}");
    }
}


bool ShouldMoveOnToNextTick(GameResponseDto _response)
{
    // Implement logic to decide if the current tick should continue to be iterated on or move on to the next tick
    return true;
}

TickDto GenerateTick(MapDto _map, int _currentTick, Recommendations rec)
{
    // Implement logic to generate ticks for the optimal score
    return new TickDto
    {
        Tick = _currentTick,
        CustomerRecommendations = GenerateCustomerRecommendations(_map, _currentTick, rec)
    };
}

List<CustomerRecommendationDto> GenerateCustomerRecommendations(MapDto _map, int _currentTick, Recommendations rec)
{
    var customerRecommendations = new List<CustomerRecommendationDto>();
    foreach (var node in _map.Nodes)
    {
        var cargingStations = node.Target as ChargingStationDto;
        if (cargingStations is null)
            continue;
        foreach (var customer in node.Customers)
        {
            AddRecommendation(customerRecommendations, node, customer, rec);
        }
    }

    foreach (var edge in _map.Edges)
    {
        var toNode = map.Nodes.Single(n => n.Id == edge.ToNode);
        var chargingStation = toNode.Target as ChargingStationDto;
        if (chargingStation is null)
            continue;
        foreach (var customer in edge.Customers)
        {
            AddRecommendation(customerRecommendations, toNode, customer, rec);
        }
    }
    return customerRecommendations;
}

static void AddRecommendation(List<CustomerRecommendationDto> customerRecommendations, NodeDto chargingNode, CustomerDto customer, Recommendations rec)
{
    // Plan the minimum needed to reach the next station
    //var (chargeTo, targetStationId, distKm) =
    //    ChargingPlanner.ComputeChargeToNextStation(map, customer, node.Id, safetyMargin: 0.20f);
    //var isGreen = ChargingPlanner.IsGreenZone(_map, node.ZoneId);
    var chargeTo = 1f;
    var station = (ChargingStationDto)chargingNode.Target!;
    if (station.TotalAmountOfChargers - station.TotalAmountOfBrokenChargers < 1)
    {
        return;
    }

    //var path = rec.DijkstraPath(customer.FromNode, customer.ToNode); // this could be done once
    //var nextNode = rec.FindNextChargingStationAfter(path, chargingNode.Id);
    //var distance = rec.PathDistance(path, chargingNode.Id, nextNode);
    //var neededEnergy = distance * customer.EnergyConsumptionPerKm * 1.1f;
    //var energyLeft = customer.ChargeRemaining * customer.MaxCharge;

    //if (energyLeft > neededEnergy)
    //    return;

    //var chargeTo = (neededEnergy / customer.MaxCharge);
    if (customer.ChargeRemaining > .43f)
        return;

    // Chose the best charger, green or cost?
    if (rec.IsGreen(chargingNode))
        chargeTo = 1;

    if (chargeTo > 1)
        chargeTo = 1;

    customerRecommendations.Add(new CustomerRecommendationDto
    {
        CustomerId = customer.Id,
        ChargingRecommendations = new List<ChargingRecommendationDto>
                {
                    new ChargingRecommendationDto
                    {
                        NodeId = chargingNode.Id,
                        ChargeTo = chargeTo
                    }
                }
    });
}

