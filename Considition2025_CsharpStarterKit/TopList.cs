using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class TopList
{
    public class Entry
    {
        public string MapName { get; set; } = string.Empty;
        public double Score { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }

    // Public list of entries (best score per map). Kept sorted descending by Score.
    public List<Entry> Entries { get; set; } = new List<Entry>();

    // Default file path where the toplist is persisted. Uses application base directory so it's writable
    // both during development and when running the built binary.
    private static string DefaultFilePath = "toplist.json";

    public TopList()
    {
    }

    // Add a score for a map. If the map already exists and the new score is not strictly better,
    // the score is disregarded. If new score is better, update it. Persist after change.
    public bool Add(string mapName, double score)
    {
        if (string.IsNullOrEmpty(mapName)) return false;

        var existing = Entries.FirstOrDefault(e => string.Equals(e.MapName, mapName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            // Disregard scores that are not the best for this mapName.
            if (score <= existing.Score) return false;

            existing.Score = score;
            existing.UpdatedUtc = DateTime.UtcNow;
        }
        else
        {
            Entries.Add(new Entry { MapName = mapName, Score = score, UpdatedUtc = DateTime.UtcNow });
            Console.WriteLine($"✌️ New top score for map '{mapName}': {score} ✌️");
        }

        // Keep list sorted descending by score
        Entries = Entries.OrderByDescending(e => e.Score).ThenBy(e => e.MapName).ToList();

        Save();
        return true;
    }

    public void Save(string? filePath = null)
    {
        var path = filePath ?? DefaultFilePath;
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(path, json);
    }

    public static TopList Load(string? filePath = null)
    {
        var path = filePath ?? DefaultFilePath;
        Console.WriteLine($"Loading TopList from {path}");

        if (!File.Exists(path)) return new TopList();
        var json = File.ReadAllText(path);
        var tl = JsonSerializer.Deserialize<TopList>(json);
        if (tl == null) return new TopList();
        // Ensure ordering and non-null fields
        tl.Entries = tl.Entries ?? new List<Entry>();
        tl.Entries = tl.Entries.OrderByDescending(e => e.Score).ThenBy(e => e.MapName).ToList();
        return tl;
    }
}