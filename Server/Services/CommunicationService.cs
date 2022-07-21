using Server.Services.Communication;

namespace Server.Services
{
    /// <summary>
    /// Receiving requests from the client,
    /// communicating with the database and
    /// sending responses back to the client
    /// </summary>
    internal class App
    {
        private readonly Store store;

        public App()
        {
            store = new Store();
        }

        public void Run()
        {
            store.udpHandler.Start();
        }
    }
}