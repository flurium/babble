using Client.Services.Network.Base;
using System;

namespace Client.Services.Network.Base
{
    public abstract class ProtocolHandler : IProtocolService
    {
        protected ProtocolService protocolService;

        protected ProtocolHandler(int port)
        {
            protocolService = CreateProtocolService(port, Handle);
        }

        protected abstract ProtocolService CreateProtocolService(int port, Action<string> handle);

        protected abstract void Handle(string str);

        public void Start() => protocolService.Start();

        public void Stop() => protocolService.Stop();

        public void Send(byte[] data) => protocolService.Send(data);
    }
}