using Considition2025_CsharpStarterKit;
using Considition2025_CsharpStarterKit.Dtos.Request;
using Considition2025_CsharpStarterKit.Dtos.Response;

class Recommendations
{
    public void SetMap(MapDto map)
    {
        Map = map;

        var stations = map.Nodes.Where(n => n.Target is ChargingStationDto);
        var dict = new List<(string, int)>();
        foreach (var station in stations)
        {
            var charger = (ChargingStationDto)station.Target;
            var item = (station.Id, charger.TotalAmountOfChargers - charger.TotalAmountOfBrokenChargers);
            dict.Add(item);
        }
        StationSchedule = new StationSchedule(dict.ToDictionary());
    }

    public void SetGameResponse(GameResponseDto gr)
    {
        GameResponse = gr;
    }

    public StationSchedule StationSchedule { get; private set; }

    private Dictionary<string, List<(string to, float w)>> Adjacency { get; set; }
    public MapDto Map { get; private set; }
    private Dictionary<(string start, string goal), List<string>> PathCache { get; set; } = new();

    public Dictionary<string, ConsumptionRec> Consumption { get; set; } = new();
    public List<(string customerId, string stationId)> HasCharged { get; } = new();
    public GameResponseDto GameResponse { get; private set; }

    public void BuildAdjacency(MapDto map)
    {
        var adj = new Dictionary<string, List<(string to, float w)>>();
        foreach (var node in map.Nodes)
            adj[node.Id] = new List<(string to, float w)>();

        foreach (var e in map.Edges)
        {
            if (!adj.ContainsKey(e.FromNode)) adj[e.FromNode] = new List<(string to, float w)>();
            adj[e.FromNode].Add((e.ToNode, e.Length));
        }
        Adjacency = adj;
    }

    public List<string> DijkstraPath(string start, string goal)
    {
        if (!Adjacency.ContainsKey(start) || !Adjacency.ContainsKey(goal)) return new List<string>();

        // Check cache first
        var cacheKey = (start, goal);
        if (PathCache.TryGetValue(cacheKey, out var cachedPath))
            return new List<string>(cachedPath);

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

        // Cache the result
        PathCache[(start, goal)] = new List<string>(path);
        return path;
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

}