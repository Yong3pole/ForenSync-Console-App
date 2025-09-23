using Spectre.Console;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace ForenSync_Console_App.UI.MainMenuOptions.Tools_SubMenu
{
    public static class ViewProcesses
    {
        public static void Show()
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            var processes = Process.GetProcesses()
                                   .OrderBy(p => p.ProcessName)
                                   .ToList();

            // Bar Chart: Top 10 memory consumers
            var topProcesses = processes
                .OrderByDescending(p => p.WorkingSet64)
                .Take(10)
                .Select(p => new BarChartItem(
                    $"{p.ProcessName} ({p.Id})",
                    p.WorkingSet64 / (1024 * 1024),
                    Color.Green))
                .ToList();

            AnsiConsole.Write(new BarChart()
                .Width(80)
                .Label("[bold underline green]Top Memory Consumers (MB)[/]")
                .CenterLabel()
                .AddItems(topProcesses));

            // Tree View: Process summary
            var root = new Tree("[bold yellow]Running Processes[/]")
                .Guide(TreeGuide.BoldLine);

            foreach (var proc in processes.Take(20))
            {
                var node = root.AddNode($"[green]{proc.ProcessName}[/] [grey](PID: {proc.Id})[/]");
                node.AddNode($"Memory: {(proc.WorkingSet64 / (1024 * 1024)):N0} MB");
            }

            AnsiConsole.Write(root);

            // Table: Detailed process list
            var table = new Table()
                .RoundedBorder()
                .AddColumn("PID")
                .AddColumn("Name")
                .AddColumn("Memory (MB)");

            foreach (var proc in processes.Take(20))
            {
                string memory = "N/A";
                try
                {
                    memory = (proc.WorkingSet64 / (1024 * 1024)).ToString("N0");
                }
                catch { }

                table.AddRow(proc.Id.ToString(), proc.ProcessName, memory);
            }

            AnsiConsole.Write(new Panel(table)
                .Header("[bold green]Running Processes[/]")
                .Border(BoxBorder.Double)
                .Padding(1, 1)
                .BorderStyle(new Style(Color.Blue)));

            AnsiConsole.MarkupLine("\n[grey]Press any key to return to Tools menu...[/]");
            Console.ReadKey(true);
        }
        
    }
    public static class ViewProcessesWMI
    {
        public static void Show()
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            var table = new Table()
                .RoundedBorder()
                .AddColumn("PID")
                .AddColumn("Name")
                .AddColumn("Command Line")
                .AddColumn("Parent PID");

            try
            {
                var searcher = new ManagementObjectSearcher("SELECT ProcessId, Name, CommandLine, ParentProcessId FROM Win32_Process");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string pid = obj["ProcessId"]?.ToString() ?? "N/A";
                    string name = obj["Name"]?.ToString() ?? "N/A";
                    string rawCmd = obj["CommandLine"]?.ToString();
                    string cmd = string.IsNullOrWhiteSpace(rawCmd) ? "N/A" : Markup.Escape(rawCmd);
                    string parent = obj["ParentProcessId"]?.ToString() ?? "N/A";

                    table.AddRow(pid, name, cmd, parent);
                }

                AnsiConsole.Write(new Panel(table)
                    .Header("[bold green]WMI Process Snapshot[/]")
                    .Border(BoxBorder.Double)
                    .Padding(1, 1)
                    .BorderStyle(new Style(Color.Blue)));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Failed to query WMI: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key to return to Tools menu...[/]");
            Console.ReadKey(true);
        }
    }
}