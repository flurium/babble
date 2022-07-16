using CrossLibrary;
using System;
using System.Threading.Tasks;

namespace Client.Services.Network.Base
{
    public abstract class ProtocolService : IProtocolService
    {
        protected readonly Action<string> handle;
        protected readonly int port;
        protected Task listenTask;
        protected bool run = false;

        protected ProtocolService(int port, Action<string> handle)
        {
            this.port = port;
            this.handle = handle;
        }

        public long BufferSize { get; set; } = 1024;
        protected Destination Destination { get; set; }

        public abstract void Send(byte[] data);

        public abstract void Start();

        public abstract void Stop();

        protected abstract void Listen();

        protected abstract string Receive();
    }
}