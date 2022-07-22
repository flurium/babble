using System.Net;

namespace Client.Services.Network.Base
{
    public interface IProtocolService
    {
        void Send(byte[] data, IPEndPoint ipEndPoint);

        void Start();

        void Stop();

        void UpdateBufferSize(long bufferSize);

        //void UpdateDestination(Destination destination);
    }
}