using System.Text.Json;
using Microsoft.JSInterop;

namespace RainLens.WeatherApp.Services;

public sealed class BrowserStorageService(IJSRuntime jsRuntime)
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await jsRuntime.InvokeAsync<string?>("rainLensStorage.getItem", key);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await jsRuntime.InvokeVoidAsync("rainLensStorage.setItem", key, json);
    }
}
