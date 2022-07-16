using Client.Services.Network.Base;
using System;

namespace Client.Services.Network.Tcp
{
    public class TcpHandler : ProtocolHandler
    {
        public TcpHandler(int port) : base(port)
        {
        }

        protected override ProtocolService CreateProtocolService(int port, Action<string> handle) => new TcpService(port, handle);

        protected override void Handle(string str)
        {
            throw new NotImplementedException();
        }
    }
}