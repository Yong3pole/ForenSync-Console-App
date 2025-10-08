using ForenSync_Console_App.CaseManagement;
using ForenSync_Console_App.Data;
using Spectre.Console;
using System;

namespace ForenSync_Console_App.UI
{
    public static class LoginPage
    {
        public static void PromptCredentials()
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Welcome to ForenSync. Please select an operation to begin.[/]")
                    .PageSize(3)
                    .AddChoices("🔐 Log in", "🚪 Exit"));

            if (action == "🚪 Exit")
            {
                AnsiConsole.MarkupLine("\n[red]👋 Exiting ForenSync. Stay safe out there.[/]");
                Environment.Exit(0);
            }

            bool showError = false;

            while (true)
            {
                Console.Clear();
                AsciiTitle.Render("ForenSync");
                AnsiConsole.MarkupLine("[bold blue]🔐 Please log in to continue[/]\n");

                if (showError)
                {
                    AnsiConsole.MarkupLine("[red]❌ Invalid credentials. Please try again.[/]\n");
                }

                string userId = AnsiConsole.Ask<string>("👤 [white]Enter User ID[/]:").Trim();

                if (userId.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("\n[red]👋 Exiting ForenSync. Stay safe out there.[/]");
                    Environment.Exit(0);
                }

                string password = AnsiConsole.Prompt(
                    new TextPrompt<string>("🔑 [white]Enter Password[/]:")
                        .PromptStyle("red")
                        .Secret());

                AnsiConsole.Status()
                    .SpinnerStyle(Style.Parse("yellow"))
                    .Start("🔄 Authenticating...", ctx =>
                    {
                        Thread.Sleep(3000);
                    });

                bool isAuthenticated = UserAuthenticator.ValidateUser(userId, password);

                if (isAuthenticated)
                {
                    ShowSessionPrompt(userId);
                    return;
                }
                else
                {
                    showError = true;
                }
            }
        }

        private static void ShowSessionPrompt(string userId)
        {
            var options = new[]
            {
                "🆕 Initiate new case or session",
                "📂 Load existing case",
                "⏭️ Skip setup and open to main menu"
            };

            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();
                AsciiTitle.Render("ForenSync");
                AnsiConsole.MarkupLine("[green]✅ Login successful![/]\n");
                AnsiConsole.MarkupLine("────────────────────────────────────────────");
                AnsiConsole.MarkupLine("[green]Use ↑↓ to navigate, [[Enter]] to select, [[Esc]] to return to login.[/]\n");
                AnsiConsole.MarkupLine("[bold blue]🧭 Session Options: Choose to proceed:[/]\n");

                for (int i = 0; i < options.Length; i++)
                {
                    string prefix = i == selectedIndex ? "[bold blue]> " : "  ";
                    string suffix = i == selectedIndex ? "[/]" : "";
                    AnsiConsole.MarkupLine($"{prefix}{options[i]}{suffix}");
                }

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                        break;

                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % options.Length;
                        break;

                    case ConsoleKey.Enter:
                        switch (options[selectedIndex])
                        {
                            case "🆕 Initiate new case or session":
                                CaseSession.StartNewCase(userId);
                                return;

                            case "📂 Load existing case":
                                string selectedCaseId = CaseSession.SelectExistingCase(userId);
                                if (!string.IsNullOrEmpty(selectedCaseId))
                                {
                                    MainMenu.Show(selectedCaseId, userId, false);
                                }
                                break;

                            case "⏭️ Skip setup and open to main menu":
                                MainMenu.Show(null, userId, false);
                                return;
                        }
                        break;

                    case ConsoleKey.Escape:
                        Console.Clear();
                        PromptCredentials();
                        return;
                }
            }
        }
    }
}
