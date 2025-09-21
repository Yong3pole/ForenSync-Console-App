using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForenSync_Console_App.UI;

using System;

namespace ForenSync_Console_App.CaseManagement
{
    public static class CaseSession
    {
        public static void StartNewCase(string userId)
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            string caseId = GenerateCaseId();
            Console.WriteLine($"🆕 Starting New Case: {caseId}\n");

            Console.Write("Enter Jurisdiction/Department: ");
            string jurisdiction = Console.ReadLine();

            Console.Write("Enter Notes (optional): ");
            string notes = Console.ReadLine();

            Console.WriteLine("\n────────────────────────────────────────────");
            Console.WriteLine("📋 Case Summary:");
            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine($"Case ID         : {caseId}");
            Console.WriteLine($"Jurisdiction    : {jurisdiction}");
            Console.WriteLine($"Notes           : {(string.IsNullOrWhiteSpace(notes) ? "None" : notes)}");
            Console.WriteLine($"User            : John Dela Cruz");
            Console.WriteLine($"Role            : Administrator");
            Console.WriteLine("────────────────────────────────────────────\n");

            CreateCaseFolder(caseId, jurisdiction, notes, userId);

            Console.WriteLine("✅ Case folder created. Proceeding to main menu...\n");
        }

        private static string GenerateCaseId()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"CASE_{timestamp}";
        }

        private static void CreateCaseFolder(string caseId, string jurisdiction, string notes, string userId)
        {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "Cases");
            string casePath = Path.Combine(basePath, caseId);
            string evidencePath = Path.Combine(casePath, "Evidence");

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
    }

}
