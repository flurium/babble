namespace CrossLibrary
{
  public struct Prop
  {
    public int Id { get; set; }
    public string Name { get; set; }
  }

  public struct Request
  {
    public Command Command { get; set; }
    public dynamic Data { get; set; }
  }

  public struct Response
  {
    public Status Status { get; set; }
    public Command Command { get; set; }
    public dynamic Data { get; set; }
  }

  public enum Status
  {
    OK,
    Bad
  }

  public enum Command
  {
    SignIn,
    SignUp,
    SendMessageToContact,
    SendMessageToGroup,
    GetMessageFromContact,
    GetMessageFromGroup,
    SendInvite,
    GetInvite,
    AcceptInvite,
    GetContact,
    RenameContact,
    RemoveContact,
    AddGroup,
    LeaveGroup,
    Disconnect
  }
}