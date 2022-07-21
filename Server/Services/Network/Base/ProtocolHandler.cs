using CrossLibrary;
using CrossLibrary.Network;
using System.Net;

namespace Server.Services.Network.Base
{
    internal abstract class ProtocolHandler
    {
        /// <summary>
        /// Instance of ProtocoleService
        /// </summary>
        protected ProtocolService protocol;

        protected ProtocolHandler(string ip, int port)
        {
            protocol = CreateProtocolService(ip, port, Handle);
        }

        public void Start() => protocol.Start();

        public void Stop() => protocol.Stop();

        public void UpdateBufferSize(long bufferSize) => protocol.UpdateBufferSize(bufferSize);

        protected abstract ProtocolService CreateProtocolService(string ip, int port, Action<string> handle);

        protected abstract void Handle(string str);

        protected void Send(Transaction transaction)
        {
            // BAD: TO JSON 2 TIMES
            // FIX !!!!!!!!!!!
            string json = transaction.ToJson();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(json);
            Console.ForegroundColor = ConsoleColor.Cyan;

            protocol.Send(transaction.ToStrBytes());
        }

        protected void Send(Transaction transaction, IPEndPoint endPoint)
        {
            // BAD: TO JSON 2 TIMES
            // FIX !!!!!!!!!!!
            string json = transaction.ToJson();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(json);
            Console.ForegroundColor = ConsoleColor.Cyan;

            protocol.Send(transaction.ToStrBytes(), endPoint);
        }
    }
}