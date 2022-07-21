using CrossLibrary;
using System.Net;

namespace Server.Services.Network.Base
{
    internal abstract class ProtocolHandler
    {
        /// <summary>
        /// Instance of ProtocoleService
        /// </summary>
        protected ProtocolService protocol;

        protected ProtocolHandler(int port)
        {
            protocol = CreateProtocolService(port, Handle);
        }

        public void Start() => protocol.Start();

        public void Stop() => protocol.Stop();

        public void UpdateBufferSize(long bufferSize) => protocol.UpdateBufferSize(bufferSize);

        protected abstract ProtocolService CreateProtocolService(int port, Action<string, IPEndPoint> handle);

        protected abstract void Handle(string str, IPEndPoint endPoint);

        protected void Send(Transaction transaction, IPEndPoint endPoint) => protocol.Send(transaction, endPoint);
    }
}