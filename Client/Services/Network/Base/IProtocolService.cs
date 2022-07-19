using CrossLibrary;

namespace Client.Services.Network.Base
{
    public interface IProtocolService
    {
        void Send(byte[] data);

        void Start();

        void Stop();

        void UpdateBufferSize(long bufferSize);

        void UpdateDestination(Destination destination);
    }
}