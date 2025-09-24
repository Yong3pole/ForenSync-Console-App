using Microsoft.Data.Sqlite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Security.Cryptography;

namespace ForenSync_Console_App.UI.MainMenuOptions.CaseOperations_SubMenu
{
    public static class DriveImager
    {
        public static void Show(string caseId, string userId, bool isNewCase)
        {
            Console.Clear();
            AsciiTitle.Render("Drive Imaging");

            var selectedDrive = PromptDriveSelection();
            if (selectedDrive == null) return;

            var outputName = AnsiConsole.Ask<string>("💾 Enter output image filename (e.g., [grey]volume_image.dd[/]):");
            var casePath = Path.Combine("Cases", caseId, "Evidence", "Cloned Drive");
            Directory.CreateDirectory(casePath);
            var outputPath = Path.Combine(casePath, outputName);

            var rawPath = $"\\\\.\\{selectedDrive.Substring(0, 2)}"; // e.g., "C:"
            ImageVolume(rawPath, outputPath, caseId);
        }

        private static string PromptDriveSelection()
        {
            var drives = DriveInfo.GetDrives();
            var choices = new List<string>();

            foreach (var drive in drives)
            {
                if (!drive.IsReady) continue;
                string label = $"{drive.Name} ({drive.DriveFormat}) - {FormatBytes(drive.TotalFreeSpace)} free";
                choices.Add(label);
            }

            if (choices.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]❌ No ready drives found.[/]");
                return null;
            }

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a drive to image")
                    .PageSize(10)
                    .UseConverter(x => x)
                    .AddChoices(choices));
        }

        private static void ImageVolume(string volumePath, string outputPath, string caseId)
        {
            if (!IsAdmin())
            {
                AnsiConsole.MarkupLine("[red]🔒 Imaging requires administrator privileges.[/]");
                return;
            }

            AnsiConsole.MarkupLine($"🛠 Imaging [yellow]{volumePath}[/] to [green]{outputPath}[/]...");

            string hash = "";
            try
            {
                using var src = new FileStream(volumePath, FileMode.Open, FileAccess.Read);
                using var dst = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                using var hasher = SHA256.Create();

                byte[] buffer = new byte[1024 * 1024]; // 1MB
                long totalBytes = 0;

                var progress = AnsiConsole.Progress()
                    .AutoClear(true)
                    .Columns(new ProgressColumn[]
                    {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new ElapsedTimeColumn()
                    });

                progress.Start(ctx =>
                {
                    var task = ctx.AddTask("Imaging volume", autoStart: true);

                    while (true)
                    {
                        int bytesRead = src.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        dst.Write(buffer, 0, bytesRead);
                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                        totalBytes += bytesRead;
                        task.Increment((bytesRead / 1024f / 1024f));
                    }

                    hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    hash = BitConverter.ToString(hasher.Hash).Replace("-", "").ToLowerInvariant();
                });

                LogToDatabase(caseId, "drive", ".NET FileStream", outputPath, hash);

                AnsiConsole.MarkupLine("\n[green]✅ Imaging complete.[/]");
                AnsiConsole.Write(new Table()
                    .Title("📄 Imaging Summary")
                    .AddColumns("Field", "Value")
                    .AddRow("Case ID", caseId)
                    .AddRow("Type", "drive")
                    .AddRow("Tool", ".NET FileStream")
                    .AddRow("Output Path", outputPath)
                    .AddRow("SHA256 Hash", hash)
                    .AddRow("Created At", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Border(TableBorder.Rounded));

                AnsiConsole.MarkupLine("\n[grey]Press any key to return to Case Operations menu...[/]");
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"⚠️ [red]Error:[/] {ex.Message}");
            }
        }

        private static void LogToDatabase(string caseId, string type, string tool, string outputPath, string hash)
        {
            string basePath = AppContext.BaseDirectory;
            string dbPath = Path.Combine(basePath, "forensync.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            string sql = @"
        INSERT INTO acquisition_log (case_id, type, tool, output_path, hash, created_at)
        VALUES (@case_id, @type, @tool, @output_path, @hash, @created_at);";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@case_id", caseId);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@tool", tool);
            cmd.Parameters.AddWithValue("@output_path", outputPath);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.ExecuteNonQuery();
        }

        private static string FormatBytes(long bytes)
        {
            double gb = bytes / 1024d / 1024d / 1024d;
            return $"{gb:N2} GB";
        }

        private static bool IsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}