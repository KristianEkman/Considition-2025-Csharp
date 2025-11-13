using System.Net.Http.Json;
using System.IO;
using System;
using Considition2025_CsharpStarterKit.Dtos.Request;
using Considition2025_CsharpStarterKit.Dtos.Response;

namespace Considition2025_CsharpStarterKit;

public class ConsiditionClient
{
    private readonly HttpClient client;

    public ConsiditionClient(string _baseUri, string _apiKey)
    {
        client = new HttpClient();
        client.BaseAddress = new Uri(_baseUri);
        client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
    }

    private void LogError(string where, Exception ex, string? responseText = null)
    {
        try
        {
            var lines = new List<string>
            {
                "\n----------------",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                $"ConsiditionClient ERROR in {where}: {ex.Message}",
            };
            if (!string.IsNullOrEmpty(responseText))
                lines.Add($"Response: {responseText}");
            if (!string.IsNullOrEmpty(ex.StackTrace))
                lines.Add(ex.StackTrace);

            File.AppendAllLines("log.txt", lines);
        }
        catch
        {
            // Don't let logging fail the operation
        }

        // Also write a concise message to the console
        Console.WriteLine($"ERROR in {where}: {ex.Message}");
    }

    public async Task<GameResponseDto?> PostGame(GameInputDto _inputDto)
    {
        try
        {
            var response = await client.PostAsJsonAsync("api/game", _inputDto);

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                var msg = $"Error {response.StatusCode} posting game: {text}";
                Console.WriteLine(msg);
                LogError(nameof(PostGame), new Exception(msg), text);
                throw new Exception(text);
            }

            return await response.Content.ReadFromJsonAsync<GameResponseDto>();
        }
        catch (Exception ex)
        {
            LogError(nameof(PostGame), ex);
            throw;
        }
    }

    public async Task<MapDto?> GetMap(string _mapName)
    {
        try
        {
            return await client.GetFromJsonAsync<MapDto>($"api/map?mapName={_mapName}");
        }
        catch (Exception ex)
        {
            LogError(nameof(GetMap), ex);
            return null;
        }
    }

    public async void SaveGetMapConfig(string _mapName)
    {
        try
        {
            var response = await client.GetFromJsonAsync<MapConfigDto>($"api/map?mapName={_mapName}");
            var text = System.Text.Json.JsonSerializer.Serialize(response);
            File.WriteAllText($"map-config-{_mapName}.json", text);
        }
        catch (Exception ex)
        {
            LogError(nameof(SaveGetMapConfig), ex);
        }
    }

    public async Task<GameResponseDto> PostOwnConfig(GameInputAndMapConfigDto config)
    {
        try
        {
            var mapConfigDto = File.ReadAllText($"map-config-{config.GameInput.MapName}.json");
            config.MapConfig = System.Text.Json.JsonSerializer.Deserialize<MapConfigDto>(mapConfigDto);
            var response = await client.PostAsJsonAsync("/api/game-with-custom-map", config);
            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                LogError(nameof(PostOwnConfig), new Exception($"Error {response.StatusCode}"), text);
                throw new Exception(text);
            }

            return await response.Content.ReadFromJsonAsync<GameResponseDto>();
        }
        catch (Exception ex)
        {
            LogError(nameof(PostOwnConfig), ex);
            throw;
        }
    }
}
