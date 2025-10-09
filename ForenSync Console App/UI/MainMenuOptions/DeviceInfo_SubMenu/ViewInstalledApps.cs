using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using ForenSync.Utils;

namespace ForenSync_Console_App.UI.MainMenuOptions.DeviceInfo_SubMenu
{
    public static class ViewInstalledApps
    {
        public static void Show(string currentCasePath, string userId)
        {
            Console.Clear();
            AsciiTitle.Render("Installed Applications");

            var apps = new List<ManagementObject>();
            var sb = new StringBuilder();
            int appCount = 0;

            AuditLogger.Log(userId, AuditAction.ViewUserConfig, "Viewed: installed_applications");

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .Start("Fetching installed applications...", ctx =>
                {
                    try
                    {
                        foreach (ManagementBaseObject baseObj in new ManagementObjectSearcher("SELECT * FROM Win32_Product").Get())
                        {
                            var app = baseObj as ManagementObject;
                            if (app != null)
                            {
                                apps.Add(app);
                                appCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]❌ Failed to retrieve installed applications: {ex.Message}[/]");
                        AuditLogger.Log(userId, AuditAction.ViewUserConfig, $"Failed to retrieve installed applications: {ex.Message}");
                    }
                });

            if (apps.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No installed applications found.[/]");
                return;
            }

            // BreakdownChart: Top vendors
            var vendorCounts = new Dictionary<string, int>();
            foreach (var app in apps)
            {
                string vendor = app["Vendor"]?.ToString() ?? "Unknown";
                if (!vendorCounts.ContainsKey(vendor))
                    vendorCounts[vendor] = 0;
                vendorCounts[vendor]++;
            }

            var breakdownItems = vendorCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(6)
                .Select((kvp, i) => new BreakdownChartItem(kvp.Key, kvp.Value, GetColor(i)))
                .ToList();

            AnsiConsole.MarkupLine("\n[bold underline green]Top Application Vendors[/]\n");
            AnsiConsole.Write(new BreakdownChart()
                .Width(60)
                .ShowPercentage()
                .UseValueFormatter(v => $"{v:N0} apps")
                .AddItems(breakdownItems));

            // Table: Installed applications
            var table = new Table()
                .RoundedBorder()
                .Title($"[bold yellow]Installed Applications ({appCount})[/]")
                .AddColumn("[blue]Name[/]")
                .AddColumn("[green]Version[/]")
                .AddColumn("[cyan]Vendor[/]")
                .AddColumn("[magenta]Install Date[/]");

            foreach (var app in apps)
            {
                string name = app["Name"]?.ToString() ?? "N/A";
                string version = app["Version"]?.ToString() ?? "N/A";
                string vendor = app["Vendor"]?.ToString() ?? "N/A";
                string installDate = FormatDate(app["InstallDate"]?.ToString());

                table.AddRow(name, version, vendor, installDate);
                sb.AppendLine($"{name} | {version} | {vendor} | Installed: {installDate}");
            }

            AnsiConsole.Write(new Panel(table)
                .Header("[bold green]Application Summary[/]")
                .Border(BoxBorder.Double)
                .Padding(1, 1)
                .BorderStyle(new Style(Color.Blue)));

            AnsiConsole.MarkupLine("\n[green][[S]][/]: Save snapshot   [green][[Esc]][/]: Return to Device Info");

            var key = EvidenceWriter.TryReadKey();
            if (key?.Key == ConsoleKey.S)
            {
                if (string.IsNullOrWhiteSpace(currentCasePath))
                {
                    AnsiConsole.MarkupLine("\n[red]⚠️ No active case detected. This session is not linked to any case.[/]");
                    AnsiConsole.MarkupLine("[grey]Press [bold]Enter[/] to return to Device Info.[/]");
                    Console.ReadKey(true);
                    DeviceInfo.Show(null, userId, false);
                    return;
                }

                EvidenceWriter.SaveToEvidence(currentCasePath, sb.ToString(), "user_accounts_snapshot");
                AuditLogger.Log(userId, AuditAction.ExportedSnapshot, "Saved: user_accounts_snapshot");
            }
        }

        private static string FormatDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw.Length < 8) return "N/A";
            try
            {
                return DateTime.ParseExact(raw.Substring(0, 8), "yyyyMMdd", null).ToString("yyyy-MM-dd");
            }
            catch
            {
                return "N/A";
            }
        }

        private static Color GetColor(int index)
        {
            var palette = new[] { Color.Green, Color.Blue, Color.Yellow, Color.Magenta1, Color.Cyan1, Color.Orange1 };
            return palette[index % palette.Length];
        }
    }
}
