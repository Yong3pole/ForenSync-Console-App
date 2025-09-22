using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace ForenSync_Console_App.UI.MainMenuOptions
{
    public static class Help
    {
        public static void Show(string caseId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");
            AnsiConsole.MarkupLine("[blue]🔍 Main Menu > Help [/]");
            AnsiConsole.MarkupLine("────────────────────────────────────────────\n");
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select an option:[/]")
                    .PageSize(3)
                    .AddChoices(new[]
                    {
                        "📖 View Documentation",
                        "💬 Contact Support",
                        "🔙 Back to Main Menu"
                    }));

            switch (choice)
            {
                case "📖 View Documentation":
                    AnsiConsole.MarkupLine("[yellow]→ Opening documentation...[/]");
                    break;
                case "💬 Contact Support":
                    AnsiConsole.MarkupLine("[yellow]→ Contacting support...[/]");
                    break;
                case "🔙 Back to Main Menu":
                    bool isNewCase = true; // for the Main Menu to show the summary if returning from Help
                    MainMenu.Show(caseId, isNewCase);
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                    break;
            }
        }
    }
}
