using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

namespace ForenSync_Console_App.Data
{
    public static class UserAuthenticator
    {
        public static bool ValidateUser(string userId, string password)
        {
            string dbPath = @"C:\Users\kindr\source\repos\ForenSync-Console-App\forensync.db";

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM users_tbl
                WHERE user_id = $userId AND password = $password;
            ";

            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$password", password);

            long count = (long)command.ExecuteScalar();
            return count == 1;
        }
    }
}

