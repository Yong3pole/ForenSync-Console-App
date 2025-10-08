using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ForenSync.Utils
{
    public enum AuditAction
    {
        Image,
        MemCapture,
        ExportedSnapshot,
        AddUser,
        ChangePassword,
        AccessCase,
        CreateCase,
        Login,
        LoginFailed,
        ViewCase,
        ViewUserConfig,
        ViewHistory,
        ContactSupport,
        ViewDocs
    }

    public static class AuditLogger
    {
        public static void Log(string userId, AuditAction action, string context = null)
        {
            try
            {
                string dbPath = Path.Combine(AppContext.BaseDirectory, "forensync.db");
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO audit_trail (user_id, action, created_at, context)
                    VALUES ($userId, $action, $createdAt, $context);";

                command.Parameters.AddWithValue("$userId", userId);
                command.Parameters.AddWithValue("$action", action.ToString());
                command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
                command.Parameters.AddWithValue("$context", context ?? "");

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Optional: log to fallback file or console for diagnostics
                Console.Error.WriteLine($"[AuditLogger] Failed to log action: {ex.Message}");
            }
        }
    }
}
