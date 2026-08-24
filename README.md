# Ebay Clone Admin System

Admin module for an eBay-style application, implemented with a simple and review-friendly architecture.

## Architecture

```text
MVC
  ↓ HTTP Client
Web API
  ↓
Service
  ↓
Repository
  ↓
DbContext
  ↓
SQL Server
```

The MVC project never accesses the database directly. Business rules and authorization stay in the API.

## Technologies

- .NET 8
- ASP.NET Core Web API
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Docker Compose
- Nginx
- JWT Bearer authentication

## Features

- Admin authentication and role authorization
- User approval, block and unblock
- Dashboard statistics
- Product hide and unhide moderation
- Review hide and unhide moderation
- Feedback monitoring with filtering and pagination
- Order list and detail monitoring
- Dispute assign, resolve and reject workflow
- Audit logging
- Responsive Admin MVC panel with light/dark mode

## Run locally

Requirements: .NET 8 and SQL Server LocalDB.

```powershell
dotnet build EbayClone.sln
dotnet run --project EbayClone.API --urls http://localhost:5173
dotnet run --project EbayClone.MVC --urls http://127.0.0.1:5090
```

Open `http://127.0.0.1:5090/Account/Login`.

For local development, copy `EbayClone.API/appsettings.Development.example.json` to
`EbayClone.API/appsettings.Development.json`, then set your LocalDB password, admin password
and a JWT key of at least 32 characters. The copied file is ignored by Git.
The MVC development URL is in `EbayClone.MVC/appsettings.Development.json`.

## Run with Docker

Copy `.env.example` to `.env` and replace the values where appropriate. `.env` is ignored by Git.

```bash
docker compose up --build
```

Open `http://localhost`. Nginx routes `/` to MVC and `/api/` to the API.

Docker uses the `sqlserver` service name in the API connection string; LocalDB is only used for local development.

Demo data is controlled by `EnableDemoSeed`. It is enabled in the Development example and Docker Compose, but disabled by default in the common configuration. The seed is idempotent and only adds records with the `demo.*` markers; it never clears existing data.

## Authorization design

The current system uses Role-Based Authorization with one Admin Panel role:

| Area | Access |
| --- | --- |
| Admin Panel | Only users with the `Admin` role |
| Marketplace | Regular `User` accounts and their buyer/seller business behavior |

All Admin API controllers require a valid JWT with the `Admin` role. Regular `User` accounts cannot access the Admin Panel or Admin API.

## Health check

The API exposes:

```text
GET /health
```

It is used as a deployment and service availability check.

## Admin account

The local demo account is configured by `AdminAccount` settings:

```text
Email: admin@gmail.com
Password: Admin@123
```

The marketplace demo accounts use the password `Demo@123`; they have the regular `User` role and cannot access the Admin Panel.

For Docker, set `ADMIN_PASSWORD` in `.env`; do not commit real credentials.

## CI

GitHub Actions runs restore, build, test and Docker image builds on pushes and pull requests to `main` or `master`.

## API testing with Postman

Import these files from `docs/postman`:

- `EbayClone_Admin_API.postman_collection.json`
- `EbayClone_Admin_API.postman_environment.json`

Run `Authentication/Login Admin` first. Its test script stores the returned JWT in `jwtToken`; the other requests inherit the collection-level Bearer authorization automatically.
