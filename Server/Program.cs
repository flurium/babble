using Server.DbService;

namespace Server
{
  public class Program
  {

    static void Test()
    {
      DatabaseService dbService = new DatabaseService();

      //dbService.AddGroup("admin", "A");
      //dbService.RenameGroup("A", "B");
      //dbService.AddUser("admin", "admin");
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
      Test();
    }
  }
}