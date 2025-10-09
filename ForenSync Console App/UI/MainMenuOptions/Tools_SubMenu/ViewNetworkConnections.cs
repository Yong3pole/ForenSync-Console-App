using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Spectre.Console;
using ForenSync.Utils;

namespace ForenSync_Console_App.UI.MainMenuOptions.Tools_SubMenu
{
    public static class ViewNetworkConnections
    {
        public static void Show(string currentCasePath, string userId)
        {
            Console.Clear();
            AsciiTitle.Render("Network Connections");

            TcpConnectionInformation[] connections;

            try
            {
                connections = IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpConnections();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Failed to retrieve network connections: {ex.Message}[/]");
                return;
            }

            // Bar Chart: Connection states
            var stateGroups = connections
                .GroupBy(c => c.State)
                .Select(g => new BarChartItem(g.Key.ToString(), g.Count(), Color.Yellow))
                .ToList();

            AnsiConsole.Write(new BarChart()
                .Width(60)
                .Label("[green bold underline]TCP Connection States[/]")
                .CenterLabel()
                .AddItems(stateGroups));

            // Tree View: Top remote IPs
            var ipGroups = connections
                .Where(c => !c.RemoteEndPoint.Address.Equals(IPAddress.Any))
                .GroupBy(c => c.RemoteEndPoint.Address.ToString())
                .OrderByDescending(g => g.Count())
                .Take(5);

            var root = new Tree("[bold yellow]Top Remote IPs[/]").Guide(TreeGuide.BoldLine);

            foreach (var group in ipGroups)
            {
                var node = root.AddNode($"[green]{group.Key}[/] [grey]({group.Count()} connections)[/]");
                foreach (var conn in group.Take(3))
                {
                    node.AddNode($"Local: {conn.LocalEndPoint} → State: {conn.State}");
                }
            }

            AnsiConsole.Write(root);

            // Table: Full connection list
            var table = new Table()
                .RoundedBorder()
                .AddColumn("Local")
                .AddColumn("Remote")
                .AddColumn("State");

            var sb = new StringBuilder();

            foreach (var conn in connections)
            {
                string local = $"{conn.LocalEndPoint.Address}:{conn.LocalEndPoint.Port}";
                string remote = conn.RemoteEndPoint.Address.Equals(IPAddress.Any)
                    ? "N/A"
                    : $"{conn.RemoteEndPoint.Address}:{conn.RemoteEndPoint.Port}";
                string state = conn.State.ToString();

                table.AddRow(local, remote, state);
                sb.AppendLine($"{local} | {remote} | {state}");
            }

            AnsiConsole.Write(new Panel(table)
                .Header("[bold green]TCP Connection Snapshot[/]")
                .Border(BoxBorder.Double)
                .Padding(1, 1)
                .BorderStyle(new Style(Color.Blue)));

            // Snapshot logic
            AnsiConsole.MarkupLine("\n[green][[S]][/]: Save snapshot   [green][[Esc]][/]: Return to Tools");

            var key = EvidenceWriter.TryReadKey();

            if (key?.Key == ConsoleKey.S)
            {
                if (string.IsNullOrWhiteSpace(currentCasePath))
                {
                    AnsiConsole.MarkupLine("\n[red]⚠️ No active case detected. This session is not linked to any case.[/]");
                    AnsiConsole.MarkupLine("[grey]Press [bold]Enter[/] to return to Tools.[/]");
                    Console.ReadKey(true);
                    Tools.Show(null, userId, false);
                    return;
                }

                EvidenceWriter.SaveToEvidence(currentCasePath, sb.ToString(), "network_connections");
                AuditLogger.Log(userId, AuditAction.ExportedSnapshot, "Saved: network_connections");
            }

        }
    }
}
