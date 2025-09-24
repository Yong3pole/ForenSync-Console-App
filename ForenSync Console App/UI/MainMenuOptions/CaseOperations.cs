using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForenSync_Console_App.UI.MainMenuOptions
{
    public static class CaseOperations
    {
   
        public static void Show(string caseId, string userId, bool isNewCase) // isNewCase indicates if this is a new case or not
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            if (isNewCase) // Show summary only for new cases 
            {
                Console.WriteLine("🆕 Starting New Case\n");

                string dbPath = Path.Combine(AppContext.BaseDirectory, "forensync.db");
                string connectionString = $"Data Source={dbPath}";

                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                // Query to get case details along with user info
                string query = @"
                    SELECT 
                        c.case_id,
                        u.firstname || ' ' || u.lastname AS full_name,
                        u.role,
                        c.date
                    FROM case_logs c
                    JOIN users_tbl u ON c.user_id = u.user_id
                    WHERE c.case_id = @caseId
                    LIMIT 1";

                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@caseId", caseId);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string id = reader.GetString(0);
                    string user = reader.GetString(1);
                    string role = reader.GetString(2);
                    string rawDate = reader.GetString(3);

                    DateTime createdDate;
                    string formattedDate = DateTime.TryParse(rawDate, out createdDate)
                        ? createdDate.ToString("MMM dd, yyyy")
                        : rawDate;

                    // Display the case summary
                    Console.WriteLine("📋 Case Summary:");
                    Console.WriteLine("───────────────────────────────────────────────────────────────────────────");
                    Console.WriteLine($"{id} | {user} ({role}) | Created: {formattedDate}");
                    Console.WriteLine("───────────────────────────────────────────────────────────────────────────\n");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️ Case not found in database.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("📂 Welcome Back\n");
            }

            
            AnsiConsole.MarkupLine("[cyan]📂 Main Menu > Case Operations [/]");
            AnsiConsole.MarkupLine("────────────────────────────────────────────");

            if (isNewCase) // Show full menu for new cases
            {
                var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select an operation:[/]")
                    .PageSize(5)
                    //.HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(new[]
                    {
                        "📁 View Cases",
                        "💽 View Mounted Drives",
                        "🧠 Capture Memory",
                        "🧲 Image/Clone Drive or Partition",
                        "🔙 Back to Main Menu"
                    }));

                switch (choice)
                {
                    case "📁 View Cases":
                        AnsiConsole.MarkupLine($"[yellow]→ Viewing cases for [bold]{caseId}[/][/]");
                        CaseOperations_SubMenu.CaseViewer.Show(caseId);
                        break;

                    case "💽 View Mounted Drives":
                        AnsiConsole.MarkupLine("[yellow]→ Scanning mounted drives...[/]");
                        DeviceInfo_SubMenu.ViewDiskLayout.Show(); // Same code for now, refer to DeviceInfo 
                        Show(caseId, userId, isNewCase);
                        break;

                    case "🧠 Capture Memory":
                        AnsiConsole.MarkupLine("[yellow]→ Launching memory capture...[/]");
                        CaseOperations_SubMenu.CaptureMemory.Run(caseId, userId, isNewCase);
                        break;

                    case "🧲 Image/Clone Drive or Partition":
                        AnsiConsole.MarkupLine("[yellow]→ Starting imaging workflow...[/]");
                        CaseOperations_SubMenu.DriveImager.Show(caseId, userId);
                        break;

                    case "🔙 Back to Main Menu":
                        //bool isNewCase = true; // for the Main Menu to still show the summary if returning from a new case
                        MainMenu.Show(caseId, userId, isNewCase);
                        break;

                    default:
                        AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                        break;
                }
            }

            else
            {
                // Show the same available submenu but disable imaging and memory capture for skipping creating a case
                var choices = new List<string>
                {
                    "📁 View Cases",
                    "💽 View Mounted Drives",
                    "🧠 Capture Memory (disabled)",
                    "🧲 Image/Clone Drive or Partition (disabled)",
                    "🔙 Back to Main Menu"
                };

                var prompt = new SelectionPrompt<string>()
                    .Title("[green]Select an operation:[/]")
                    .PageSize(5)
                    //.HighlightStyle(new Style(foreground: Color.Yellow))
                    .UseConverter(choice =>
                    {
                        return choice.Contains("(disabled)")
                            ? $"[grey]{choice}[/]"
                            : choice;
                    })
                    .AddChoices(choices); // ✅ This must be present

                var selected = AnsiConsole.Prompt(prompt);

                // Handle selection
                if (selected.Contains("(disabled)"))
                {
                    AnsiConsole.MarkupLine("[red]That option is disabled for existing cases.[/]");
                    CaseOperations.Show(caseId, userId, false); // Re-render menu
                    return;
                }

                switch (selected)
                {
                    case "📁 View Cases":
                        AnsiConsole.MarkupLine($"[yellow]→ Viewing cases for [bold]{caseId}[/][/]");
                        CaseOperations_SubMenu.CaseViewer.Show(caseId);
                        break;

                    case "💽 View Mounted Drives":
                        AnsiConsole.MarkupLine("[yellow]→ Scanning mounted drives...[/]");
                        DeviceInfo_SubMenu.ViewDiskLayout.Show(); // Same code for now, refer to DeviceInfo
                        Show(caseId, userId, isNewCase);
                        break;

                    case "🔙 Back to Main Menu":
                        MainMenu.Show(caseId, userId, false);
                        break;
                }
            }

        }
    }
}


