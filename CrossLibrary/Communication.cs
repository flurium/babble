using Newtonsoft.Json;
using static CrossLibrary.Globals;

namespace CrossLibrary
{
    public enum Command
    {
        SignIn,
        SignUp,
        SendMessageToContact, SendMessageToGroup,
        GetMessageFromContact, GetMessageFromGroup,
        SendInvite,
        GetInvite,
        AcceptInvite,
        GetContact,
        RenameContact,
        RemoveContact,
        RenameGroup,
        CreateGroup,
        LeaveGroup,
        EnterGroup,
        Disconnect,
        SendFileMessageToContact, SendFileMessageToGroup,
        GetFileMessageSize, GetClientAddress
    }

    public enum Status
    {
        OK,
        Bad
    }

    public struct Prop
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public struct Request
    {
        public Command Command { get; set; }
        public dynamic Data { get; set; }

        public byte[] ToStrBytes()
        {
            string str = JsonConvert.SerializeObject(this);
            return CommunicationEncoding.GetBytes(str);
        }
    }

    public struct Response
    {
        public Command Command { get; set; }
        public dynamic Data { get; set; }
        public Status Status { get; set; }
    }

    public struct Destination
    {
        public string Ip { get; set; }
        public int Port { get; set; }
    }
}