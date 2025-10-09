using System;
using System.Linq;
using System.Text;
using Spectre.Console;
using System.Management;
using ForenSync.Utils;

namespace ForenSync_Console_App.UI.MainMenuOptions.DeviceInfo_SubMenu
{
    public static class ViewSystemInfo
    {
        public static void Show(string currentCasePath, string userId)
        {
            Console.Clear();
            AsciiTitle.Render("System Information");

            var table = new Table()
                .RoundedBorder()
                .Title("[bold yellow]System Overview[/]")
                .AddColumn("[blue]Property[/]")
                .AddColumn("[green]Value[/]");

            var sb = new StringBuilder();

            try
            {
                // Computer System
                foreach (var sys in new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem").Get())
                {
                    AddRow(table, sb, "Machine Name", sys["Name"]);
                    AddRow(table, sb, "Manufacturer", sys["Manufacturer"]);
                    AddRow(table, sb, "Model", sys["Model"]);
                    AddRow(table, sb, "System Type", sys["SystemType"]);
                    AddRow(table, sb, "Total Physical Memory", FormatBytes(sys["TotalPhysicalMemory"]?.ToString()));
                }

                // Operating System
                foreach (var os in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get())
                {
                    AddRow(table, sb, "OS Name", os["Caption"]);
                    AddRow(table, sb, "Version", os["Version"]);
                    AddRow(table, sb, "Architecture", os["OSArchitecture"]);
                    AddRow(table, sb, "Install Date", FormatDate(os["InstallDate"]?.ToString()));
                    AddRow(table, sb, "Last Boot Time", FormatDate(os["LastBootUpTime"]?.ToString()));
                    AddRow(table, sb, "System Uptime", FormatUptime(os["LastBootUpTime"]?.ToString()));
                }

                // Processor
                foreach (var cpu in new ManagementObjectSearcher("SELECT * FROM Win32_Processor").Get())
                {
                    AddRow(table, sb, "CPU Name", cpu["Name"]);
                    AddRow(table, sb, "Cores", cpu["NumberOfCores"]);
                    AddRow(table, sb, "Logical Processors", cpu["NumberOfLogicalProcessors"]);
                    AddRow(table, sb, "Architecture", cpu["Architecture"]);
                }

                // BIOS
                foreach (var bios in new ManagementObjectSearcher("SELECT * FROM Win32_BIOS").Get())
                {
                    var biosVersion = string.Join(", ", (string[])bios["BIOSVersion"] ?? new string[] { "N/A" });
                    AddRow(table, sb, "BIOS Version", biosVersion);
                    AddRow(table, sb, "BIOS Vendor", bios["Manufacturer"]);
                    AddRow(table, sb, "BIOS Release Date", FormatDate(bios["ReleaseDate"]?.ToString()));
                }

                // Motherboard
                foreach (var board in new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard").Get())
                {
                    AddRow(table, sb, "Motherboard Manufacturer", board["Manufacturer"]);
                    AddRow(table, sb, "Product", board["Product"]);
                }

                AnsiConsole.Write(new Panel(table)
                    .Header("[bold green]System Info Snapshot[/]")
                    .Border(BoxBorder.Double)
                    .Padding(1, 1)
                    .BorderStyle(new Style(Color.Blue)));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Failed to retrieve system info: {ex.Message}[/]");
            }

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

                EvidenceWriter.SaveToEvidence(currentCasePath, sb.ToString(), "system_info_snapshot");
                AuditLogger.Log(userId, AuditAction.ExportedSnapshot, "Saved: system_info_snapshot");
            }
        }

        private static void AddRow(Table table, StringBuilder sb, string label, object value)
        {
            string val = value?.ToString() ?? "N/A";
            table.AddRow(label, val);
            sb.AppendLine($"{label}: {val}");
        }

        private static string FormatBytes(string rawBytes)
        {
            if (ulong.TryParse(rawBytes, out ulong bytes))
            {
                double gb = bytes / (1024.0 * 1024 * 1024);
                return $"{gb:F2} GB";
            }
            return "N/A";
        }

        private static string FormatDate(string wmiDate)
        {
            if (string.IsNullOrWhiteSpace(wmiDate)) return "N/A";
            try
            {
                return ManagementDateTimeConverter.ToDateTime(wmiDate).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch { return "N/A"; }
        }

        private static string FormatUptime(string bootTime)
        {
            if (string.IsNullOrWhiteSpace(bootTime)) return "N/A";
            try
            {
                var boot = ManagementDateTimeConverter.ToDateTime(bootTime);
                var uptime = DateTime.Now - boot;
                return $"{(int)uptime.TotalHours} hrs {uptime.Minutes} mins";
            }
            catch { return "N/A"; }
        }
    }
}
