# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WeatherCacheApi.csproj", "./"]
RUN dotnet restore "WeatherCacheApi.csproj"
COPY . .
RUN dotnet publish "WeatherCacheApi.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WeatherCacheApi.dll"]