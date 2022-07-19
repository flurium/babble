using CrossLibrary;
using System;
using System.Threading.Tasks;

namespace Client.Services.Network.Base
{
    public abstract class ProtocolService : IProtocolService
    {
        protected readonly Action<string> handle;
        protected readonly int port;
        protected long bufferSize = 1024;
        protected Destination destination;
        protected Task listenTask;
        protected bool run = false;

        protected ProtocolService(int port, Action<string> handle)
        {
            this.port = port;
            this.handle = handle;
        }

        public abstract void Send(byte[] data);

        public abstract void Start();

        public abstract void Stop();

        public void UpdateBufferSize(long bufferSize) => this.bufferSize = bufferSize;

        public void UpdateDestination(Destination destination) => this.destination = destination;

        protected abstract void Listen();

        protected abstract string Receive();
    }
}