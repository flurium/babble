namespace Client.Services.Network.Base
{
    public interface IProtocolService
    {
        void Start();

        void Stop();

        void Send(byte[] data);
    }
}