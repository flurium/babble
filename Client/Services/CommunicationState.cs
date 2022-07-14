using CrossLibrary;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

      // base functions
      /// <summary>
      /// Base rename function, must be called from children.
      /// </summary>
      /// <param name="data">Data in request. Must have NewName property.</param>
      /// <param name="collection">Collection that must be updated.</param>
      /// <param name="command"></param>
      protected void Rename(dynamic data, ref ObservableCollection<Prop> collection, Command command)
      {
        Request req = new() { Command = command, Data = data };
        cs.SendData(req);

        for (int i = 0; i < collection.Count; i++)
        {
          if (collection[i].Id == cs.currentProp.Id)
          {
            cs.currentProp = new Prop { Id = cs.currentProp.Id, Name = data.NewName };
            collection[i] = cs.currentProp;
            break;
          }
        }
      }

      // abstracts (will be overrided)

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