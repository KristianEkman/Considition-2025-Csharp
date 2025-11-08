using System;
using System.Collections.Generic;
using System.Linq;
using Considition2025_CsharpStarterKit.Dtos.Response;
using Considition2025_CsharpStarterKit.Dtos.Request;

static class ChargingPlanner
{
    /// <summary>
    /// Compute how much to charge (0..1) at currentNode so the customer can reach the next charging station.
    /// </summary>
    /// <param name="map">Current map.</param>
    /// <param name="customer">Customer to plan for.</param>
    /// <param name="currentNodeId">
    /// The node where the customer will charge right now (usually the node they're at).
    /// </param>
    /// <param name="safetyMargin">Extra margin (e.g. 0.10 = +10%).</param>
    public static (float chargeToFraction, string targetStationNodeId, float requiredKm)
        ComputeChargeToNextStation(MapDto map, CustomerDto customer, string currentNodeId, float safetyMargin = 0.10f)
    {
        // 1) Build adjacency for Dijkstra (directed graph as per Edges list).
        var neighbors = map.Edges
            .GroupBy(e => e.FromNode)
            .ToDictionary(g => g.Key, g => g.Select(e => (to: e.ToNode, dist: e.Length)).ToList());

        // 2) Collect all nodes that have a ChargingStation (except the current node if it also has one).
        var stationNodeIds = map.Nodes
            .Where(n => n.Target is ChargingStationDto && n.Id != currentNodeId)
            .Select(n => n.Id)
            .ToHashSet();

        // If there is no *other* station, fall back to "full charge".
        if (stationNodeIds.Count == 0)
            return (1f, currentNodeId, 0f);

        // 3) Dijkstra from currentNodeId to nearest station.
        var (nearestStation, distKm) = DijkstraToNearestStation(currentNodeId, neighbors, stationNodeIds);

        // 4) Energy needed and charge target.
        float neededEnergy = distKm * customer.EnergyConsumptionPerKm;       // in same units as ChargeRemaining/MaxCharge
        neededEnergy *= (1f + safetyMargin);                                 // add safety buffer

        // Clamp between current charge and battery capacity
        float targetEnergy = MathF.Min(customer.MaxCharge, MathF.Max(customer.ChargeRemaining, neededEnergy));
        float chargeToFraction = targetEnergy / customer.MaxCharge;
        chargeToFraction = MathF.Max(0f, MathF.Min(1f, chargeToFraction));

        return (chargeToFraction, nearestStation, distKm);
    }

    private static (string nodeId, float distKm) DijkstraToNearestStation(
        string start,
        Dictionary<string, List<(string to, float dist)>> neighbors,
        HashSet<string> stationNodeIds)
    {
        var dist = new Dictionary<string, float> { [start] = 0f };
        var pq = new PriorityQueue<string, float>();
        pq.Enqueue(start, 0f);

        while (pq.Count > 0)
        {
            pq.TryDequeue(out var u, out var du);
            if (stationNodeIds.Contains(u))
                return (u, du);

            if (!neighbors.TryGetValue(u, out var outs)) continue;
            foreach (var (v, w) in outs)
            {
                var alt = du + w;
                if (!dist.TryGetValue(v, out var dv) || alt < dv)
                {
                    dist[v] = alt;
                    pq.Enqueue(v, alt);
                }
            }
        }
        // If unreachable, return the start with 0 km so caller can default to full charge.
        return (start, 0f);
    }
    public static bool IsGreenZone(MapDto map, string? zoneId)
    {
        if (string.IsNullOrWhiteSpace(zoneId)) return false;
        var zone = map.Zones.FirstOrDefault(z => z.Id == zoneId);
        if (zone is null) return false;

        // Green if any renewable is present in the zone
        return zone.EnergySources.Any(es =>
            es.Type.Equals("Solar", StringComparison.OrdinalIgnoreCase) ||
            es.Type.Equals("Wind", StringComparison.OrdinalIgnoreCase) ||
            es.Type.Equals("Hydro", StringComparison.OrdinalIgnoreCase));
    }

}
