using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Network
{
    public abstract class ProtocolService
    {
        protected readonly int port;
        protected readonly Action<string> handle;
        protected bool run = false;
        protected Task listenTask;

        public ProtocolService(int port, Action<string> handle)
        {
            this.port = port;
            this.handle = handle;
        }
        public long BufferSize { get; set; } = 1024;

        public abstract void Start();
        public abstract void Stop();
        protected abstract string Receive();
        protected abstract void Listen();
    }
}
