using ForenSync_Console_App.CaseManagement;
using ForenSync_Console_App.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForenSync_Console_App.UI
{
    public static class LoginPage
    {
        public static void PromptCredentials()
        {
            Console.Clear();
            AsciiTitle.Render("ForenSync");

            Console.WriteLine("🔐 Please log in to continue.\n");

            Console.Write("Enter User ID: ");
            string userId = Console.ReadLine();

            Console.Write("Enter Password: ");
            string password = ReadPassword();

            Console.WriteLine("\nAuthenticating...\n");

            if (UserAuthenticator.ValidateUser(userId, password))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Login successful!\n");
                Console.ResetColor();
                ShowSessionPrompt(userId);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Invalid credentials. Access denied.\n");
                Console.ResetColor();
            }
        }

        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);

            return password;
        }

        private static void ShowSessionPrompt(string userId)
        {
            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine("🧭 Session Options:");
            Console.WriteLine("────────────────────────────────────────────\n");
            Console.WriteLine("[1] Start a new case or session");
            Console.WriteLine("[2] Continue an ongoing case");
            Console.WriteLine("[3] Skip case setup and proceed to main menu\n");

            Console.Write("Enter your choice [1-3]: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                CaseManagement.CaseSession.StartNewCase(userId); // ✅ dynamic
            }
            
            else if (choice == "2") {
                //CaseManagement.CaseSession.ContinueCase(userId); // ✅ dynamic
            }
            else if (choice == "3")
            {
                MainMenu.Show(null, false); // Proceed to main menu without case
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Invalid choice. Please try again.\n");
                Console.ResetColor();
                ShowSessionPrompt(userId);
            }

            Console.WriteLine($"\nYou selected option {choice}. Proceeding...\n");
        }
    }
}
