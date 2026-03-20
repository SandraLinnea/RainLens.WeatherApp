using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using RainLens.WeatherApp.Models;

namespace RainLens.WeatherApp.Services;

public sealed class WeatherService(HttpClient httpClient)
{
    private static readonly Dictionary<int, (string Summary, string Icon)> WeatherCodeMap = new()
    {
        [0] = ("Clear sky", "☀"),
        [1] = ("Mainly clear", "☀"),
        [2] = ("Partly cloudy", "⛅"),
        [3] = ("Overcast", "☁"),
        [45] = ("Fog", "〰"),
        [48] = ("Depositing rime fog", "〰"),
        [51] = ("Light drizzle", "☂"),
        [53] = ("Drizzle", "☂"),
        [55] = ("Dense drizzle", "☂"),
        [56] = ("Freezing drizzle", "❄"),
        [57] = ("Freezing drizzle", "❄"),
        [61] = ("Slight rain", "☂"),
        [63] = ("Rain", "☂"),
        [65] = ("Heavy rain", "☂"),
        [66] = ("Freezing rain", "❄"),
        [67] = ("Freezing rain", "❄"),
        [71] = ("Slight snow", "❄"),
        [73] = ("Snow", "❄"),
        [75] = ("Heavy snow", "❄"),
        [77] = ("Snow grains", "❄"),
        [80] = ("Rain showers", "☔"),
        [81] = ("Rain showers", "☔"),
        [82] = ("Heavy showers", "☔"),
        [85] = ("Snow showers", "❄"),
        [86] = ("Heavy snow showers", "❄"),
        [95] = ("Thunderstorm", "⚡"),
        [96] = ("Thunderstorm with hail", "⚡"),
        [99] = ("Thunderstorm with hail", "⚡")
    };

    public async Task<WeatherDashboardData> GetDashboardAsync(string cityName, CancellationToken cancellationToken = default)
    {
        var location = await FindCityAsync(cityName, cancellationToken)
            ?? throw new InvalidOperationException($"No matching city was found for '{cityName}'.");

        return await GetDashboardAsync(location, cancellationToken);
    }

    public async Task<WeatherDashboardData> GetDashboardAsync(CityLocation location, CancellationToken cancellationToken = default)
    {
        var url =
            $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={location.Longitude.ToString(CultureInfo.InvariantCulture)}" +
            "&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation_probability,weather_code,wind_speed_10m,visibility" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
            "&forecast_days=5&timezone=auto";

        var forecast = await httpClient.GetFromJsonAsync<OpenMeteoForecastResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Weather data could not be loaded.");

        if (forecast.Current is null || forecast.Daily is null)
        {
            throw new InvalidOperationException("Weather data response was incomplete.");
        }

        var currentPresentation = GetPresentation(forecast.Current.WeatherCode);
        var currentWeather = new CurrentWeather
        {
            Summary = currentPresentation.Summary,
            Icon = currentPresentation.Icon,
            TemperatureC = (int)Math.Round(forecast.Current.Temperature2m),
            FeelsLikeC = (int)Math.Round(forecast.Current.ApparentTemperature),
            Metrics = new List<WeatherMetric>
            {
                new() { Label = "Humidity", Value = $"{forecast.Current.RelativeHumidity2m}%", Icon = "◌" },
                new() { Label = "Wind", Value = $"{(int)Math.Round(forecast.Current.WindSpeed10m)} km/h", Icon = "↗" },
                new() { Label = "Visibility", Value = $"{Math.Round(forecast.Current.Visibility / 1000d, 1):0.#} km", Icon = "◍" },
                new() { Label = "Rain Chance", Value = $"{forecast.Current.PrecipitationProbability}%", Icon = "☂" }
            }
        };

        var forecastDays = new List<ForecastDay>();
        for (var index = 0; index < forecast.Daily.Time.Count; index++)
        {
            var date = DateOnly.Parse(forecast.Daily.Time[index], CultureInfo.InvariantCulture);
            var presentation = GetPresentation(forecast.Daily.WeatherCode[index]);

            forecastDays.Add(new ForecastDay
            {
                Date = date,
                DayLabel = date.ToString("dddd", CultureInfo.InvariantCulture),
                Summary = presentation.Summary,
                Icon = presentation.Icon,
                RainChance = forecast.Daily.PrecipitationProbabilityMax[index],
                MaxTempC = (int)Math.Round(forecast.Daily.Temperature2mMax[index]),
                MinTempC = (int)Math.Round(forecast.Daily.Temperature2mMin[index])
            });
        }

        return new WeatherDashboardData
        {
            Location = location,
            Current = currentWeather,
            Forecast = forecastDays,
            UpdatedAt = DateTimeOffset.Now
        };
    }

    private async Task<CityLocation?> FindCityAsync(string cityName, CancellationToken cancellationToken)
    {
        var url =
            $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(cityName)}&count=1&language=en&format=json";

        var response = await httpClient.GetFromJsonAsync<OpenMeteoGeocodingResponse>(url, cancellationToken);
        var result = response?.Results?.FirstOrDefault();

        if (result is null)
        {
            return null;
        }

        return new CityLocation
        {
            Name = result.Name,
            Country = result.Country,
            AdminArea = result.Admin1,
            Latitude = result.Latitude,
            Longitude = result.Longitude
        };
    }

    private static (string Summary, string Icon) GetPresentation(int weatherCode) =>
        WeatherCodeMap.TryGetValue(weatherCode, out var presentation)
            ? presentation
            : ("Weather update", "⛅");

    private sealed class OpenMeteoGeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<OpenMeteoGeocodingResult>? Results { get; set; }
    }

    private sealed class OpenMeteoGeocodingResult
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("country")]
        public required string Country { get; set; }

        [JsonPropertyName("admin1")]
        public string? Admin1 { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    private sealed class OpenMeteoForecastResponse
    {
        [JsonPropertyName("current")]
        public OpenMeteoCurrentWeather? Current { get; set; }

        [JsonPropertyName("daily")]
        public OpenMeteoDailyForecast? Daily { get; set; }
    }

    private sealed class OpenMeteoCurrentWeather
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public int RelativeHumidity2m { get; set; }

        [JsonPropertyName("apparent_temperature")]
        public double ApparentTemperature { get; set; }

        [JsonPropertyName("precipitation_probability")]
        public int PrecipitationProbability { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed10m { get; set; }

        [JsonPropertyName("visibility")]
        public double Visibility { get; set; }
    }

    private sealed class OpenMeteoDailyForecast
    {
        [JsonPropertyName("time")]
        public required List<string> Time { get; set; }

        [JsonPropertyName("weather_code")]
        public required List<int> WeatherCode { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public required List<double> Temperature2mMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public required List<double> Temperature2mMin { get; set; }

        [JsonPropertyName("precipitation_probability_max")]
        public required List<int> PrecipitationProbabilityMax { get; set; }
    }
}
