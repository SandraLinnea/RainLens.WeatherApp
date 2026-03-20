namespace RainLens.WeatherApp.Models;

public sealed class CityLocation
{
    public required string Name { get; init; }

    public required string Country { get; init; }

    public string? AdminArea { get; init; }

    public required double Latitude { get; init; }

    public required double Longitude { get; init; }

    public string DisplayName =>
        string.IsNullOrWhiteSpace(AdminArea) || string.Equals(AdminArea, Name, StringComparison.OrdinalIgnoreCase)
            ? $"{Name}, {Country}"
            : $"{Name}, {AdminArea}, {Country}";
}

public sealed class WeatherMetric
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public required string Icon { get; init; }
}

public sealed class CurrentWeather
{
    public required string Summary { get; init; }

    public required string Icon { get; init; }

    public required int TemperatureC { get; init; }

    public required int FeelsLikeC { get; init; }

    public required IReadOnlyList<WeatherMetric> Metrics { get; init; }
}

public sealed class ForecastDay
{
    public required DateOnly Date { get; init; }

    public required string DayLabel { get; init; }

    public required string Summary { get; init; }

    public required string Icon { get; init; }

    public required int RainChance { get; init; }

    public required int MaxTempC { get; init; }

    public required int MinTempC { get; init; }
}

public sealed class WeatherDashboardData
{
    public required CityLocation Location { get; init; }

    public required CurrentWeather Current { get; init; }

    public required IReadOnlyList<ForecastDay> Forecast { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}
