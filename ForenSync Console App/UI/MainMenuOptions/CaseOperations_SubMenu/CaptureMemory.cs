using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using System.Diagnostics;

namespace ForenSync_Console_App.UI.MainMenuOptions.CaseOperations_SubMenu
{
    public static class CaptureMemory
    {
        public static void Run(string caseId, string userId)
        {
            Console.Clear();
            AsciiTitle.Render("Memory Capture");

            string basePath = AppContext.BaseDirectory;
            string outputDir = Path.Combine(basePath, "Cases", caseId, "Evidence", "Captured Memory");

            Directory.CreateDirectory(outputDir);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            string outputFile = Path.Combine(outputDir, $"memdump_{timestamp}.raw");

            string winpmemPath = Path.Combine(basePath, "winpmem.exe");

            if (!File.Exists(winpmemPath))
            {
                AnsiConsole.MarkupLine($"[red]❌ winpmem.exe not found in base directory.[/]");
                return;
            }

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .Start("Capturing memory...", ctx =>
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = winpmemPath,
                            Arguments = $"-o \"{outputFile}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    process.WaitForExit();
                });

            AnsiConsole.MarkupLine($"[green]✅ Memory captured successfully![/]");
            AnsiConsole.MarkupLine($"[grey]Saved to:[/] [bold]{outputFile}[/]");
        }
    }
}

