using ForenSync.Utils;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;

namespace ForenSync_Console_App.UI.MainMenuOptions
{
    public static class ChangePassword
    {
        private class FormField
        {
            public string Label { get; set; }
            public string Value { get; set; } = "";
            public bool IsSecret { get; set; } = false;
        }

        private static bool IsPasswordValid(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasUpper = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsLetterOrDigit(c)) hasSpecial = true;
            }

            return hasUpper && hasDigit && hasSpecial;
        }


        public static void Render(string userId)
        {
            while (true) // 🔁 retry loop
            {
                var fields = new[]
                {
            new FormField { Label = "Enter Old Password", Value = "", IsSecret = true },
            new FormField { Label = "Enter New Password", Value = "", IsSecret = true },
            new FormField { Label = "Confirm New Password", Value = "", IsSecret = true }
        };

                int fieldIndex = 0;

                // 🔽 Input loop
                while (true)
                {
                    Console.Clear();
                    AsciiTitle.Render("Change Password");
                    AnsiConsole.MarkupLine("[green]Use ↑↓ to navigate, [[F10]] to confirm, [[Esc]] to cancel and return.[/]\n");
                    AnsiConsole.MarkupLine("[white]🔐 Please enter your credentials below.[/]\n");

                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        string highlight = i == fieldIndex ? "[blue bold]" : "";
                        string end = i == fieldIndex ? "[/]" : "";
                        string displayValue = field.IsSecret ? new string('*', field.Value.Length) : field.Value;
                        AnsiConsole.MarkupLine($"{highlight}{field.Label,-25}:{end} {displayValue}");
                    }

                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.UpArrow)
                        fieldIndex = (fieldIndex - 1 + fields.Length) % fields.Length;
                    else if (key.Key == ConsoleKey.DownArrow)
                        fieldIndex = (fieldIndex + 1) % fields.Length;
                    else if (key.Key == ConsoleKey.F10)
                        break;
                    else if (key.Key == ConsoleKey.Backspace && fields[fieldIndex].Value.Length > 0)
                        fields[fieldIndex].Value = fields[fieldIndex].Value[..^1];
                    else if (key.KeyChar >= 32 && key.KeyChar <= 126)
                        fields[fieldIndex].Value += key.KeyChar;
                    else if (key.Key == ConsoleKey.Escape)
                    {
                        MainMenu.Show(null, userId, false);
                        return;
                    }
                }

                string oldPassword = fields[0].Value.Trim();
                string newPassword = fields[1].Value.Trim();
                string confirmPassword = fields[2].Value.Trim();

                Console.Clear();
                AsciiTitle.Render("Confirm Password Change");

                AnsiConsole.MarkupLine("[bold]Please confirm the following:[/]\n");
                AnsiConsole.MarkupLine($"[green]Old Password     :[/] {"*".PadLeft(oldPassword.Length, '*')}");
                AnsiConsole.MarkupLine($"[green]New Password     :[/] {"*".PadLeft(newPassword.Length, '*')}");
                AnsiConsole.MarkupLine($"[green]Confirm Password :[/] {"*".PadLeft(confirmPassword.Length, '*')}");
                AnsiConsole.MarkupLine("\n[bold yellow]Are you sure you want to change your password?[/]");
                AnsiConsole.MarkupLine("[grey]Press [[1]] = YES   [[2]] = NO[/]");

                var confirm = Console.ReadKey(true).Key;
                if (confirm == ConsoleKey.D2)
                    continue; // 🔁 restart form

                if (newPassword != confirmPassword)
                {
                    AnsiConsole.MarkupLine("[red]❌ New passwords do not match.[/]");
                    AnsiConsole.MarkupLine("[grey]Press [[Enter]] to try again...[/]");
                    Console.ReadLine();
                    continue; // 🔁 restart form
                }

                if (!IsPasswordValid(newPassword))
                {
                    AnsiConsole.MarkupLine("[red]❌ Password must be 8+ chars with uppercase, number, and special character.[/]");
                    AnsiConsole.MarkupLine("[grey]Press [[Enter]] to try again...[/]");
                    Console.ReadLine();
                    continue; // 🔁 restart form
                }

                string dbPath = Path.Combine(AppContext.BaseDirectory, "forensync.db");
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                var verifyCommand = connection.CreateCommand();
                verifyCommand.CommandText = "SELECT COUNT(*) FROM users_tbl WHERE user_id = $id AND password = $current;";
                verifyCommand.Parameters.AddWithValue("$id", userId);
                verifyCommand.Parameters.AddWithValue("$current", oldPassword);

                long match = (long)verifyCommand.ExecuteScalar();
                if (match != 1)
                {
                    AnsiConsole.MarkupLine("[red]❌ Old password is incorrect.[/]");
                    AnsiConsole.MarkupLine("[grey]Press [[Enter]] to try again...[/]");
                    Console.ReadLine();
                    continue; // 🔁 restart form
                }

                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "UPDATE users_tbl SET password = $new WHERE user_id = $id;";
                updateCommand.Parameters.AddWithValue("$new", newPassword);
                updateCommand.Parameters.AddWithValue("$id", userId);
                updateCommand.ExecuteNonQuery();
                //save to audit log
                AuditLogger.Log(userId, AuditAction.ChangePassword, "User changed their password successfully.");

                AnsiConsole.MarkupLine("[green]✅ Password successfully updated.[/]");
                Console.ReadLine();
                return; // ✅ success, exit
            }
        }



    }

}
