using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace StayOps.Infrastructure.Persistence;

/// <summary>
/// Applies the hand-written SQL under database/02-indexes, database/03-views, and
/// database/04-stored-procedures on startup, right after EF Core migrations create the
/// CRUD-owned tables - so `dotnet run` and `docker compose up` both produce a fully-set-up
/// database with zero manual steps. Idempotent (CREATE OR ALTER / IF NOT EXISTS guards in the
/// scripts themselves), so re-running on every startup is safe and cheap.
/// </summary>
public static class DatabaseScriptRunner
{
    private static readonly string[] ScriptFolders = ["02-indexes", "03-views", "04-stored-procedures"];

    public static async Task ApplyAsync(string connectionString, string databaseFolderPath, ILogger logger, CancellationToken ct = default)
    {
        if (!Directory.Exists(databaseFolderPath))
        {
            logger.LogWarning("Database scripts folder '{Path}' not found - skipping indexes/views/stored-procedure setup. " +
                               "Run scripts/setup-database.ps1 manually, or check Database:ScriptsPath configuration.", databaseFolderPath);
            return;
        }

        foreach (var folder in ScriptFolders)
        {
            var folderPath = Path.Combine(databaseFolderPath, folder);
            if (!Directory.Exists(folderPath)) continue;

            foreach (var file in Directory.GetFiles(folderPath, "*.sql").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                await ApplyScriptFileAsync(connectionString, file, ct);
                logger.LogInformation("Applied database script {File}", Path.GetFileName(file));
            }
        }
    }

    private static async Task ApplyScriptFileAsync(string connectionString, string filePath, CancellationToken ct)
    {
        var script = await File.ReadAllTextAsync(filePath, ct);

        // sqlcmd-style "GO" batch separators aren't understood by SqlCommand - split and run each batch in turn.
        var batches = System.Text.RegularExpressions.Regex.Split(script, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        foreach (var batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;

            await using var command = new SqlCommand(batch, connection) { CommandTimeout = 60 };
            await command.ExecuteNonQueryAsync(ct);
        }
    }
}
