namespace WeatherApi;

public static class RouteBuilderExtension
{
    public static void MapWeatherRoutes(this IEndpointRouteBuilder app, string appId)
    {
        app.MapGet("api/getweatherasync", async (IHttpClientFactory httpClientFactory) =>
        {
            var httpClient = httpClientFactory.CreateClient("WeatherClient");
            var response = await httpClient.GetFromJsonAsync<WeatherForecastRecord>($"/data/2.5/weather?lat=44.34&lon=10.99&appid={appId}");

            if (response != null && response is WeatherForecastRecord)
            {
                return Results.Json(response);
            }

            return Results.Problem("Unable to retrieve weather data.");
        })
            .WithName("GetWeatherAsync")
            .WithTags("Weather");
    }
}

public record WeatherForecastRecord(
    Coord coord,
    IReadOnlyList<Weather> weather,
    string @base,
    Main main,
    int visibility,
    Wind wind,
    Clouds clouds,
    int dt,
    Sys sys,
    int timezone,
    int id,
    string name,
    int cod
);

public record Clouds(
    int all
);

public record Coord(
    double lon,
    double lat
);

public record Main(
    double temp,
    double feels_like,
    double temp_min,
    double temp_max,
    int pressure,
    int humidity,
    int sea_level,
    int grnd_level
);

public record Sys(
    int type,
    int id,
    string country,
    int sunrise,
    int sunset
);

public record Weather(
    int id,
    string main,
    string description,
    string icon
);

public record Wind(
    double speed,
    int deg,
    double gust
);

