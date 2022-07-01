using Server.Services;
using System.Text;

namespace Server
{
  public class Program
  {
    private static async void Test()
    {
      DatabaseService db = new DatabaseService();

      try
      {
        //db.SendInviteAsync("z", "a");

        //foreach (var c in db.GetContacts(3))
        //{
        //  Console.WriteLine(c.Name);
        //}

        // вот так использовать
        //await db.SendInviteAsync(1, 3);
        //await db.AddUserAsync("a", "a");

        //var user = db.AddUser("g", "g");
        //Console.WriteLine(user.Id);

        //var ser = JsonConvert.SerializeObject(db.GetUser("a"));
        //Console.WriteLine(ser);

        //var obj = JsonConvert.DeserializeObject<dynamic>(ser);
        //if (obj != null)
        //  Console.WriteLine(obj.Name);

        //foreach(var g in db.GetUserGroups(1))
        //{
        //  Console.WriteLine(g.Name);
        //}

        //db.AddUser("a", "a");

        //db.AddGroupAsync(1, "масленок");
        //db.AddUserToGroupAsync(2, "масленок");
        //db.AddUserAsync("z", "z");
      }
      catch (Exception ex)
      {
        // тут обрабативать исключения основиваясь на месседже исключения
        // beacause add existing name
        Console.WriteLine(ex.Message);
        Console.Write("aaaaaaaaaaaaaaaaaaaaaaaaaaaa");
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

    private static void Main(string[] args)
    {
      Console.Title = "Babble server";
      Console.OutputEncoding = Encoding.UTF8;

      CommunicationService cs = new CommunicationService();

      //cs.Run();
      Test();
    }
  }
}