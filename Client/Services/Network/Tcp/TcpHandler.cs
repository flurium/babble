using Client.Services.Network.Base;
using Client.Services.Network.Tcp;
using System;

namespace Client.Services
{
    public partial class CommunicationService
    {
        public class TcpHandler : ProtocolHandler
        {
            public TcpHandler(int port) : base(port) { }

            protected override ProtocolService CreateProtocolService(int port, Action<string> handle) => new TcpService(port, handle);

            protected override void Handle(string str)
            {
                //throw new NotImplementedException();
            }
        }
    }
}