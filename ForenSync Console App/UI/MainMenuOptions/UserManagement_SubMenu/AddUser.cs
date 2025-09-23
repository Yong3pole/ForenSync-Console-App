using System;
using System.Text;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using ForenSync_Console_App.UI;

namespace ForenSync_Console_App.UI.MainMenuOptions.UserManagement_SubMenu
{
    public static class AddUser
    {
        private class FormField
        {
            public string Label { get; set; }
            public string Value { get; set; } = "";
            public bool IsOptional { get; set; } = false;
        }

        public static void Render(string caseId, string currentUserId, bool isNewCase)
        {
            var fields = new[]
            {
                new FormField { Label = "First Name" },
                new FormField { Label = "Last Name" },
                new FormField { Label = "Badge Number (optional)", IsOptional = true },
                new FormField { Label = "Department" }
            };

            string[] roles = { "ADMIN", "OPERATOR" };
            int roleIndex = 0;
            int fieldIndex = 0;

            while (true)
            {
                Console.Clear();
                AsciiTitle.Render("Register New User");
                AnsiConsole.MarkupLine("[bold]Please fill the required fields. Press [[F10]] to confirm.[/]\n");

                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    string highlight = i == fieldIndex ? "[underline]" : "";
                    string end = i == fieldIndex ? "[/]" : "";
                    AnsiConsole.MarkupLine($"{highlight}{field.Label,-25}:{end} {field.Value}");
                }

                string roleHighlight = fieldIndex == fields.Length ? "[underline bold yellow]" : "[bold yellow]";
                AnsiConsole.MarkupLine($"\nRole: {roleHighlight}{roles[roleIndex]}[/]");
                AnsiConsole.MarkupLine("Use [yellow]←[/] and [yellow]→[/] to toggle roles.");
                AnsiConsole.MarkupLine("Press [black on grey] Esc [/] to cancel/return.");

                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    UserManagement.Show(caseId, currentUserId, isNewCase);
                    return;
                }

                if (key.Key == ConsoleKey.F10)
                {
                    break;
                }

                if (key.Key == ConsoleKey.UpArrow)
                {
                    fieldIndex = (fieldIndex - 1 + fields.Length + 1) % (fields.Length + 1);
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    fieldIndex = (fieldIndex + 1) % (fields.Length + 1);
                }
                else if (fieldIndex < fields.Length)
                {
                    if (key.Key == ConsoleKey.Backspace && fields[fieldIndex].Value.Length > 0)
                    {
                        fields[fieldIndex].Value = fields[fieldIndex].Value[..^1];
                    }
                    else if (key.KeyChar >= 32 && key.KeyChar <= 126)
                    {
                        fields[fieldIndex].Value += key.KeyChar;
                    }
                }
                else if (fieldIndex == fields.Length)
                {
                    if (key.Key == ConsoleKey.LeftArrow)
                        roleIndex = (roleIndex - 1 + roles.Length) % roles.Length;
                    else if (key.Key == ConsoleKey.RightArrow)
                        roleIndex = (roleIndex + 1) % roles.Length;
                }
            }

            string firstName = fields[0].Value.Trim();
            string lastName = fields[1].Value.Trim();
            string badgeNum = fields[2].Value.Trim();
            string department = fields[3].Value.Trim();
            string role = roles[roleIndex];
            string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string createdBy = currentUserId;
            int active = 1;

            string userId = GenerateUserId(role);
            string password = GeneratePassword(role);

            Console.Clear();
            AsciiTitle.Render("Confirm Registration");

            AnsiConsole.MarkupLine("[bold]Please confirm the following details:[/]\n");
            AnsiConsole.MarkupLine($"[green]Name      :[/] {firstName} {lastName}");
            AnsiConsole.MarkupLine($"[green]Badge     :[/] {(string.IsNullOrWhiteSpace(badgeNum) ? "None" : badgeNum)}");
            AnsiConsole.MarkupLine($"[green]Department:[/] {department}");
            AnsiConsole.MarkupLine($"[green]Role      :[/] [bold yellow]{role}[/]");
            AnsiConsole.MarkupLine($"[green]Created By:[/] {createdBy}");
            AnsiConsole.MarkupLine($"[green]Created At:[/] {createdAt}");
            AnsiConsole.MarkupLine($"[green]Active    :[/] {active}");

            Console.WriteLine($"\nYou are registering this user as [bold yellow]{role}[/]. Are you sure you wish to continue?");
            Console.WriteLine("Press [1] = YES   Press [2] = NO");

            var confirm = Console.ReadKey(intercept: true);
            if (confirm.Key == ConsoleKey.D2)
            {
                Render(caseId, currentUserId, isNewCase);
                return;
            }

            if (InsertUser(userId, lastName, firstName, badgeNum, department, role, password, createdAt, createdBy, active))
            {
                Console.Clear();
                AsciiTitle.Render("User Registered");
                AnsiConsole.MarkupLine($"[green]✅ User successfully added.[/]");
                AnsiConsole.MarkupLine($"[blue]Generated User ID:[/] {userId}");
                AnsiConsole.MarkupLine($"[blue]Generated Password:[/] {password}");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]❌ Failed to add user. Please check input or database.[/]");
            }

            Console.WriteLine("\nPress [Enter] to continue...");
            Console.ReadLine();
            UserManagement.Show(caseId, currentUserId, isNewCase);
        }

        private static string GenerateUserId(string role)
        {
            var rand = new Random();
            string prefix = role.ToUpper() == "ADMIN" ? "AD" : "OP";
            string digits = rand.Next(10000, 99999).ToString();
            char letter = (char)rand.Next('A', 'Z' + 1);
            return $"{prefix}{digits}{letter}";
        }

        private static string GeneratePassword(string role)
        {
            var rand = new Random();
            string[] adminPrefixes = { "ADfsync", "fsyncAD" };
            string[] operatorPrefixes = { "OPfsync", "fsyncOP" };

            string prefix = role.ToUpper() == "ADMIN"
                ? adminPrefixes[rand.Next(adminPrefixes.Length)]
                : operatorPrefixes[rand.Next(operatorPrefixes.Length)];

            string digits = rand.Next(10000, 99999).ToString();
            char letter = (char)rand.Next('A', 'Z' + 1);
            return $"{prefix}{digits}{letter}";
        }

        private static bool InsertUser(string userId, string lastName, string firstName, string badgeNum, string department,
                                       string role, string password, string createdAt, string createdBy, int active)
        {
            try
            {
                string dbPath = @"C:\Users\kindr\source\repos\ForenSync-Console-App\forensync.db";
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO users_tbl (user_id, lastname, firstname, badge_num, department, role, password, created_at, created_by, active)
                    VALUES ($userId, $lastName, $firstName, $badgeNum, $department, $role, $password, $createdAt, $createdBy, $active);
                ";

                command.Parameters.AddWithValue("$userId", userId);
                command.Parameters.AddWithValue("$lastName", lastName);
                command.Parameters.AddWithValue("$firstName", firstName);
                command.Parameters.AddWithValue("$badgeNum", string.IsNullOrWhiteSpace(badgeNum) ? DBNull.Value : badgeNum);
                command.Parameters.AddWithValue("$department", department);
                command.Parameters.AddWithValue("$role", role.ToLower());
                command.Parameters.AddWithValue("$password", password);
                command.Parameters.AddWithValue("$createdAt", createdAt);
                command.Parameters.AddWithValue("$createdBy", createdBy);
                command.Parameters.AddWithValue("$active", active);

                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[DB ERROR] {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
    }
}
