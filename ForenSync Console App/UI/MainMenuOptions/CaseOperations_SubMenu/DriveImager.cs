using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;

namespace ForenSync_Console_App.UI.MainMenuOptions.CaseOperations_SubMenu
{
    public static class DriveImager
    {
        public static void Show(string caseId, string userId)
        {
            Console.Clear();
            AsciiTitle.Render("Drive Imaging");

            AnsiConsole.MarkupLine($"[bold yellow]Case ID:[/] {caseId}");
            AnsiConsole.MarkupLine($"[bold yellow]User ID:[/] {userId}\n");

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green"))
                .Start("Detecting available partitions...", ctx =>
                {
                    // Simulate partition detection
                    System.Threading.Thread.Sleep(1000);
                });

            var partitions = GetMockPartitions(); // Replace with actual detection later

            if (partitions.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]❌ No partitions found.[/]");
                return;
            }

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Select a partition to image[/]")
                    .PageSize(10)
                    .AddChoices(partitions));

            AnsiConsole.MarkupLine($"\n[bold yellow]🧲 Selected Partition:[/] [bold]{selected}[/]");
            AnsiConsole.MarkupLine("\n[grey]Press any key to return to Case Operations menu...[/]");
            Console.ReadKey(true);
        }

        private static List<string> GetMockPartitions()
        {
            // Replace with actual logic later (e.g., WMI or DriveInfo)
            return new List<string>
            {
                "C:\\ [System]",
                "D:\\ [Data]",
                "E:\\ [Backup]",
                "F:\\ [External USB]",
            };
        }
    }
}
