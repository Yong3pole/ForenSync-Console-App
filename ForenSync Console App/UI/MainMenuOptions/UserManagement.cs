using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace ForenSync_Console_App.UI.MainMenuOptions
{
    public static class UserManagement
    {
        public static void Show(string caseId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");
            AnsiConsole.MarkupLine("[blue]🔍 Main Menu > User Management [/]");
            AnsiConsole.MarkupLine("────────────────────────────────────────────\n");
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select an option:[/]")
                    .PageSize(3)
                    .AddChoices(new[]
                    {
                        "👤 Add User",
                        "🛠️ Manage User Roles",
                        "🔙 Back to Main Menu"
                    }));

            switch (choice)
            {
                case "👤 Add User":
                    AnsiConsole.MarkupLine("[yellow]→ Adding a new user...[/]");
                    break;
                case "🛠️ Manage User Roles":
                    AnsiConsole.MarkupLine("[yellow]→ Managing user roles...[/]");
                    break;
                case "🔙 Back to Main Menu":
                    bool isNewCase = true; // for the Main Menu to show the summary if returning from User Management
                    MainMenu.Show(caseId, isNewCase);
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                    break;

            }
        }
    }
}
