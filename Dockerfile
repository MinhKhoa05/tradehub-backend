# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj & restore
COPY *.sln .
COPY TradeHub.DAL/*.csproj ./TradeHub.DAL/
COPY TradeHub.BLL/*.csproj ./TradeHub.BLL/
COPY TradeHub.API/*.csproj ./TradeHub.API/
RUN dotnet restore

# Copy source code & build
COPY . .
WORKDIR /app/TradeHub.API
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime nhẹ
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "TradeHub.API.dll"]