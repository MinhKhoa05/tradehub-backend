# TradeHub - P2P Marketplace Backend

A RESTful backend API for a peer-to-peer marketplace built with ASP.NET Core.

## Tech Stack
- ASP.NET Core Web API
- MySQL
- Dapper
- JWT Authentication

## Architecture
3-layer architecture:

- TradeHub.API – Controllers / HTTP layer
- TradeHub.BLL – Business logic / services
- TradeHub.DAL – Database access (repositories)

## Features
- Authentication (JWT)
- Product management
- Cart system
- Order checkout
- Wallet & wallet transactions

## Run project

```bash
dotnet restore
dotnet build
dotnet run --project TradeHub.API
