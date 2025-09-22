using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace ForenSync_Console_App.UI.MainMenuOptions
{
    public static class Tools
    {
        public static void Show(string caseId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            AnsiConsole.MarkupLine("[blue]🔍 Main Menu > Tools [/]");
            AnsiConsole.MarkupLine("────────────────────────────────────────────\n");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select a tool:[/]")
                    .PageSize(4)
                    .AddChoices(new[]
                    {
                        "📁 View running processes",
                        "🛜 List network connections",
                        "⏏️ List usb devices / removable media",
                        "🔙 Back to Main Menu"
                    }));

            switch (choice)
            {
                case "📁 view running processes":
                    AnsiConsole.MarkupLine("[yellow]→ Listing running processes...[/]");
                    break;

                case "💽 list network connections":
                    AnsiConsole.MarkupLine("[yellow]→ Listing network connections...[/]");
                    break;

                case "🧠 list usb devices / removable media":
                    AnsiConsole.MarkupLine("[yellow]→ Listing usb devices / removable media...[/]");
                    break;

                case "🔙 Back to Main Menu":
                    bool isNewCase = true; // for the Main Menu to show the summary if returning from Tools
                    MainMenu.Show(caseId, isNewCase);
                    break;

                default:
                    AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                    break;
            }
        }
    }
}
