using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace ForenSync_Console_App.UI.MainMenuOptions
{
    public static class AcquisitionHistory
    {
        public static void Show(string caseId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");
            AnsiConsole.MarkupLine("[blue]🔍 Main Menu > Acquisition History [/]");
            AnsiConsole.MarkupLine("────────────────────────────────────────────\n");
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select an option:[/]")
                    .PageSize(3)
                    .AddChoices(new[]
                    {
                        "📜 View Acquisition Logs",
                        "🗂️ Export Acquisition Reports",
                        "🔙 Back to Main Menu"
                    }));
            switch (choice)
            {
                case "📜 View Acquisition Logs":
                    AnsiConsole.MarkupLine("[yellow]→ Displaying acquisition logs...[/]");
                    break;

                case "🗂️ Export Acquisition Reports":
                    AnsiConsole.MarkupLine("[yellow]→ Exporting acquisition reports...[/]");
                    break;

                case "🔙 Back to Main Menu":
                    bool isNewCase = true; // for the Main Menu to show the summary if returning from Acquisition History
                    MainMenu.Show(caseId, isNewCase);
                    break;

                default:
                    AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                    break;
            }
        }
    }
}
