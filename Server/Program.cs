using Server.DbService;
using Server.Services;
using System.Text;

namespace Server
{
  public class Program
  {

    static void Test()
    {
      DatabaseService db = new DatabaseService();

      try
      {
        //db.SendInviteAsync("z", "a");

        //foreach(var c in db.GetContacts(1))
        // {
        //   Console.WriteLine(c.Name);
        // }

        foreach(var g in db.GetUserGroups(1))
        {
          Console.WriteLine(g.Name);
        }

        //db.AddUser("a", "a");

        //db.AddGroupAsync(1, "масленок");
        //db.AddUserToGroupAsync(2, "масленок");
        //db.AddUserAsync("z", "z");
      }
      catch (Exception ex)
      {
        // beacause add existing name
        Console.WriteLine(ex.Message);
      }


      //db.AddUser("a", "b");
      //dbService.AddGroup("admin", "A");
      //dbService.RenameGroup("A", "B");
      //db.AddUser("b", "b");
      //db.SendInviteAsync("a", "b"); 

      //db.AcceptInviteAsync("a", "b");

      //var contacts = db.GetContacts("a");
      //contacts.ToList().ForEach((contact) => Console.WriteLine(contact.NameAtUserFrom));


      /*
       dbService.AddUser("aboba", "aboba");

       
       dbService.AddUserToGroup("aboba", "Admin Group");

       var g = dbService.GetUserGroups("admin");
       foreach(var group in g)
       {
         Console.WriteLine(group.Name);
       }
       */
      //dbService.AddUserToGroup("aboba", "Admin Group");


      //dbService.RemoveUser("aboba");

      //dbService.RemoveUserFromGroup("aboba", "Admin Group");
    }

    static void Main(string[] args)
    {
      Console.Title ="Babble server";
      Console.OutputEncoding = Encoding.UTF8; 

      CommunicationService cs = new CommunicationService();

      //cs.Run();      
      Test();

    }
  }
}