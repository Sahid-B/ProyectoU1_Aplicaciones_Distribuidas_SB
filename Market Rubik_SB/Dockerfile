FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MiniMarketApp.csproj", "./"]
RUN dotnet restore "MiniMarketApp.csproj"

COPY . .
RUN dotnet build "MiniMarketApp.csproj" -c Release -o /app/build
RUN dotnet publish "MiniMarketApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MiniMarketApp.dll"]
