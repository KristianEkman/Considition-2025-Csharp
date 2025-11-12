using System.Net.Http.Json;
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

    public async Task<GameResponseDto?> PostGame(GameInputDto _inputDto)
    {
        var response = await client.PostAsJsonAsync("api/game", _inputDto);

        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync();
            throw new Exception(text);
        }

        return await response.Content.ReadFromJsonAsync<GameResponseDto>();
    }

    public async Task<MapDto?> GetMap(string _mapName)
    {
        return await client.GetFromJsonAsync<MapDto>($"api/map?mapName={_mapName}");
    }

    public async void SaveGetMapConfig(string _mapName)
    {
        var response = await client.GetFromJsonAsync<MapConfigDto>($"api/map?mapName={_mapName}");
        var text = System.Text.Json.JsonSerializer.Serialize(response);
        File.WriteAllText($"map-config-{_mapName}.json", text);
    }

    public async Task<GameResponseDto> PostOwnConfig(GameInputAndMapConfigDto config)
    {
        var mapConfigDto = File.ReadAllText($"map-config-{config.GameInput.MapName}.json");
        config.MapConfig = System.Text.Json.JsonSerializer.Deserialize<MapConfigDto>(mapConfigDto);
        var response = await client.PostAsJsonAsync("/api/game-with-custom-map", config);
        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync();
            throw new Exception(text);
        }

        return await response.Content.ReadFromJsonAsync<GameResponseDto>();
    }
}
