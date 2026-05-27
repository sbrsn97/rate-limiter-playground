FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RateLimiterPlayground.csproj", "./"]
RUN dotnet restore "RateLimiterPlayground.csproj"

COPY . .
RUN dotnet publish "RateLimiterPlayground.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "RateLimiterPlayground.dll"]