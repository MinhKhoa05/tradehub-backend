# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj & restore
COPY *.sln .
COPY GameTopUp.DAL/*.csproj ./GameTopUp.DAL/
COPY GameTopUp.BLL/*.csproj ./GameTopUp.BLL/
COPY GameTopUp.API/*.csproj ./GameTopUp.API/
COPY GameTopUp.Tests/*.csproj ./GameTopUp.Tests/
RUN dotnet restore

# Copy source code & build
COPY . .
WORKDIR /app/GameTopUp.API
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime nhẹ
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "GameTopUp.API.dll"]