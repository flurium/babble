using Newtonsoft.Json;
using static CrossLibrary.Globals;

namespace CrossLibrary
{
    /// <summary>
    /// GetGroupAddress = array of adresses of clients in group
    /// </summary>
    public enum Command
    {
        SignIn,
        SignUp,
        SendMessageToContact, SendMessageToGroup,
        GetMessageFromContact, GetMessageFromGroup,
        SendInvite,
        GetInvite,
        AcceptInvite,
        RejectInvite,
        GetContact,
        RenameContact,
        RemoveContact,
        RenameGroup,
        CreateGroup,
        LeaveGroup,
        EnterGroup,
        Disconnect,
        SendFileMessageToContact, SendFileMessageToGroup,
        ContactFileSize, GroupFileSize,
        GetClientAddress, GetGroupAddress,
        Exception
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

    public struct Transaction
    {
        public Command Command { get; set; }
        public dynamic Data { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public byte[] ToStrBytes()
        {
            string str = ToJson();
            return CommunicationEncoding.GetBytes(str);
        }
    }

    public struct Destination
    {
        public string Ip { get; set; }
        public int Port { get; set; }
    }
}