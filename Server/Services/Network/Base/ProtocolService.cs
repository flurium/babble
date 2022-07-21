using CrossLibrary;
using Server.Services.Communication;
using System.Net;

namespace Server.Services.Network.Base
{
    internal abstract class ProtocolService
    {
        protected readonly Action<string, IPEndPoint> handle;
        protected readonly int port;
        protected long bufferSize = 1024;
        protected bool run = false;

        protected ProtocolService(int port, Action<string, IPEndPoint> handle)
        {
            this.port = port;
            this.handle = handle;
        }

        public abstract void Send(Transaction response, IPEndPoint ip);

        public abstract void Start();

        public abstract void Stop();

        public void UpdateBufferSize(long bufferSize) => this.bufferSize = bufferSize;

        protected abstract void Listen();

        protected abstract string Receive();
    }
}