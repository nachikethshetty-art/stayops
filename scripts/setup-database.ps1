#Requires -Version 5.1
<#
.SYNOPSIS
    Creates/updates the StayOps India database: EF Core migrations, then indexes/views/stored
    procedures/reference seed data from the database/ folder, in the correct order.

.PARAMETER Server
    SQL Server instance to target. Defaults to the LocalDB instance used throughout local dev.

.PARAMETER Database
    Database name. Defaults to StayOpsIndiaDb (must match ConnectionStrings:DefaultConnection
    in src/StayOps.Api/appsettings.json if you change it).
#>
param(
    [string]$Server = "(localdb)\MSSQLLocalDB",
    [string]$Database = "StayOpsIndiaDb"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "==> Applying EF Core migrations (creates all CRUD-owned tables)..." -ForegroundColor Cyan
dotnet ef database update `
    --project (Join-Path $repoRoot "src\StayOps.Infrastructure") `
    --startup-project (Join-Path $repoRoot "src\StayOps.Api")

function Invoke-SqlFile([string]$Path) {
    Write-Host "==> $Path" -ForegroundColor Cyan
    sqlcmd -S $Server -d $Database -E -C -i $Path
    if ($LASTEXITCODE -ne 0) {
        throw "sqlcmd failed for $Path (exit code $LASTEXITCODE)"
    }
}

$dbRoot = Join-Path $repoRoot "database"

Write-Host "==> Layering on indexes, views, stored procedures, and reference/demo seed data..." -ForegroundColor Cyan
Get-ChildItem (Join-Path $dbRoot "02-indexes") -Filter *.sql | Sort-Object Name | ForEach-Object { Invoke-SqlFile $_.FullName }
Get-ChildItem (Join-Path $dbRoot "03-views") -Filter *.sql | Sort-Object Name | ForEach-Object { Invoke-SqlFile $_.FullName }
Get-ChildItem (Join-Path $dbRoot "04-stored-procedures") -Filter *.sql | Sort-Object Name | ForEach-Object { Invoke-SqlFile $_.FullName }

Write-Host "==> Database setup complete. Start the API (it seeds roles/demo data/sample stays on first run)." -ForegroundColor Green
