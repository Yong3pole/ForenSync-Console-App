using ForenSync.Utils;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ForenSync_Console_App.UI.MainMenuOptions.CaseOperations_SubMenu
{
    public static class CaptureMemory
    {
        public static void Run(string caseId, string userId, bool isNewCase)
        {
            Console.Clear();
            AsciiTitle.Render("Memory Capture");

            var confirmCapture = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Do you want to capture volatile memory for this device session?[/]")
                    .PageSize(3)
                    .AddChoices(new[]
                    {
            "✅ Yes, proceed with memory capture",
            "❌ No, return to Case Operations"
                    }));

            if (confirmCapture == "❌ No, return to Case Operations")
            {
                CaseOperations.Show(caseId, userId, isNewCase);
                return;
            }
            // Check for admin privileges
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                Console.WriteLine("❌ This operation requires Administrator privileges.");
                return;
            }

            string basePath = AppContext.BaseDirectory;
            string winpmemPath = Path.Combine(basePath, "winpmem.exe");

            if (!File.Exists(winpmemPath))
            {
                Console.WriteLine("❌ winpmem.exe not found in base directory.");
                return;
            }

            string outputDir = Path.Combine(basePath, "Cases", caseId, "Evidence", "Captured Memory");
            Directory.CreateDirectory(outputDir);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            string filename = $"memdump_{timestamp}.raw";
            string fullOutputPath = Path.Combine(outputDir, filename);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = winpmemPath,
                    Arguments = $"acquire --progress --nosparse \"{filename}\"",
                    WorkingDirectory = outputDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            string output = "", error = "";

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .Start("Capturing memory with WinPmem...", ctx =>
                {
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                });

            string hash = "";

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("yellow"))
                .Start("Computing SHA-256 hash...", ctx =>
                {
                    using var sha256 = SHA256.Create();
                    using var stream = File.OpenRead(fullOutputPath);
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                });

            string dbPath = Path.Combine(basePath, "forensync.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO acquisition_log (case_id, type, tool, output_path, hash, created_at)
                VALUES (@case_id, @type, @tool, @output_path, @hash, @created_at)";
            command.Parameters.AddWithValue("@case_id", caseId);
            command.Parameters.AddWithValue("@type", "memory capture");
            command.Parameters.AddWithValue("@tool", "ForenSync | WinPmem");
            command.Parameters.AddWithValue("@output_path", Path.Combine("Cases", caseId, "Evidence", "Captured Memory", filename));
            command.Parameters.AddWithValue("@hash", hash);
            command.Parameters.AddWithValue("@created_at", DateTime.Now);

            command.ExecuteNonQuery();
            //log to audit trail
            AuditLogger.Log(userId, AuditAction.MemCapture, $"Captured volatile memory for case: {caseId} — output: {filename}, hash: {hash}");

            AnsiConsole.MarkupLine("\n[green]✅ Memory capture completed successfully![/]");
            AnsiConsole.MarkupLine($"[grey]Saved to:[/] [bold]{fullOutputPath}[/]");
            AnsiConsole.MarkupLine($"[grey]SHA-256:[/] [blue]{hash}[/]");

            if (!string.IsNullOrWhiteSpace(error))
            {
                AnsiConsole.MarkupLine("[yellow]⚠️ WinPmem reported errors:[/]");
                Console.WriteLine(error);
            }

            AnsiConsole.MarkupLine("\n[bold]Press any key to return to Case Operations...[/]");
            Console.ReadKey(true);

            CaseOperations.Show(caseId, userId, isNewCase);
        }
    }
}
