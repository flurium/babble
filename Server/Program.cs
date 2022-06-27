using Server.DbService;

namespace Server
{
  public class Program
  {

    static void Test()
    {
      DatabaseService dbService = new DatabaseService();

      /*
      dbService.AddUser("admin", "admin");
      dbService.AddUser("aboba", "aboba");

      dbService.AddGroup("admin", "Admin Group");
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
      
    }
  }
}