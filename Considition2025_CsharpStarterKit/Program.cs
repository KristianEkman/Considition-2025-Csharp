using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Considition2025_CsharpStarterKit;
using Considition2025_CsharpStarterKit.Dtos.Request;
using Considition2025_CsharpStarterKit.Dtos.Response;

public class Program
{
    public static async Task Main(string[] args)
    {
        var apiKey = "5eeb877d-5150-4ba6-884e-23888104341f";
        var client = new ConsiditionClient("http://localhost:8181", apiKey);
        var remoteClient = new ConsiditionClient("https://api.considition.com", apiKey);

        ConfigParams.ReadInput(args);
        ConfigParams.WriteLine();
        // log time stamp
        File.AppendAllLines(
            "log.txt",
            new[]
            {
                "\n----------------",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ConfigParams.WriteLine(),
            }
        );

        var mapName = ConfigParams.MapName != "" ? ConfigParams.MapName : "Turbohill";
        ConfigParams.MapName = mapName;
        var map = await client.GetMap(mapName);
        Recommendations rec = new Recommendations();
        rec.SetMap(map);
        rec.BuildAdjacency(map);

        if (map is null)
        {
            Console.WriteLine("Failed to fetch map!");
            return;
        }

        Console.WriteLine($"{map.Name} {map.DimX}x{map.DimY} {map.Ticks}");

        var finalScore = 0.0f;
        var goodTicks = new List<TickDto>();

        // Initial input for the first tick
        var currentTick = new TickDto { Tick = 0, CustomerRecommendations = [] };

        var input = new GameInputDto { MapName = mapName, Ticks = [currentTick] };

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
            rec.SetMap(gameResponse.Map);

            finalScore = gameResponse.CustomerCompletionScore + gameResponse.KwhRevenue;

            if (i == map.Ticks - 1)
            {
                PrintCustomers(gameResponse, i);
                File.AppendAllLines(
                    "log.txt",
                    [
                        $"Final Score: {gameResponse.CustomerCompletionScore} + {gameResponse.KwhRevenue} = {finalScore} {PrintCustomerInfo(gameResponse.Map)}",
                    ]
                );
            }

            Console.WriteLine(
                $"Tick {i} Score: {gameResponse.CustomerCompletionScore} + {gameResponse.KwhRevenue} = {finalScore} {PrintCustomerInfo(gameResponse.Map)}"
            );
            // PrintCustomers(gameResponse, i);

            // If we are, we save the current ticks in the list of good ticks
            goodTicks.Add(currentTick);

            // Generate new tick for next iteration
            currentTick = GenerateTick(gameResponse.Map, i + 1, rec);

            // Set new input
            input = new GameInputDto
            {
                MapName = mapName,
                PlayToTick = i + 1,
                Ticks = [.. goodTicks, currentTick],
            };
        }

        if (ConfigParams.SaveToServer)
        {
            input.PlayToTick = null;
            var serverResponse = await remoteClient.PostGame(input);
            Console.WriteLine(
                $"Remote server response: {serverResponse.GameId} Score {serverResponse.CustomerCompletionScore} + {serverResponse.KwhRevenue} = {serverResponse.Score}"
            );
        }
    }

    static void PrintCustomers(GameResponseDto response, int tick, string filter = null)
    {
        var map = response.Map;
        var items = new List<(CustomerDto Customer, string At)>();
        foreach (var node in map.Nodes)
        {
            foreach (var customer in node.Customers)
            {
                items.Add((customer, node.Id));
            }
        }

        foreach (var edge in map.Edges)
        {
            foreach (var customer in edge.Customers)
            {
                items.Add((customer, $"{edge.FromNode}->{edge.ToNode}"));
            }
        }

        var filtered = items.OrderBy(c => c.Customer.Id).ToArray();
        if (!string.IsNullOrEmpty(filter))
            filtered = filtered.Where(f => f.Customer.Id == filter).ToArray();

        foreach (var item in filtered) //.Where(x => x.Customer.Id == "0.7"))
        {
            Console.WriteLine(
                $"Tick:{tick, -4}Id {item.Customer.Id, -10}State: {item.Customer.State, -20}At:{item.At, -10}Charge {item.Customer.ChargeRemaining}"
            );
        }
    }

    static TickDto GenerateTick(MapDto map, int tick, Recommendations rec)
    {
        // Implement logic to generate ticks for the optimal score
        if (tick == 1)
        {
            return new TickDto
            {
                Tick = tick,
                CustomerRecommendations = new List<CustomerRecommendationDto>(),
            };
        }

        return new TickDto
        {
            Tick = tick,
            CustomerRecommendations = GenerateCustomerRecommendations(map, tick, rec),
        };
    }

    static List<CustomerRecommendationDto> GenerateCustomerRecommendations(
        MapDto map,
        int _currentTick,
        Recommendations rec
    )
    {
        var customerRecommendations = new List<CustomerRecommendationDto>();

        foreach (var node in map.Nodes)
        {
            foreach (var customer in node.Customers)
            {
                AddRerouteIfNeeded(customer, node, rec, customerRecommendations, map);
            }
        }

        foreach (var node in map.Nodes)
        {
            foreach (var customer in node.Customers)
            {
                AddRecommendation(customerRecommendations, node, customer, rec);
            }
        }

        foreach (var edge in map.Edges)
        {
            var toNode = map.Nodes.Single(n => n.Id == edge.ToNode);
            foreach (var customer in edge.Customers)
            {
                AddRecommendation(customerRecommendations, toNode, customer, rec);
            }
        }
        return customerRecommendations;
    }

    static string PrintCustomerInfo(MapDto map)
    {
        var customers = new List<CustomerDto>();
        customers.AddRange(map.Edges.SelectMany(e => e.Customers));
        customers.AddRange(map.Nodes.SelectMany(n => n.Customers));
        return $"Count: {customers.Count} Ran out: {customers.Count(c => c.State == CustomerState.RanOutOfJuice)} Reached: {customers.Count(c => c.State == CustomerState.DestinationReached)}";
    }

    static void AddRecommendation(
        List<CustomerRecommendationDto> customerRecommendations,
        NodeDto atNode,
        CustomerDto customer,
        Recommendations rec
    )
    {
        var chargeTo = 1f;
        var station = atNode.Target as ChargingStationDto;
        if (station is null)
        {
            return;
        }

        if (station.TotalAmountOfChargers - station.TotalAmountOfBrokenChargers < 1)
        {
            return;
        }

        //var path = rec.DijkstraPath(atNode.Id, customer.ToNode); // this could be done once
        //var nextStation = rec.FindNextChargingStationAfter(path, chargingNode.Id);
        //var distanceToGoal = rec.PathDistance(path, atNode.Id, customer.ToNode);
        //var distanceToNextStation = rec.PathDistance(path, chargingNode.Id, nextStation);
        //var neededEnergyToGoal = distanceToGoal * customer.EnergyConsumptionPerKm * 0.9f;
        //var neededEnergyToNextStation = distanceToNextStation * customer.EnergyConsumptionPerKm;
        //var energyLeft = customer.ChargeRemaining * customer.MaxCharge;
        //if (energyLeft > neededEnergyToGoal)
        //    return;
        // It seams to be a good idea to charge

        // Select if we should charge at this node or the next.

        chargeTo = 1f; // (neededEnergyToGoal / customer.MaxCharge) * 1.2f;

        if (customer.ChargeRemaining > ConfigParams.SkipChargeLimit)
            return;

        // Chose the best charger, green or cost? Does nothing it seams
        //if (rec.IsGreen(chargingNode))
        //    chargeTo = 1;

        if (chargeTo > 1)
            chargeTo = 1;

        customerRecommendations.Add(
            new CustomerRecommendationDto
            {
                CustomerId = customer.Id,
                ChargingRecommendations = new List<ChargingRecommendationDto>
                {
                    new ChargingRecommendationDto { NodeId = atNode.Id, ChargeTo = chargeTo },
                },
            }
        );

        //Console.WriteLine($"Recommending charge to {chargeTo} at station {chargingNode.Id}");
    }

    static void AddRerouteIfNeeded(
        CustomerDto customer,
        NodeDto atNode,
        Recommendations rec,
        List<CustomerRecommendationDto> customerRecommendations,
        MapDto map
    )
    {
        var path = rec.DijkstraPath(atNode.Id, customer.ToNode);
        var dis = rec.PathDistance(path, atNode.Id, customer.ToNode);
        var energy = dis * customer.EnergyConsumptionPerKm;
        if (customer.ChargeRemaining * customer.MaxCharge > energy)
            return;

        foreach (var nodeId in path)
        {
            var node = map.Nodes.Single(n => n.Id == nodeId);
            var station = node.Target as ChargingStationDto;
            if (
                station != null
                && station.TotalAmountOfChargers - station.TotalAmountOfBrokenChargers > 0
            )
                return;
            // also filter out if there is no power in that zone
        }

        // No good station found on path
        var allStations = map.Nodes.Where(n => n.Target is ChargingStationDto).ToList();
        var nearestDistance = float.MaxValue;
        NodeDto bestStation = null;
        foreach (var toStation in allStations)
        {
            var toStationPath = rec.DijkstraPath(atNode.Id, toStation.Id);
            // Can I reach this station?
            if (toStationPath == null || !toStationPath.Any())
            {
                continue;
            }

            // Can this station reach the goal?
            var toGoalPAth = rec.DijkstraPath(toStation.Id, customer.ToNode);
            if (toGoalPAth == null || !toGoalPAth.Any())
            {
                continue;
            }

            toStationPath.AddRange(toGoalPAth.Skip(1));
            var dist = rec.PathDistance(toStationPath, atNode.Id, toStation.Id);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                bestStation = toStation;
            }
        }
        if (bestStation == null)
            return;
        customerRecommendations.Add(
            new CustomerRecommendationDto
            {
                CustomerId = customer.Id,
                ChargingRecommendations = new List<ChargingRecommendationDto>
                {
                    new ChargingRecommendationDto { ChargeTo = 1, NodeId = bestStation.Id },
                },
            }
        );
        //Console.WriteLine($"Re-route {customer.Id} to {bestStation.Id}");
    }
}
