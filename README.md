# Weather API with Redis Caching

This is a solution to the [Weather API project](https://roadmap.sh/projects/weather-api-wrapper-service) on roadmap.sh. 

## Overview
This project is a RESTful Weather API built with .NET 10 Minimal APIs. It fetches real-time weather data from the Visual Crossing API and uses a Redis distributed cache to store the results for 12 hours. This drastically reduces external API calls and improves response times. 

## Features
- **3rd Party Integration:** Fetches real weather data based on city names.
- **Redis Caching:** Implements `IDistributedCache` to store data with a 12-hour TTL.
- **Rate Limiting:** Protects the API from abuse (max 10 requests per minute).
- **Unit Testing:** Includes an xUnit test project using `WebApplicationFactory` and `Moq` to verify caching logic without hitting external services.

## Tech Stack
- C# / .NET 10 (Minimal APIs)
- StackExchange.Redis
- xUnit & Moq (Testing)

## How to Run Locally

1. **Clone the repository:**

   git clone [https://github.com/hieubt1710/weather-cache-api.git](https://github.com/hieubt1710/weather-cache-api.git)

2. **Set up Environment Variables:**
Update the appsettings.json file with your own API keys:

Get a free Weather API key from Visual Crossing.

Set up a free Redis database (e.g., via Docker or Upstash) and paste the connection string.

3. **Run the API:**
```bash
cd WeatherCacheApi
dotnet run
```
4. **Test the Endpoint:**
Navigate to http://localhost:<port>/api/weather/{city-name} (e.g., /api/weather/london).