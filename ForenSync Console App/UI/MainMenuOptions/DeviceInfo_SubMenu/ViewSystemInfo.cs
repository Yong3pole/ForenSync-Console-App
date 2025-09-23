using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;
using System.Management;

namespace ForenSync_Console_App.UI.MainMenuOptions.DeviceInfo_SubMenu
{
    public static class ViewSystemInfo
    {
        public static void Show()
        {
            Console.Clear();
            AsciiTitle.Render("System Information");

            var table = new Table()
                .RoundedBorder()
                .Title("[bold yellow]System Overview[/]")
                .AddColumn("[blue]Property[/]")
                .AddColumn("[green]Value[/]");

            try
            {
                // Computer System
                foreach (var sys in new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem").Get())
                {
                    table.AddRow("Machine Name", sys["Name"]?.ToString() ?? "N/A");
                    table.AddRow("Manufacturer", sys["Manufacturer"]?.ToString() ?? "N/A");
                    table.AddRow("Model", sys["Model"]?.ToString() ?? "N/A");
                    table.AddRow("System Type", sys["SystemType"]?.ToString() ?? "N/A");
                    table.AddRow("Total Physical Memory", FormatBytes(sys["TotalPhysicalMemory"]?.ToString()));
                }

                // Operating System
                foreach (var os in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get())
                {
                    table.AddRow("OS Name", os["Caption"]?.ToString() ?? "N/A");
                    table.AddRow("Version", os["Version"]?.ToString() ?? "N/A");
                    table.AddRow("Architecture", os["OSArchitecture"]?.ToString() ?? "N/A");
                    table.AddRow("Install Date", FormatDate(os["InstallDate"]?.ToString()));
                    table.AddRow("Last Boot Time", FormatDate(os["LastBootUpTime"]?.ToString()));
                    table.AddRow("System Uptime", FormatUptime(os["LastBootUpTime"]?.ToString()));
                }

                // Processor
                foreach (var cpu in new ManagementObjectSearcher("SELECT * FROM Win32_Processor").Get())
                {
                    table.AddRow("CPU Name", cpu["Name"]?.ToString() ?? "N/A");
                    table.AddRow("Cores", cpu["NumberOfCores"]?.ToString() ?? "N/A");
                    table.AddRow("Logical Processors", cpu["NumberOfLogicalProcessors"]?.ToString() ?? "N/A");
                    table.AddRow("Architecture", cpu["Architecture"]?.ToString() ?? "N/A");
                }

                // BIOS
                foreach (var bios in new ManagementObjectSearcher("SELECT * FROM Win32_BIOS").Get())
                {
                    table.AddRow("BIOS Version", string.Join(", ", (string[])bios["BIOSVersion"] ?? new string[] { "N/A" }));
                    table.AddRow("BIOS Vendor", bios["Manufacturer"]?.ToString() ?? "N/A");
                    table.AddRow("BIOS Release Date", FormatDate(bios["ReleaseDate"]?.ToString()));
                }

                // Motherboard
                foreach (var board in new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard").Get())
                {
                    table.AddRow("Motherboard Manufacturer", board["Manufacturer"]?.ToString() ?? "N/A");
                    table.AddRow("Product", board["Product"]?.ToString() ?? "N/A");
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

            AnsiConsole.MarkupLine("\n[grey]Press any key to return to Device Info menu...[/]");
            Console.ReadKey(true);
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