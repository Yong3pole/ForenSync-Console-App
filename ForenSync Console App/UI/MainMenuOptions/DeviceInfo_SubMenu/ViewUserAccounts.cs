using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;
using System.Management;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ForenSync_Console_App.UI.MainMenuOptions.DeviceInfo_SubMenu
{
    public static class ViewUserAccounts
    {
        public static void Show()
        {
            Console.Clear();
            AsciiTitle.Render("User Accounts");

            var table = new Table()
                .RoundedBorder()
                .Title("[bold yellow]Local User Accounts[/]")
                .AddColumn("[blue]Username[/]")
                .AddColumn("[green]Full Name[/]")
                .AddColumn("[yellow]Status[/]")
                .AddColumn("[cyan]Password Expiry[/]")
                .AddColumn("[magenta]Last Login[/]")
                .AddColumn("[grey]Groups[/]");

            int enabledCount = 0;
            int disabledCount = 0;

            var groupMap = BuildGroupMembership();

            try
            {
                foreach (var user in new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount WHERE LocalAccount = TRUE").Get())
                {
                    string username = user["Name"]?.ToString() ?? "N/A";
                    string fullName = user["FullName"]?.ToString() ?? "N/A";
                    bool disabled = (bool)(user["Disabled"] ?? false);

                    string status = disabled ? "[red]❌ Disabled[/]" : "[green]✅ Enabled[/]";
                    if (disabled) disabledCount++; else enabledCount++;

                    var netUserOutput = RunCommand($"net user \"{username}\"");
                    string lastLogin = ParseNetUserField(netUserOutput, "Last logon");
                    string passwordExpires = ParseNetUserField(netUserOutput, "Password expires");

                    string groups = groupMap.ContainsKey(username)
                        ? string.Join(", ", groupMap[username])
                        : "None";

                    table.AddRow(username, fullName, status, passwordExpires, lastLogin, groups);
                }

                AnsiConsole.Write(new Panel(table)
                    .Header("[bold green]Account Summary[/]")
                    .Border(BoxBorder.Double)
                    .Padding(1, 1)
                    .BorderStyle(new Style(Color.Blue)));

                AnsiConsole.MarkupLine("\n[bold underline green]Account Status Breakdown[/]\n");

                AnsiConsole.Write(new BreakdownChart()
                    .Width(60)
                    .ShowPercentage()
                    .UseValueFormatter(v => $"{v:N0} accounts")
                    .AddItem("Enabled", enabledCount, Color.Green)
                    .AddItem("Disabled", disabledCount, Color.Red));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Failed to retrieve user accounts: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key to return to Device Info menu...[/]");
            Console.ReadKey(true);
        }

        private static Dictionary<string, List<string>> BuildGroupMembership()
        {
            var map = new Dictionary<string, List<string>>();

            foreach (var rel in new ManagementObjectSearcher("SELECT * FROM Win32_GroupUser").Get())
            {
                string part = rel["PartComponent"]?.ToString() ?? "";
                string group = rel["GroupComponent"]?.ToString() ?? "";

                string user = ExtractName(part);
                string groupName = ExtractName(group);

                if (!map.ContainsKey(user))
                    map[user] = new List<string>();

                map[user].Add(groupName);
            }

            return map;
        }

        private static string ExtractName(string wmiPath)
        {
            var match = Regex.Match(wmiPath, @"Name=""([^""]+)""");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        private static string RunCommand(string command)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                return process?.StandardOutput.ReadToEnd() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static string ParseNetUserField(string output, string field)
        {
            var match = Regex.Match(output, $@"{Regex.Escape(field)}\s+(.+)");
            return match.Success ? match.Groups[1].Value.Trim() : "N/A";
        }
    }
}