using Client.Services.Communication;
using CrossLibrary;
using CrossLibrary.Network;
using System;
using System.Net;

namespace Client.Services.Network.Base
{
    public abstract class ProtocolHandler : IProtocolService
    {
        protected ProtocolService protocolService;
        protected Store store;

        protected ProtocolHandler(string ip, int port, Store store)
        {
            protocolService = CreateProtocolService(ip, port, Handle);
            this.store = store;
        }

        public void Send(byte[] data) => protocolService.Send(data);

        public void Send(byte[] data, IPEndPoint ipEndPoint) => protocolService.Send(data, ipEndPoint);

        public void Start() => protocolService.Start();

        public void Stop() => protocolService.Stop();

        public void UpdateBufferSize(long bufferSize) => protocolService.UpdateBufferSize(bufferSize);

        //public void UpdateDestination(Destination destination) => protocolService.UpdateDestination(destination);

        protected abstract ProtocolService CreateProtocolService(string ip, int port, Action<string> handle);

        protected abstract void Handle(string str);
    }
}