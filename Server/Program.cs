using Server.Services;
using System.Text;
using static CrossLibrary.Globals;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "Babble server";
            Console.OutputEncoding = CommunicationEncoding;

            CommunicationService cs = new();
            cs.Listen();
        }
    }
}