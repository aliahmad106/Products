# Products API

A production-grade .NET 8 Web API with a React frontend for managing a product catalogue.

## Tech Stack

**Backend**: .NET 8, ASP.NET Core, Entity Framework Core, SQL Server (LocalDB), FluentValidation, Serilog, JWT Authentication

**Frontend**: React 18, TypeScript, Vite, react-hot-toast

**Testing**: xUnit, FluentAssertions, FsCheck (property-based), WebApplicationFactory (integration)

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server LocalDB (included with Visual Studio)

## Getting Started

### Backend

```bash
cd src/Products.Api
dotnet run
```

The API starts at `http://localhost:54736` (HTTP) and `https://localhost:54735` (HTTPS).

Swagger UI: http://localhost:54736/swagger

The database is created automatically on first run via EF Core migrations.

### Frontend

```bash
cd client
npm install
npm run dev
```

The app starts at http://localhost:5173.

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/health` | None | Health check |
| POST | `/api/auth/register` | None | Register a new user |
| POST | `/api/auth/login` | None | Login (sets httpOnly cookie) |
| POST | `/api/auth/refresh` | Cookie | Refresh access token |
| POST | `/api/auth/logout` | Cookie | Logout and revoke refresh token |
| GET | `/api/products` | Required | List all products |
| GET | `/api/products?colour=Blue` | Required | Filter products by colour |
| POST | `/api/products` | Required | Create a product |

## Running Tests

```bash
dotnet test
```

Runs 21 tests across 3 projects:
- **Products.UnitTests** (10) — Service logic and validation
- **Products.IntegrationTests** (5) — Full HTTP pipeline
- **Products.PropertyTests** (6) — Property-based tests with FsCheck

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for system and internal architecture diagrams.

## Project Structure

```
src/
  Products.Api/           → ASP.NET Core host, controllers, middleware
  Products.Application/   → Interfaces, DTOs, validators, services
  Products.Domain/        → Entities (Product, User)
  Products.Infrastructure/→ EF Core, repositories, JWT, auth
tests/
  Products.UnitTests/
  Products.IntegrationTests/
  Products.PropertyTests/
client/                   → React + TypeScript frontend
```

## Security Features

- JWT access tokens (15 min) in httpOnly cookies
- Refresh tokens (7 days) with rotation
- Rate limiting on login/register (5 req/min)
- BCrypt password hashing
- Strong password policy (8+ chars, upper, lower, digit, special)
- CORS restricted to frontend origin
- Global exception handling (no stack traces leaked)
