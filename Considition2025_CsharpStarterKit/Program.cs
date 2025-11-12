using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
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


        // "Turbohill" "Clutchfield"
        var mapName = ConfigParams.MapName != "" ? ConfigParams.MapName : "Clutchfield";
        ConfigParams.MapName = mapName;
        // client.SaveGetMapConfig(mapName);
        await client.PostOwnConfig(new GameInputAndMapConfigDto
        {
            GameInput = new GameInputDto
            {
                MapName = mapName,
                Ticks = [],
            },
        });
        return;
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
            rec.SetGameResponse(gameResponse);

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
            //PrintCustomers(gameResponse, i, "0.7");

            // If we are, we save the current ticks in the list of good ticks
            goodTicks.Add(currentTick);

            UpdateConsumption(gameResponse, rec);

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
                $"Tick:{tick,-4}Id {item.Customer.Id,-10}State: {item.Customer.State,-20}At:{item.At,-20}Charge {item.Customer.ChargeRemaining}"
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

        foreach (var edge in map.Edges)
        {
            var node = map.Nodes.Single(n => n.Id == edge.ToNode);
            foreach (var customer in edge.Customers)
            {
                AddRerouteIfNeeded(customer, node, rec, customerRecommendations, map);
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
    static void AddRerouteIfNeeded(
        CustomerDto customer,
        NodeDto atNode,
        Recommendations rec,
        List<CustomerRecommendationDto> customerRecommendations,
        MapDto map
    )
    {
        ConsumptionRec consumption;
        var guessed = customer.Type == "Truck" ? 0.1f : customer.Type == "Car" ? 0.004f : 0.003f;
        if (!rec.Consumption.ContainsKey(customer.Id))
        {
            consumption = new ConsumptionRec("", "", 0, guessed);
        }
        else
        {
            consumption = rec.Consumption[customer.Id];
            if (!consumption.batteryPtcPerKm.HasValue)
            {
                consumption = new ConsumptionRec("", "", 0, guessed);
            }
        }

        if (customer.State == CustomerState.WaitingForCharger)
            return;

        if (customer.State == CustomerState.RanOutOfJuice)
            return;

        if (customer.State == CustomerState.DestinationReached)
            return;

        if (customerRecommendations.Any(c => c.CustomerId == customer.Id))
            return;

        if (customer.ChargeRemaining > ConfigParams.SkipChargeLimit)
            return;

        if (customer.State == CustomerState.Charging)
            rec.HasCharged.Add(customer.Id);

        // Customer wants to reach its destination
        var path = rec.DijkstraPath(atNode.Id, customer.ToNode);
        var dis = rec.PathDistance(path, atNode.Id, customer.ToNode);
        var batteryCharge = dis * consumption.batteryPtcPerKm + 0.1;
        if (customer.ChargeRemaining > batteryCharge && rec.HasCharged.Contains(customer.Id))
            return;

        var allStations = map.Nodes.Where(n => n.Target is ChargingStationDto).ToList();
        var nearestDistance = float.MaxValue;
        NodeDto bestStation = null;
        var bestStationScore = 0f;
        foreach (var toStation in allStations)
        {
            var toStationPath = rec.DijkstraPath(atNode.Id, toStation.Id);
            // Can I reach this station?
            if (toStationPath == null || !toStationPath.Any())
            {
                continue;
            }

            //Is energy enough to reach this station?
            var p = rec.DijkstraPath(atNode.Id, toStation.Id);
            var d = rec.PathDistance(p, atNode.Id, toStation.Id);

            // This faulty calulation is aparently needed
            var needed = d * customer.EnergyConsumptionPerKm;
            var actual = customer.ChargeRemaining * customer.MaxCharge;
            if (actual < needed)
            {
                continue;
            }

            var stationScore = toStation.GetScore(rec.GameResponse, customer.Persona);
            if (stationScore > bestStationScore)
            {
                bestStationScore = stationScore;
                bestStation = toStation;
            }
        }

        if (bestStation == null)
        {
            //Console.WriteLine($"Found no station to route to. {customer.Id} at node {atNode.Id} ");
            return;
        }

        var chargeTo = 1f; // CalculateChargeTo(atNode, customer);

        customerRecommendations.Add(
            new CustomerRecommendationDto
            {
                CustomerId = customer.Id,
                ChargingRecommendations = new List<ChargingRecommendationDto>
                {
                    new ChargingRecommendationDto { ChargeTo = chargeTo, NodeId = bestStation.Id },
                },
            }
        );
        //Console.WriteLine($"Route {customer.Id} to {bestStation.Id}");
    }

    private static void UpdateConsumption(GameResponseDto gameResponse, Recommendations rec)
    {
        var customers = gameResponse.Map.Edges.SelectMany(e => e.Customers);
        foreach (var edge in gameResponse.Map.Edges)
        {
            foreach (var customer in edge.Customers)
            {
                if (customer.State == CustomerState.Traveling)
                {
                    if (!rec.Consumption.ContainsKey(customer.Id))
                    {
                        // Adding Entry for first time
                        rec.Consumption[customer.Id] = new(edge.FromNode, edge.ToNode, customer.ChargeRemaining, null);
                    }
                }

                if (customer.State == CustomerState.TransitioningToNode)
                {
                    var consumption = rec.Consumption[customer.Id];
                    if (consumption.batteryPtcPerKm == null)
                    {
                        var path = rec.DijkstraPath(edge.FromNode, edge.ToNode);
                        var distance = rec.PathDistance(path, edge.FromNode, edge.ToNode);
                        var chargeAtStart = consumption.chargeStart;
                        var charge = customer.ChargeRemaining;
                        var ptcPerKm = (chargeAtStart - charge) / distance;
                        rec.Consumption[customer.Id] = new(edge.FromNode, edge.ToNode, chargeAtStart, ptcPerKm);
                        // Console.WriteLine($"{customer.Persona} {customer.Type} {ptcPerKm}");
                    }
                }
            }
        }
    }
}

public record ConsumptionRec(string FromNode, string toNode, float chargeStart, float? batteryPtcPerKm);