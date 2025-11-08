using Considition2025_CsharpStarterKit.Dtos.Request;
using Considition2025_CsharpStarterKit.Dtos.Response;

class Recommendations
{
    public Recommendations(MapDto map)
    {
        Adjacency = BuildAdjacency(map);
        Map = map;
    }

    private Dictionary<string, List<(string to, float w)>> Adjacency { get; }
    public MapDto Map { get; }

    private static Dictionary<string, List<(string to, float w)>> BuildAdjacency(MapDto map)
    {
        var adj = new Dictionary<string, List<(string to, float w)>>();
        foreach (var node in map.Nodes)
            adj[node.Id] = new List<(string to, float w)>();

        foreach (var e in map.Edges)
        {
            if (!adj.ContainsKey(e.FromNode)) adj[e.FromNode] = new List<(string to, float w)>();
            adj[e.FromNode].Add((e.ToNode, e.Length));
        }
        return adj;
    }

    public List<string> DijkstraPath(string start, string goal)
    {
        if (!Adjacency.ContainsKey(start) || !Adjacency.ContainsKey(goal)) return new List<string>();

        var dist = new Dictionary<string, float>();
        var prev = new Dictionary<string, string?>();
        var pq = new PriorityQueue<string, float>();

        foreach (var v in Adjacency.Keys)
        {
            dist[v] = float.PositiveInfinity;
            prev[v] = null;
        }
        dist[start] = 0;
        pq.Enqueue(start, 0);

        while (pq.TryDequeue(out var u, out _))
        {
            if (u == goal) break;
            foreach (var (v, w) in Adjacency[u])
            {
                var alt = dist[u] + w;
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                    pq.Enqueue(v, alt);
                }
            }
        }

        if (float.IsInfinity(dist[goal])) return new List<string>();

        // Reconstruct
        var path = new List<string>();
        var cur = goal;
        while (cur != null)
        {
            path.Add(cur);
            cur = prev[cur];
        }
        path.Reverse();
        return path;
    }

    public string FindNextChargingStationAfter(List<string> path, string startNodeId)
    {
        // Find first station appearing AFTER startNodeId in the path
        var nodesById = Map.Nodes.ToDictionary(n => n.Id);
        var seenStart = false;
        foreach (var nodeId in path)
        {
            if (!seenStart)
            {
                if (nodeId == startNodeId) seenStart = true;
                continue;
            }
            var n = nodesById[nodeId];
            if (n.Target is ChargingStationDto) return nodeId;
        }
        return path.Last();
    }

    public float PathDistance(List<string> path, string fromNodeId, string toNodeId)
    {
        var total = 0f;
        var summing = false;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var a = path[i];
            var b = path[i + 1];
            if (a == fromNodeId) summing = true;
            if (summing)
            {
                // find weight a->b
                var w = Adjacency[a].First(x => x.to == b).w;
                total += w;
                if (b == toNodeId) break;
            }
        }
        return total;
    }

    public  static float SafeChargeTargetFraction(float neededEnergy, float maxEnergy, float currentEnergy, float bufferFraction)
    {
        if (maxEnergy <= 0) return 1f;
        var frac = neededEnergy / maxEnergy;
        frac *= (1f + bufferFraction);
        var currentFrac = currentEnergy / maxEnergy;
        var target = MathF.Max(frac, currentFrac);
        return MathF.Min(1f, MathF.Max(0f, target));
    }

    internal bool IsGreen(NodeDto chargingNode)
    {
        var station = (ChargingStationDto)chargingNode.Target;
        var zone = Map.Zones.Single(z => chargingNode.ZoneId == z.Id);
        if (zone == null) return false;
        var good = new[] {"Hydro", "Solar", "Wind" };
        return zone.EnergySources.Any(e => good.Contains(e.Type));
    }
}