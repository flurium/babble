using Server.Services;
using static CrossLibrary.Globals;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "Babble server";
            Console.OutputEncoding = CommunicationEncoding;

            App cs = new();
            cs.Run();
        }
    }
}