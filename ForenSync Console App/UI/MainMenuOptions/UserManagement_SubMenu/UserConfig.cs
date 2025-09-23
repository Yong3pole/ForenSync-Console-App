using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using ForenSync_Console_App.UI;


namespace ForenSync_Console_App.UI.MainMenuOptions.UserManagement_SubMenu
{


    public static class UserConfig
    {
        private enum SortField
        {
            UserId,
            LastName,
            FirstName,
            BadgeNum,
            Department,
            CreatedAt,
            CreatedBy,
            Active
        }

        public static void Render(string caseId, string currentUserId, bool isNewCase)
        {
            const int pageSize = 5;
            var allUsers = FetchUsers();
            var users = new List<UserRecord>(allUsers);

            SortField currentSort = SortField.CreatedAt;
            bool ascending = true;

            int totalPages = (int)Math.Ceiling(users.Count / (double)pageSize);
            int currentPage = 0;
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();
                AsciiTitle.Render("User Configuration");

                // Apply sort
                users = ascending
                    ? users.OrderBy(u => GetSortValue(u, currentSort)).ToList()
                    : users.OrderByDescending(u => GetSortValue(u, currentSort)).ToList();

                totalPages = (int)Math.Ceiling(users.Count / (double)pageSize);
                var pageUsers = users.Skip(currentPage * pageSize).Take(pageSize).ToList();

                if (pageUsers.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No users found.[/]");
                    Console.WriteLine("\nPress [Enter] to return...");
                    Console.ReadLine();
                    UserManagement.Show(caseId, currentUserId, isNewCase);
                    return;
                }

                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .Title("[bold underline]Registered Users[/]")
                    .AddColumn("User ID")
                    .AddColumn("Last Name")
                    .AddColumn("First Name")
                    .AddColumn("Badge Number")
                    .AddColumn("Department")
                    .AddColumn("Date Created")
                    .AddColumn("Registered By")
                    .AddColumn("Status");

                for (int i = 0; i < pageUsers.Count; i++)
                {
                    var user = pageUsers[i];
                    bool isSelected = i == selectedIndex;
                    string style = isSelected ? "[bold yellow]" : "";
                    string end = isSelected ? "[/]" : "";

                    table.AddRow(
                        $"{style}{user.UserId}{end}",
                        $"{style}{user.LastName}{end}",
                        $"{style}{user.FirstName}{end}",
                        $"{style}{(string.IsNullOrWhiteSpace(user.BadgeNum) ? "N/A" : user.BadgeNum)}{end}",
                        $"{style}{user.Department}{end}",
                        $"{style}{user.CreatedAt}{end}",
                        $"{style}{user.CreatedBy}{end}",
                        user.Active == 1 ? $"{style}Active{end}" : $"{style}[red]Inactive[/]{end}"
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"\n[grey]Page {currentPage + 1} of {totalPages}[/]");
                AnsiConsole.MarkupLine($"[grey]Sorted by: {currentSort} {(ascending ? "[green]↑[/]" : "[red]↓[/]")}[/]");
                AnsiConsole.MarkupLine("[grey]Use [[↑↓]] to select, [[←→]] to switch pages, [[, .]] to change sort, [[S]] to search, [[Enter]] for actions, [[Esc]] to exit.[/]");
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + pageUsers.Count) % pageUsers.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % pageUsers.Count;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (currentPage > 0)
                        {
                            currentPage--;
                            selectedIndex = 0;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (currentPage < totalPages - 1)
                        {
                            currentPage++;
                            selectedIndex = 0;
                        }
                        break;
                    case ConsoleKey.OemComma: // ,
                        currentSort = PrevSortField(currentSort);
                        ascending = true;
                        break;
                    case ConsoleKey.OemPeriod: // .
                        currentSort = NextSortField(currentSort);
                        ascending = true;
                        break;
                    case ConsoleKey.S:
                        Console.Clear();
                        AsciiTitle.Render("Search Users");
                        AnsiConsole.Markup("[cyan]Search by name, badge, or department[/]: ");
                        string query = Console.ReadLine()?.Trim().ToLower() ?? "";

                        users = allUsers.FindAll(u =>
                            u.UserId.ToLower().Contains(query) ||
                            u.FirstName.ToLower().Contains(query) ||
                            u.LastName.ToLower().Contains(query) ||
                            u.BadgeNum.ToLower().Contains(query) ||
                            u.Department.ToLower().Contains(query)
                        );

                        currentPage = 0;
                        selectedIndex = 0;
                        break;
                    case ConsoleKey.Enter:
                        var selectedUser = pageUsers[selectedIndex];
                        Console.Clear();
                        AsciiTitle.Render("User Actions");

                        AnsiConsole.MarkupLine($"[bold]User Selected:[/] {selectedUser.UserId} | {selectedUser.FirstName} {selectedUser.LastName} | {selectedUser.Department}");
                        AnsiConsole.MarkupLine($"[green]Current Time:[/] {DateTime.Now:dddd, dd MMMM yyyy, HH:mm:ss}");
                        AnsiConsole.MarkupLine("\n[[1]] Change Password\n[[2]] Toggle Active Status\n[[Esc]] Cancel");


                        var actionKey = Console.ReadKey(true).Key;
                        if (actionKey == ConsoleKey.Escape) break;

                        if (actionKey == ConsoleKey.D1)
                        {
                            Console.Clear();
                            AsciiTitle.Render("Change Password");
                            AnsiConsole.MarkupLine("[yellow]Are you sure you want to change this user's password?[/]");
                            Console.WriteLine("Press [1] = YES   Press [2] = NO");

                            var confirm = Console.ReadKey(true).Key;
                            if (confirm != ConsoleKey.D1) break;

                            Console.Write("Enter admin ID: ");
                            string adminId = Console.ReadLine()?.Trim();
                            Console.Write("Enter admin password: ");
                            string adminPass = Console.ReadLine()?.Trim();

                            if (!ValidateAdmin(adminId, adminPass))
                            {
                                AnsiConsole.MarkupLine("[red]❌ Admin override failed.[/]");
                                Console.ReadLine();
                                break;
                            }

                            string newPassword = GeneratePassword(selectedUser.UserId);
                            if (UpdatePassword(selectedUser.UserId, newPassword))
                            {
                                AnsiConsole.MarkupLine("[green]✅ Password updated successfully.[/]");
                                AnsiConsole.MarkupLine($"[blue]New Password:[/] {newPassword}");
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("[red]❌ Failed to update password.[/]");
                            }

                            Console.WriteLine("\nPress [Enter] to continue...");
                            Console.ReadLine();
                            break;
                        }

                        if (actionKey == ConsoleKey.D2)
                        {
                            Console.Clear();
                            AsciiTitle.Render("Toggle User Status");
                            string action = selectedUser.Active == 1 ? "deactivate" : "activate";
                            AnsiConsole.MarkupLine($"[yellow]Are you sure you want to {action} this user?[/]");
                            Console.WriteLine("Press [1] = YES   Press [2] = NO");

                            var confirm = Console.ReadKey(true).Key;
                            if (confirm != ConsoleKey.D1) break;

                            Console.Write("Enter admin ID: ");
                            string adminId = Console.ReadLine()?.Trim();
                            Console.Write("Enter admin password: ");
                            string adminPass = Console.ReadLine()?.Trim();

                            if (!ValidateAdmin(adminId, adminPass))
                            {
                                AnsiConsole.MarkupLine("[red]❌ Admin override failed.[/]");
                                Console.ReadLine();
                                break;
                            }

                            int newStatus = selectedUser.Active == 1 ? 0 : 1;
                            if (UpdateStatus(selectedUser.UserId, newStatus))
                            {
                                AnsiConsole.MarkupLine($"[green]✅ User status updated to {(newStatus == 1 ? "Active" : "Inactive")}.[/]");
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("[red]❌ Failed to update status.[/]");
                            }

                            Console.WriteLine("\nPress [Enter] to continue...");
                            Console.ReadLine();
                            break;
                        }
                        break;


                    case ConsoleKey.Escape:
                        UserManagement.Show(caseId, currentUserId, isNewCase);
                        return;
                }
            }
        }



        private static List<UserRecord> FetchUsers()
        {
            var users = new List<UserRecord>();
            try
            {
                string dbPath = Path.Combine(AppContext.BaseDirectory, "forensync.db");
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();
                

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT user_id, lastname, firstname, badge_num, department, created_at, created_by, active
                    FROM users_tbl
                    ORDER BY created_at DESC;
                ";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new UserRecord
                    {
                        UserId = reader.GetString(0),
                        LastName = reader.GetString(1),
                        FirstName = reader.GetString(2),
                        BadgeNum = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Department = reader.GetString(4),
                        CreatedAt = reader.GetString(5),
                        CreatedBy = reader.GetString(6),
                        Active = reader.GetInt32(7)
                    });
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Database error:[/] {ex.Message}");
            }

            return users;
        }

        private static SortField PrevSortField(SortField current)
        {
            var values = Enum.GetValues(typeof(SortField));
            int index = Array.IndexOf(values, current);
            index = (index - 1 + values.Length) % values.Length;
            return (SortField)values.GetValue(index);
        }

        private static SortField NextSortField(SortField current)
        {
            var values = Enum.GetValues(typeof(SortField));
            int index = Array.IndexOf(values, current);
            index = (index + 1) % values.Length;
            return (SortField)values.GetValue(index);
        }

        private static object GetSortValue(UserRecord u, SortField field) => field switch
        {
            SortField.UserId => u.UserId,
            SortField.LastName => u.LastName,
            SortField.FirstName => u.FirstName,
            SortField.BadgeNum => u.BadgeNum,
            SortField.Department => u.Department,
            SortField.CreatedAt => u.CreatedAt,
            SortField.CreatedBy => u.CreatedBy,
            SortField.Active => u.Active,
            _ => u.CreatedAt
        };

        private static bool ValidateAdmin(string adminId, string adminPassword)
        {
            try
            {
                string dbPath = @"C:\Users\kindr\source\repos\ForenSync-Console-App\forensync.db";
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT COUNT(*) FROM users_tbl
            WHERE user_id = $id AND password = $pw AND role = 'admin';
        ";
                command.Parameters.AddWithValue("$id", adminId);
                command.Parameters.AddWithValue("$pw", adminPassword);

                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
            catch { return false; }
        }

        private static bool UpdatePassword(string userId, string newPassword)
        {
            try
            {
                string dbPath = @"C:\Users\kindr\source\repos\ForenSync-Console-App\forensync.db";
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "UPDATE users_tbl SET password = $pw WHERE user_id = $id;";
                command.Parameters.AddWithValue("$pw", newPassword);
                command.Parameters.AddWithValue("$id", userId);

                return command.ExecuteNonQuery() == 1;
            }
            catch { return false; }
        }

        private static bool UpdateStatus(string userId, int newStatus)
        {
            try
            {
                string dbPath = @"C:\Users\kindr\source\repos\ForenSync-Console-App\forensync.db";
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "UPDATE users_tbl SET active = $status WHERE user_id = $id;";
                command.Parameters.AddWithValue("$status", newStatus);
                command.Parameters.AddWithValue("$id", userId);

                return command.ExecuteNonQuery() == 1;
            }
            catch { return false; }
        }

        private static string GeneratePassword(string userId)
        {
            var rand = new Random();
            string prefix;

            if (userId.StartsWith("AD"))
                prefix = rand.Next(2) == 0 ? "ADfsync" : "fsyncAD";
            else if (userId.StartsWith("OP"))
                prefix = rand.Next(2) == 0 ? "OPfsync" : "fsyncOP";
            else
                prefix = "fsync";

            string digits = rand.Next(10000, 99999).ToString();
            char letter = (char)rand.Next('A', 'Z' + 1);

            return $"{prefix}{digits}{letter}";
        }



        private class UserRecord
        {
            public string UserId { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string BadgeNum { get; set; }
            public string Department { get; set; }
            public string CreatedAt { get; set; }
            public string CreatedBy { get; set; }
            public int Active { get; set; }
        }
    }
}
