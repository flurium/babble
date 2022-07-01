using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossLibrary
{
  public struct Request
  {
    public Command Command { get; set; }
    public dynamic Data { get; set; }
  }

  public struct Response
  {
    public Status Status;
    public Command Command;
    public dynamic Data { get; set; }
  }

  public enum Status {
    OK,
    Bad
  }

  public enum Command
  {
    SignIn,
    SignUp,
    SendToContact,
    SendToGroup,
    Invite,
    AcceptInvite
  }
}
