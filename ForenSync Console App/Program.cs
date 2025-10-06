using ForenSync_Console_App.UI;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        LoginPage.PromptCredentials();
    }
}
