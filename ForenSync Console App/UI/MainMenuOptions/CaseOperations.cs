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

            AnsiConsole.MarkupLine("[blue]📂 Main Menu > Case Operations [/]");
            AnsiConsole.MarkupLine("────────────────────────────────────────────\n");

            if (isNewCase)
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
                        //bool isNewCase = true; // for the Main Menu to still show the summary if returning from a new case
                        MainMenu.Show(caseId, isNewCase);
                        break;

                    default:
                        AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                        break;
                }
            }

            else
            {
                // Show the same available submenu but disable imaging and memory capture for existing cases, like greyed out and cannot be selected
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
                    CaseOperations.Show(caseId, false); // Re-render menu
                    return;
                }

                switch (selected)
                {
                    case "📁 View Cases":
                        AnsiConsole.MarkupLine($"[yellow]→ Viewing cases for [bold]{caseId}[/][/]");
                        // TODO: CaseViewer.Show(caseId);
                        break;

                    case "💽 View Mounted Drives":
                        AnsiConsole.MarkupLine("[yellow]→ Scanning mounted drives...[/]");
                        // TODO: DriveScanner.Show(caseId);
                        break;

                    case "🔙 Back to Main Menu":
                        MainMenu.Show(caseId, false);
                        break;
                }
            }

        }
    }
}


