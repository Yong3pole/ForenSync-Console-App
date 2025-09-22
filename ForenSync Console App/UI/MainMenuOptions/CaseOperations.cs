using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace ForenSync_Console_App.UI.MainMenuOptions
{
    public static class CaseOperations
    {
        public static void Show(string caseId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            AnsiConsole.MarkupLine("[blue]🔍 Main Menu > Case Operations [/]");
            AnsiConsole.MarkupLine("────────────────────────────────────────────\n");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select an operation:[/]")
                    .PageSize(5)
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
                    // TODO: Call CaseViewer.Show(caseId);
                    break;

                case "💽 View Mounted Drives":
                    AnsiConsole.MarkupLine("[yellow]→ Scanning mounted drives...[/]");
                    // TODO: Call DriveScanner.Show(caseId);
                    break;

                case "🧠 Capture Memory":
                    AnsiConsole.MarkupLine("[yellow]→ Launching memory capture...[/]");
                    // TODO: Call MemoryCapture.Run(caseId);
                    break;

                case "🧲 Image/Clone Drive or Partition":
                    AnsiConsole.MarkupLine("[yellow]→ Starting imaging workflow...[/]");
                    // TODO: Call DriveImager.Start(caseId);
                    break;

                case "🔙 Back to Main Menu":
                    bool isNewCase = true; // for the Main Menu to still show the summary if returning from a new case
                    MainMenu.Show(caseId, isNewCase);
                    break;

                default:
                    AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                    break;
            }
        }
    }
}


