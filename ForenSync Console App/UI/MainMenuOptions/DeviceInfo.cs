using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace ForenSync_Console_App.UI.MainMenuOptions
{
    public static class DeviceInfo
    {
        public static void Show(string caseId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            AnsiConsole.MarkupLine("[blue]🔍 Main Menu > Device Info [/]");
            AnsiConsole.MarkupLine("────────────────────────────────────────────\n");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select an option:[/]")
                    .PageSize(5)
                    .AddChoices(new[]
                    {
                        "🖥️ View system info",
                        "💽 View Disk Layout",
                        "👤 List user accounts",
                        "ℹ️ View installed applications",
                        "🔙 Back to Main Menu"
                    }));

            switch (choice)
            {
                case "🖥️ View system info":
                    AnsiConsole.MarkupLine("[yellow]→ Gathering system info...[/]");
                    break;

                case "💽 View Disk Layout":
                    AnsiConsole.MarkupLine("[yellow]→ Retrieving disk layout...[/]");
                    break;

                case "👤 List user accounts":
                    AnsiConsole.MarkupLine("[yellow]→ Listing user accounts...[/]");
                    break;

                case "ℹ️ View installed applications":
                    AnsiConsole.MarkupLine("[yellow]→ Fetching installed applications...[/]");
                    break;

                case "🔙 Back to Main Menu":
                    bool isNewCase = true; // for the Main Menu to show the summary if returning from Device Info
                    MainMenu.Show(caseId, isNewCase);
                    break;

                default:
                    AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                    break;
            }
        }
    }
}
