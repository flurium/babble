using Server.Services;
using System.Text;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "Babble server";
            Console.OutputEncoding = Encoding.UTF8;

            CommunicationService cs = new();

            cs.Listen();
        }
    }
}