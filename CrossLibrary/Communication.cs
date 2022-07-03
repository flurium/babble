namespace CrossLibrary
{
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
    RenameGroup,
    AddGroup,
    LeaveGroup,
    EnterToGroup,
    Disconnect
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
  }

  public struct Response
  {
    public Command Command { get; set; }
    public dynamic Data { get; set; }
    public Status Status { get; set; }
  }
}