# StayOps India - Database

Schema ownership follows a normal EF-Core-first workflow, with raw SQL layered on top for
everything EF Core migrations don't (and shouldn't) own:

| Folder | Owner | Purpose |
|---|---|---|
| `01-schema/` | EF Core migrations (exported) | `01_initial_schema.sql` is `dotnet ef migrations script --idempotent` output from `src/StayOps.Infrastructure/Persistence/Migrations` - kept here for review/offline execution. **The C# migrations are the actual source of truth**; this file is a generated artifact, not hand-maintained. |
| `02-indexes/` | Hand-written | Additional indexes not expressed via EF Fluent configuration (composite/covering indexes tuned for report queries). |
| `03-views/` | Hand-written | Reporting views used by Dapper-based report endpoints. |
| `04-stored-procedures/` | Hand-written | All transactional workflows called out in the README (holds, reservations, cancellations, folios, POS, night audit, reports) - executed via Dapper, never inlined as ad-hoc C# LINQ-to-SQL. |
| `05-seed-data/` | N/A - see note | All seed data (hotels, room types/rooms, GST rules, cancellation policies, rate plans, corporate account, travel agent, POS outlet, guests, sample stays/finance records) is seeded by C# at API startup instead - see below. Folder kept for the structure the brief asks for; `NOTES.md` inside explains why. |

Application-level seeding (`src/StayOps.Infrastructure/Persistence/Seed/`), run automatically once
by `Program.cs` on every API startup (idempotent - checks `HotelGroups` first):

- `DbSeeder` - ensures the 6 roles exist.
- `DemoDataSeeder` - reference/demo data (2 hotels, room types/rooms, GST rules, cancellation
  policies, rate plans, a corporate account + contract, a travel agent + contract, a POS outlet,
  guests, and one seeded user per role via `UserManager` - passwords must go through Identity's
  hasher, so this can't be raw SQL).
- `SampleStaySeeder` - a handful of sample stays/finance records (a paid-and-invoiced historical
  checkout, a live checked-in stay with a POS charge, an upcoming corporate-billed booking, and a
  cancelled online booking with a pending refund) created by calling the **real stored procedures**
  (`sp_CreateReservation`, `sp_CheckInGuest`, `sp_PostFolioCharge`, `sp_RecordFolioPayment`,
  `sp_CheckOutGuest`, `sp_GenerateGstInvoice`, `sp_CreateInventoryHold`,
  `sp_ConfirmOnlineReservation`, `sp_CancelReservation`, `sp_PostPosChargeToFolio`) rather than raw
  INSERTs, so sample data can never drift from the invariants those procedures enforce. This step
  is best-effort at startup: if the stored procedures haven't been applied yet, it logs a warning
  and skips itself rather than failing API startup - run `scripts/setup-database.ps1` first.

## Setup order

**Zero manual steps required** - `dotnet run` alone stands up the entire database:

```powershell
dotnet run --project src/StayOps.Api
```

On every startup, `Program.cs`: (1) runs `Database.Migrate()` for all EF-owned tables, (2) applies
every script under `02-indexes/`, `03-views/`, and `04-stored-procedures/` via
`DatabaseScriptRunner` (splitting on `GO` batch separators the way `sqlcmd` would), then (3) seeds
roles, demo reference data, and sample stays. All three steps are idempotent, so restarting the
API never duplicates anything.

`scripts/setup-database.ps1` runs the same migration + script-application steps standalone (no API
startup involved) - useful for CI, or for pre-warming a database before the API's first run. Point
`Database:ScriptsPath` in configuration at a different folder if you deploy the `database/` folder
somewhere other than two levels above the API's content root (that's what the Docker image does -
see root README).

## sqlcmd gotcha: QUOTED_IDENTIFIER

Several tables have filtered unique indexes (e.g. `Reservations.IdempotencyKey`,
`FolioTransactions.UniquePostingKey`, `Payments.IdempotencyKey`). SQL Server requires
`QUOTED_IDENTIFIER ON` for any INSERT/UPDATE/DELETE against such a table, and bakes that setting
into a stored procedure/function at the time it is compiled. All scripts under
`04-stored-procedures/` explicitly `SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;` before their
`CREATE OR ALTER` statement for exactly this reason. .NET's `SqlConnection` (and therefore EF Core
and Dapper) already defaults to `QUOTED_IDENTIFIER ON`, so the application is unaffected - this
only matters if you run ad-hoc DML by hand through `sqlcmd`, whose own default is `OFF`. If you
hit error 1934 doing that, prefix your ad-hoc batch with `SET QUOTED_IDENTIFIER ON;`.

## Why LocalDB for the demo

This session's environment only has a domain-restricted shared SQL Server instance the current
Windows account cannot authenticate against, so `(localdb)\MSSQLLocalDB` (SQL Server Express
engine, bundled with Visual Studio) is used as a documented demo default. Point
`ConnectionStrings:DefaultConnection` at any SQL Server 2022 Developer/Express/Standard instance
for a non-demo environment - nothing in the schema or procedures is LocalDB-specific.
