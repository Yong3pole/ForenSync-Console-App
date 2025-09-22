using ForenSync_Console_App.UI.MainMenuOptions;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ForenSync_Console_App.UI
{
    public static class MainMenu
    {
        public static void Show(string caseId, bool isNewCase)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            if (isNewCase)
            {
                Console.WriteLine("🆕 Starting New Case\n");

                string summaryPath = Path.Combine(AppContext.BaseDirectory, "Cases", caseId, "summary.txt");

                if (File.Exists(summaryPath))
                {
                    Console.WriteLine("📋 Case Summary:");
                    Console.WriteLine("────────────────────────────────────────────");
                    Console.WriteLine(File.ReadAllText(summaryPath));
                    Console.WriteLine("────────────────────────────────────────────\n");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️ Summary file not found.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("📂 Welcome Back\n");
            }

            // Menu logic goes here...

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("📂 Main Menu");
            Console.WriteLine("────────────────────────────────────────────");
            Console.ResetColor();

            var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Select a forensic mode:[/]")
                .PageSize(5)
                .AddChoices(new[]
                {
                    "🧭 Case Operations",
                    "🛠️ Tools",
                    "💻 Device Info",
                    "📜 Acquisition History & Chain of Custody",
                    "👤 User Management",
                    "❓ Help",
                    "🚪 Exit"
                }));

            switch (choice)
            {
                case "🧭 Case Operations":
                    CaseOperations.Show(caseId, isNewCase);
                    break;

                case "🛠️ Tools":
                    Tools.Show(caseId, isNewCase);
                    break;

                case "💻 Device Info":
                    DeviceInfo.Show(caseId, isNewCase);
                    break;

                case "📜 Acquisition History & Chain of Custody":
                    AcquisitionHistory.Show(caseId, isNewCase);
                    break;

                case "👤 User Management":
                    UserManagement.Show(caseId, isNewCase);
                    break;

                case "❓ Help":
                    Help.Show(caseId, isNewCase);
                    break;

                // Add other cases later
                default:
                    AnsiConsole.MarkupLine($"[red]→ Option not yet implemented: {choice}[/]");
                    break;
            }

        }
    }

}
