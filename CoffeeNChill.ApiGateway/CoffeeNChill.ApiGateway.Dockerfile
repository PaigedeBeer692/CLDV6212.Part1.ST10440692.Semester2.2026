FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CoffeeNChill.ApiGateway.csproj", "./"]
RUN dotnet restore "CoffeeNChill.ApiGateway.csproj"

COPY . .
RUN dotnet publish "CoffeeNChill.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CoffeeNChill.ApiGateway.dll"]
