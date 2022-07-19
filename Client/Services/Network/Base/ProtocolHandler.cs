using CrossLibrary;
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

        public void Send(byte[] data) => protocolService.Send(data);

        public void Start() => protocolService.Start();

        public void Stop() => protocolService.Stop();

        public void UpdateBufferSize(long bufferSize) => protocolService.UpdateBufferSize(bufferSize);

        public void UpdateDestination(Destination destination) => protocolService.UpdateDestination(destination);

        protected abstract ProtocolService CreateProtocolService(int port, Action<string> handle);

        protected abstract void Handle(string str);
    }
}