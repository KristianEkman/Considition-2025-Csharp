using System.Diagnostics;
using Considition2025_CsharpStarterKit;
using Considition2025_CsharpStarterKit.Dtos.Request;
using Considition2025_CsharpStarterKit.Dtos.Response;

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
var currentTick = GenerateTick(map, 0);

var input = new GameInputDto
{
    MapName = mapName,
    Ticks = [currentTick]
};

for (var i = 0; i < map.Ticks; i++)
{
    GameResponseDto? gameResponse = null;
    while (true)
    {
        Console.WriteLine($"Playing tick: {i} with input: {input}");
        gameResponse = await client.PostGame(input);

        if (gameResponse is null)
        {
            Console.WriteLine("Got no game response");
            return;
        }

        finalScore = gameResponse.CustomerCompletionScore + gameResponse.KwhRevenue;
        Console.WriteLine($"Tick {i} Score: {gameResponse.CustomerCompletionScore} + {gameResponse.KwhRevenue} = {finalScore}");
        PrintCustomers(gameResponse);

        // Check if we are happy with the response
        if (ShouldMoveOnToNextTick(gameResponse))
        {
            // If we are, we save the current ticks in the list of good ticks
            goodTicks.Add(currentTick);

            // Generate new tick for next iteration
            currentTick = GenerateTick(gameResponse.Map, i + 1);

            // Set new input
            input = new GameInputDto
            {
                MapName = mapName,
                PlayToTick = i + 1,
                Ticks = [..goodTicks, currentTick]
            };
            break;
        }

        // Not happy with the result
        // Try with different input
        currentTick = GenerateTick(gameResponse.Map, i);

        input = new GameInputDto
        {
            MapName = mapName,
            PlayToTick = i,
            Ticks = [..goodTicks, currentTick]
        };
    }
}
input.PlayToTick = null;
var serverResponse = await remoteClient.PostGame(input);
Console.WriteLine($"Remote server response: {serverResponse.GameId} Score {serverResponse.Score}");

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

Console.WriteLine($"Final score: {finalScore}");

return;

bool ShouldMoveOnToNextTick(GameResponseDto _response)
{
    // Implement logic to decide if the current tick should continue to be iterated on or move on to the next tick
    return true;
}

TickDto GenerateTick(MapDto _map, int _currentTick)
{
    // Implement logic to generate ticks for the optimal score
    return new TickDto
    {
        Tick = _currentTick,
        CustomerRecommendations = GenerateCustomerRecommendations(_map, _currentTick)
    };
}

List<CustomerRecommendationDto> GenerateCustomerRecommendations(MapDto _map, int _currentTick)
{
    var customerRecommendations = new List<CustomerRecommendationDto>();  

    foreach (var node in _map.Nodes)
    {
        var cargingStations = node.Target as ChargingStationDto;
        if (cargingStations is null)
            continue;
        foreach (var customer in node.Customers)
        {
            customerRecommendations.Add(new CustomerRecommendationDto
            {
                CustomerId = customer.Id,
                ChargingRecommendations = new List<ChargingRecommendationDto>
                {
                    new ChargingRecommendationDto
                    {
                        NodeId = node.Id,
                        ChargeTo = 1
                    }
                }
            });
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
            customerRecommendations.Add(new CustomerRecommendationDto
            {
                CustomerId = customer.Id,
                ChargingRecommendations = new List<ChargingRecommendationDto>
                {
                    new ChargingRecommendationDto
                    {
                        NodeId = toNode.Id,
                        ChargeTo = 1
                    }
                }
            });
         }
    }
    return customerRecommendations;
}