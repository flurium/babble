using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CrossLibrary.Network
{
    public abstract class ProtocolService
    {
        protected readonly Action<string> handle;
        protected readonly string ip;
        protected readonly int port;
        protected long bufferSize = 1024;
        protected bool run = false;

        protected ProtocolService(string ip, int port, Action<string> handle)
        {
            this.ip = ip;
            this.port = port;
            this.handle = handle;
        }

        public abstract IPEndPoint RemoteIpEndPoint { get; }

        /// <summary>
        /// Send to current remote ip end point
        /// </summary>
        /// <param name="data"></param>
        public abstract void Send(byte[] data);

        /// <summary>
        /// Send to special ip end point
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        public abstract void Send(byte[] data, IPEndPoint ip);

        public abstract void Start();

        public abstract void Stop();

        public void UpdateBufferSize(long bufferSize) => this.bufferSize = bufferSize;

        protected abstract void Listen();

        protected abstract string Receive();
    }
}