using System.Collections.Generic;

namespace Client.Services
{
    public partial class CommunicationService
    {
        /// <summary>
        /// Base state abstraction. Inside of CommunicationService class,
        /// because need access to private fields.
        /// </summary>
        public abstract class CommunicationState
        {
            protected CommunicationService cs;

            public void SetCommunicationService(CommunicationService cs)
            {
                this.cs = cs;
            }

            public abstract void Rename(string newName);

            /// <summary>
            /// Leave group or remove contact
            /// </summary>
            public abstract void Leave(int id);

            public abstract void SendMessage(string messageStr);

            public abstract void SendFileMessage(string messageStr, List<string> filePaths);

            public abstract void RefreshMessages();
        }
    }
}