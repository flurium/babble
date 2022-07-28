using System.Net;

namespace CrossLibrary.Network
{
    public abstract class ProtocolHandler<TStore> : IProtocolService
    {
        protected ProtocolService protocol;
        protected TStore store;

        public ProtocolHandler(string ip, int port, TStore store)
        {
            protocol = CreateProtocolService(ip, port, Handle);
            this.store = store;
        }

        public void Send(byte[] data, IPEndPoint ipEndPoint) => protocol.Send(data, ipEndPoint);

        public void Start() => protocol.Start();

        public void Stop() => protocol.Stop();

        public void UpdateBufferSize(long bufferSize) => protocol.UpdateBufferSize(bufferSize);

        protected abstract ProtocolService CreateProtocolService(string ip, int port, Action<string> handle);

        protected abstract void Handle(string str);
    }
}