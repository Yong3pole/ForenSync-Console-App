using ForenSync_Console_App.CaseManagement;
using ForenSync_Console_App.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;


namespace ForenSync_Console_App.UI
{
    public static class LoginPage
    {
        public static void PromptCredentials()
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            // Initial action prompt
            var action = AnsiConsole.Prompt( 
                new SelectionPrompt<string>() // ✅ Spectre.Console
                    .Title("[green]Welcome to ForenSync. What would you like to do?[/]")
                    .PageSize(3)
                    .AddChoices(new[]
                    {
                "🔐 Log in",
                "🚪 Exit"
                    }));

            if (action == "🚪 Exit")
            {
                AnsiConsole.MarkupLine("\n[red]👋 Exiting ForenSync. Stay safe out there.[/]");
                Environment.Exit(0);
            }

            // Proceed to login loop
            bool showError = false;


            // Proceed to login loop
            while (true)
            {
                Console.Clear();
                AsciiTitle.Render("ForenSync");

                AnsiConsole.MarkupLine("[bold blue]🔐 Please log in to continue[/]\n");

                if (showError)
                {
                    AnsiConsole.MarkupLine("[red]❌ Invalid credentials. Please try again.[/]\n");
                }

                string userId = AnsiConsole.Ask<string>("👤 [green]Enter User ID[/]:").Trim();

                if (userId.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("\n[red]👋 Exiting ForenSync. Stay safe out there.[/]");
                    Environment.Exit(0);
                }

                string password = AnsiConsole.Prompt(
                    new TextPrompt<string>("🔑 [green]Enter Password[/]:")
                        .PromptStyle("red")
                        .Secret());

                // Synchronous
                AnsiConsole.Status()
                    .SpinnerStyle(Style.Parse("yellow"))
                    .Start("🔄 Authenticating...", ctx =>
                    {
                        Thread.Sleep(3000);
                    });

                if (UserAuthenticator.ValidateUser(userId, password))
                {
                    ShowSessionPrompt(userId);
                    break;
                }
                else
                {
                    showError = true; // Show error on next loop
                }
            }

        }


        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);

            return password;
        }

        // Show session options after successful login
        private static void ShowSessionPrompt(string userId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");
            AnsiConsole.MarkupLine("[green]✅ Login successful![/]\n");
            AnsiConsole.MarkupLine("────────────────────────────────────────────");
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold blue]🧭 Session Options: Choose to proceed:[/]")
                    .PageSize(4)
                    .AddChoices(new[]
                    {
                "🆕 Start a new case or session",
                "📂 Continue an ongoing case",
                "⏭️ Skip case setup and proceed to main menu"
                    }));

            switch (choice)
            {
                case "🆕 Start a new case or session":
                    CaseManagement.CaseSession.StartNewCase(userId);
                    break;

                case "📂 Continue an ongoing case":
                    // CaseManagement.CaseSession.ContinueCase(userId); // Uncomment when ready
                    AnsiConsole.MarkupLine("[yellow]⚠️ Continue case is temporarily disabled for testing.[/]");
                    ShowSessionPrompt(userId); // Loop back
                    return;

                case "⏭️ Skip case setup and proceed to main menu":
                    MainMenu.Show(null, false);
                    break;
            }

            AnsiConsole.MarkupLine($"\n[blue]You selected:[/] [bold]{choice}[/] — proceeding...\n");
        }
    }
}
