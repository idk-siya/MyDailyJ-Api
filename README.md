# MyDailyJ.Api

ASP.NET Core (.NET 8) Web API backend for the [MyDailyJ Android app](https://github.com/idk-siya/MyDailyJ) — handles registration/login (JWT) and CRUD + search over journal entries, backed by SQL Server via EF Core.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server to connect to — either:
  - **SQL Server LocalDB** for local development (ships with Visual Studio / SQL Server Express installs), or
  - **Azure SQL** (or any SQL Server instance) for a real deployment
- `dotnet-ef` CLI tool: `dotnet tool install --global dotnet-ef` (if not already installed)

## 1. Configure the database connection

This repo intentionally does **not** ship a real connection string — you wire that up yourself.

- `appsettings.Development.json` already points at a local **SQL Server LocalDB** instance (`(localdb)\mssqllocaldb`) so you can run it locally out of the box if you have LocalDB installed.
- `appsettings.json` (used in any non-Development environment) has an **empty** `ConnectionStrings:DefaultConnection` and an **empty** `Jwt:Key` — set real values via environment variables or `dotnet user-secrets`, never by committing them:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your Azure SQL connection string>"
dotnet user-secrets set "Jwt:Key" "<a long random secret, 32+ chars>"
```

(The `Jwt:Key` in `appsettings.Development.json` is a throwaway dev-only secret — fine for local LocalDB testing, but generate a fresh one for anything real.)

## 2. Apply migrations

Once your connection string is set:

```bash
cd MyDailyJ.Api
dotnet ef database update
```

This creates the `Users` and `JournalEntries` tables (see `Migrations/` for the schema, generated from `Models/User.cs` and `Models/JournalEntry.cs`).

## 3. Run it

```bash
cd MyDailyJ.Api
dotnet run
```

Swagger UI is available at `https://localhost:<port>/swagger` in development — use it to try `auth/register`, `auth/login`, and then paste the returned token into the "Authorize" button to call the `entries` endpoints.

## API surface

All routes are prefixed `/api`. `entries/*` require `Authorization: Bearer <token>` (issued by login/register); `auth/*` do not.

| Method | Path             | Auth | Purpose            |
|--------|------------------|------|--------------------|
| POST   | `auth/register`  | —    | Create an account, returns a JWT |
| POST   | `auth/login`     | —    | Log in, returns a JWT |
| GET    | `entries`        | ✅   | List the current user's entries |
| GET    | `entries/search?q=` | ✅ | Search the current user's entries by title/content |
| GET    | `entries/{id}`   | ✅   | Get one entry (must belong to the caller) |
| POST   | `entries`        | ✅   | Create an entry |
| PUT    | `entries/{id}`   | ✅   | Update an entry |
| DELETE | `entries/{id}`   | ✅   | Delete an entry |

Request/response shapes in `Dtos/` mirror the Android app's Kotlin models exactly (`LoginRequest`, `RegisterRequest`, `AuthResponse`, `JournalEntryDto`) — including using `string` for `createdAt`/`updatedAt` (ISO-8601) rather than a native date type, since that's what the app's `JournalEntry` model expects.

## Connecting the Android app to this API

In the [MyDailyJ Android app](https://github.com/idk-siya/MyDailyJ), `app/src/main/java/com/example/poefn/ui/login/RetrofitClient.kt` has a `BASE_URL` constant:

- `http://10.0.2.2:5000/api/` reaches this API running on your machine, from the Android **emulator** (default `dotnet run` port may differ — check your console output).
- For a physical device on the same network, use your machine's LAN IP instead of `10.0.2.2`.
- For a real deployment, point it at your deployed API's HTTPS URL (and drop `android:usesCleartextTraffic="true"` from the app's manifest once you're off plain HTTP).

The app already stores the JWT returned by login (`SessionManager`) and attaches it as a `Bearer` token on every request via an OkHttp interceptor in `RetrofitClient`, so no further app-side wiring is needed.

## Project structure

- `Controllers/` — `AuthController` (register/login), `EntriesController` (CRUD + search, `[Authorize]`)
- `Models/` — EF Core entities (`User`, `JournalEntry`)
- `Dtos/` — request/response shapes matching the Android app
- `Data/AppDbContext.cs` — EF Core `DbContext`
- `Services/JwtTokenService.cs` — issues signed JWTs on login/register
- `Migrations/` — EF Core migration history
