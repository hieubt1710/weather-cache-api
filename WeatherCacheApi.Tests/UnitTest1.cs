using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using WeatherCacheApi;

namespace WeatherCacheApi.Tests;

public class WeatherEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeatherEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWeather_ReturnsCachedData_WhenCacheExists()
    {
        // Arrange
        var city = "hanoi";
        var fakeCachedWeather = new { resolvedAddress = "Hanoi, Vietnam", days = new[] { new { temp = 25.5 } } };
        var fakeCacheBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fakeCachedWeather));

        // Mock the Distributed Cache
        var mockCache = new Mock<IDistributedCache>();
        mockCache.Setup(c => c.GetAsync(city, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(fakeCacheBytes);

        // Swap the real cache with our mock in the test server
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(mockCache.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync($"/api/weather/{city}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hanoi, Vietnam", responseString);

        // Verify that the cache was checked exactly once
        mockCache.Verify(c => c.GetAsync(city, It.IsAny<CancellationToken>()), Times.Once);
    }
}