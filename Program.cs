using Microsoft.Extensions.Http.Resilience;
using Polly;
using WeatherApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("WeatherClient", client =>
{
    // Sets base URL for the weather API
    client.BaseAddress = new Uri("https://api.openweathermap.org");
})
.AddResilienceHandler("MyResilienceStrategy", resilienceBuilder => // Adds resilience policy named "MyResilienceStrategy"
{
    // Retry Strategy configuration
    resilienceBuilder.AddRetry(new HttpRetryStrategyOptions // Configures retry behavior
    {
        MaxRetryAttempts = 4, // Maximum retries before throwing an exception (default: 3)

        Delay = TimeSpan.FromSeconds(2), // Delay between retries (default: varies by strategy)

        BackoffType = DelayBackoffType.Exponential, // Exponential backoff for increasing delays (default)

        UseJitter = true, // Adds random jitter to delay for better distribution (default: false)

        ShouldHandle = new PredicateBuilder<HttpResponseMessage>() // Defines exceptions to trigger retries
        .Handle<HttpRequestException>() // Includes any HttpRequestException
        .HandleResult(response => !response.IsSuccessStatusCode) // Includes non-successful responses
    });

    // Timeout Strategy configuration
    resilienceBuilder.AddTimeout(TimeSpan.FromSeconds(5)); // Sets a timeout limit for requests (throws TimeoutRejectedException)

    // Circuit Breaker Strategy configuration
    resilienceBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions // Configures circuit breaker behavior
    {
        // Tracks failures within this time frame
        SamplingDuration = TimeSpan.FromSeconds(10),

        // Trips the circuit if failure ratio exceeds this within sampling duration (20% failures allowed)
        FailureRatio = 0.2,

        // Requires at least this many successful requests within sampling duration to reset
        MinimumThroughput = 3,

        // How long the circuit stays open after tripping
        BreakDuration = TimeSpan.FromSeconds(1),

        // Defines exceptions to trip the circuit breaker
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<HttpRequestException>() // Includes any HttpRequestException
        .HandleResult(response => !response.IsSuccessStatusCode) // Includes non-successful responses
    });
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API V1");
    options.RoutePrefix = string.Empty; 
});

// Map routes using static map
app.MapWeatherRoutes();

app.Run();
