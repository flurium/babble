using System.Text;

namespace CrossLibrary
{
    public static class Globals
    {
        public static readonly Encoding CommunicationEncoding = Encoding.UTF8;

        public static readonly Destination ServerDestination = new() { Ip = "127.0.0.1", Port = 5001 };
    }
}