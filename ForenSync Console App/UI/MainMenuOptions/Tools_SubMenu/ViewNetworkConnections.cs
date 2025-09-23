using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;
using System.Net;
using System.Net.NetworkInformation;

namespace ForenSync_Console_App.UI.MainMenuOptions.Tools_SubMenu
{
    public static class ViewNetworkConnections
    {
        public static void Show()
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

            foreach (var conn in connections)
            {
                string local = $"{conn.LocalEndPoint.Address}:{conn.LocalEndPoint.Port}";
                string remote = conn.RemoteEndPoint.Address.Equals(IPAddress.Any)
                    ? "N/A"
                    : $"{conn.RemoteEndPoint.Address}:{conn.RemoteEndPoint.Port}";
                string state = conn.State.ToString();

                table.AddRow(local, remote, state);
            }

            AnsiConsole.Write(new Panel(table)
                .Header("[bold green]TCP Connection Snapshot[/]")
                .Border(BoxBorder.Double)
                .Padding(1, 1)
                .BorderStyle(new Style(Color.Blue)));

            AnsiConsole.MarkupLine("\n[grey]Press any key to return to Tools menu...[/]");
            Console.ReadKey(true);
        }
    }
}