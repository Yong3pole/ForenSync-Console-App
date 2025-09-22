using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForenSync_Console_App.UI.MainMenuOptions.CaseOperations_SubMenu
{
    public static class CaseViewer
    {
        public static void Show(string caseId = null) // Optional param for future filtering
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            string dbPath = Path.Combine(AppContext.BaseDirectory, "forensync.db");
            string connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string query = @"
            SELECT 
                c.case_id,
                u.firstname || ' ' || u.lastname AS full_name,
                u.role,
                c.department,
                c.notes,
                c.date
            FROM case_logs c
            JOIN users_tbl u ON c.user_id = u.user_id
            ORDER BY c.date DESC";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                AnsiConsole.MarkupLine("[red]⚠️ No cases found in the database.[/]");
                return;
            }

            var table = new Table()
                .Title("[bold underline green]All Case Logs[/]")
                .Border(TableBorder.Rounded)
                .AddColumn("🆔 Case ID")
                .AddColumn("👤 User")
                .AddColumn("🛡️ Role")
                .AddColumn("🏢 Department")
                .AddColumn("📝 Notes")
                .AddColumn("📅 Date");

            while (reader.Read())
            {
                string id = reader.GetString(0);
                string user = reader.GetString(1);
                string role = reader.GetString(2);
                string department = reader.GetString(3);
                string notes = reader.GetString(4);
                string rawDate = reader.GetString(5);

                DateTime parsedDate;
                string formattedDate = DateTime.TryParse(rawDate, out parsedDate)
                    ? parsedDate.ToString("MMM dd, yyyy")
                    : rawDate;

                table.AddRow(id, user, role, department, notes, formattedDate);
            }

            AnsiConsole.Write(table);
        }
    }

}
