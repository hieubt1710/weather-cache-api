using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Load Configurations
var redisConnection = builder.Configuration["Redis:ConnectionString"];
var weatherApiKey = builder.Configuration["WeatherApi:ApiKey"];
var weatherApiBaseUrl = builder.Configuration["WeatherApi:BaseUrl"];

// 2. Add Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "WeatherApp_";
});

// 3. Add HttpClient for 3rd Party API requests
builder.Services.AddHttpClient();

// 4. Implement Rate Limiting (e.g., 10 requests per minute)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Basic", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseRateLimiter();

// 5. The Weather Endpoint
app.MapGet("/api/weather/{city}", async (string city, IDistributedCache cache, IHttpClientFactory httpClientFactory) =>
{
    var cacheKey = city.ToLowerInvariant();

    // Diagram Step 1 & 2: Check Redis Cache
    var cachedWeather = await cache.GetStringAsync(cacheKey);

    if (!string.IsNullOrEmpty(cachedWeather))
    {
        // Return cached response
        return Results.Ok(JsonDocument.Parse(cachedWeather));
    }

    // Diagram Step 3: Request 3rd Party Weather API
    var client = httpClientFactory.CreateClient();
    var requestUrl = $"{weatherApiBaseUrl}{city}?key={weatherApiKey}";

    try
    {
        var response = await client.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem($"Failed to fetch weather data. Status: {response.StatusCode}");
        }

        // Diagram Step 4: Weather API Response
        var weatherData = await response.Content.ReadAsStringAsync();

        // Diagram Step 5: Save Cached Results with a 12-hour expiration (EX flag equivalent)
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
        };
        await cache.SetStringAsync(cacheKey, weatherData, cacheOptions);

        return Results.Ok(JsonDocument.Parse(weatherData));
    }
    catch (HttpRequestException)
    {
        return Results.Problem("An error occurred while communicating with the 3rd party weather service.");
    }
})
.RequireRateLimiting("Basic")
.WithName("GetWeather");

app.Run();

public partial class Program { }