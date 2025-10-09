﻿using ForenSync.Utils;
using Spectre.Console;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace ForenSync_Console_App.UI.MainMenuOptions.Tools_SubMenu
{
    public static class ViewProcesses
    {
        public static void Show(string currentCasePath, string userId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            var sb = new StringBuilder();
            var processes = Process.GetProcesses().OrderBy(p => p.ProcessName).ToList();

            // Bar Chart
            var topProcesses = processes.OrderByDescending(p => p.WorkingSet64).Take(10)
                .Select(p => new BarChartItem($"{p.ProcessName} ({p.Id})", p.WorkingSet64 / (1024 * 1024), Color.Green)).ToList();

            var chart = new BarChart().Width(80).Label("[bold underline green]Top Memory Consumers (MB)[/]").CenterLabel().AddItems(topProcesses);
            AnsiConsole.Write(chart);
            sb.AppendLine("Top Memory Consumers (MB)");
            foreach (var item in topProcesses)
                sb.AppendLine($"{item.Label} - {item.Value} MB");

            // Tree View
            var root = new Tree("[bold yellow]Running Processes[/]").Guide(TreeGuide.BoldLine);
            foreach (var proc in processes.Take(20))
            {
                var node = root.AddNode($"[green]{proc.ProcessName}[/] [grey](PID: {proc.Id})[/]");
                node.AddNode($"Memory: {(proc.WorkingSet64 / (1024 * 1024)):N0} MB");
                sb.AppendLine($"{proc.ProcessName} (PID: {proc.Id}) - Memory: {(proc.WorkingSet64 / (1024 * 1024)):N0} MB");
            }
            AnsiConsole.Write(root);

            // Table
            var table = new Table().RoundedBorder().AddColumn("PID").AddColumn("Name").AddColumn("Memory (MB)");
            foreach (var proc in processes.Take(20))
            {
                string memory = "N/A";
                try { memory = (proc.WorkingSet64 / (1024 * 1024)).ToString("N0"); } catch { }
                table.AddRow(proc.Id.ToString(), proc.ProcessName, memory);
                sb.AppendLine($"{proc.Id} | {proc.ProcessName} | {memory} MB");
            }

            AnsiConsole.Write(new Panel(table).Header("[bold green]Running Processes[/]").Border(BoxBorder.Double).Padding(1, 1).BorderStyle(new Style(Color.Blue)));

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

                EvidenceWriter.SaveToEvidence(currentCasePath, sb.ToString(), "running_process_snapshot");
                AuditLogger.Log(userId, AuditAction.ExportedSnapshot, "Saved: running_process_snapshot");
            }
        }
    }

    public static class ViewProcessesWMI
    {
        public static void Show(string currentCasePath, string userId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            var sb = new StringBuilder();
            var table = new Table().RoundedBorder().AddColumn("PID").AddColumn("Name").AddColumn("Command Line").AddColumn("Parent PID");

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
                    sb.AppendLine($"{pid} | {name} | {rawCmd ?? "N/A"} | Parent: {parent}");
                }

                AnsiConsole.Write(new Panel(table).Header("[bold green]WMI Process Snapshot[/]").Border(BoxBorder.Double).Padding(1, 1).BorderStyle(new Style(Color.Blue)));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Failed to query WMI: {ex.Message}[/]");
            }

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

                EvidenceWriter.SaveToEvidence(currentCasePath, sb.ToString(), "wmi_running_process_snapshot");
                AuditLogger.Log(userId, AuditAction.ExportedSnapshot, "Saved: wmi_running_process_snapshot");
            }
        }
    }
}
