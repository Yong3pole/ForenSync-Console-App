using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForenSync_Console_App.UI;
using ForenSync_Console_App.Data;
using Microsoft.Data.Sqlite;

namespace ForenSync_Console_App.CaseManagement
{
    public static class CaseSession
    {
        // Starts a new case session
        public static void StartNewCase(string userId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            Console.WriteLine("🆕 Starting New Case\n");

            Console.Write("Enter Jurisdiction/Department: ");
            string jurisdiction = Console.ReadLine();

            Console.Write("Enter Notes (optional): ");
            string notes = Console.ReadLine();

            // Generate Case ID after inputs to match timestamp
            string caseId = GenerateCaseId();

            Console.WriteLine("\n────────────────────────────────────────────");
            Console.WriteLine("📋 Case Summary:");
            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine($"Case ID         : {caseId}");
            Console.WriteLine($"Jurisdiction    : {jurisdiction}");
            Console.WriteLine($"Notes           : {(string.IsNullOrWhiteSpace(notes) ? "None" : notes)}");
            Console.WriteLine($"User            : John Dela Cruz");
            Console.WriteLine($"Role            : Administrator");
            Console.WriteLine("────────────────────────────────────────────\n");

            string casePath = CreateCaseFolder(caseId, jurisdiction, notes);
            SaveToDatabase(caseId, jurisdiction, notes, userId, casePath);

            Console.WriteLine("✅ Case folder created. Proceeding to main menu...\n");
            System.Threading.Thread.Sleep(3000); // Pause for 3 seconds
            MainMenu.Show(caseId, true);
        }

        // Generates a unique case ID based on timestamp
        private static string GenerateCaseId()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"CASE_{timestamp}";
        }

        // Creates case folder and summary file
        private static string CreateCaseFolder(string caseId, string jurisdiction, string notes)
        {
            string basePath = Path.Combine(AppContext.BaseDirectory, "Cases");
            string casePath = Path.Combine(basePath, caseId);
            string evidencePath = Path.Combine(casePath, "Evidence");

            try
            {
                Directory.CreateDirectory(evidencePath);

                string summaryPath = Path.Combine(casePath, "summary.txt");
                string summaryContent = $@"
Case ID       : {caseId}
Jurisdiction  : {jurisdiction}
Notes         : {(string.IsNullOrWhiteSpace(notes) ? "None" : notes)}
User          : John Dela Cruz
Role          : Administrator
Created At    : {DateTime.Now}
";

                File.WriteAllText(summaryPath, summaryContent.Trim());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error creating case folder or summary: {ex.Message}");
                Console.ResetColor();
            }

            return casePath;

        }

        // Saves case details to database
        private static void SaveToDatabase(string caseId, string jurisdiction, string notes, string userId, string casePath)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "forensync.db");

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        INSERT INTO case_logs (case_id, jurisdiction, user_id, notes, date, case_path)
        VALUES ($id, $jurisdiction, $userId, $notes, $date, $path);
    ";

            command.Parameters.AddWithValue("$id", caseId);
            command.Parameters.AddWithValue("$jurisdiction", jurisdiction);
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$notes", string.IsNullOrWhiteSpace(notes) ? "None" : notes);
            command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("$path", casePath);

            command.ExecuteNonQuery();
        }

    }
}
