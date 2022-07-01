using Server.Data;
using Server.Models;

namespace Server.Services
{
  internal interface IUserService
  {
    //Task AddUserAsync(string name, string password);
    User AddUser(string name, string password);

    Task RemoveUserAsync(int id);

    User? GetUser(string name);

    //public Task RemoveUserAsync(string name);
  }

  public class UserService : IUserService
  {
    private BabbleContext db;

    public UserService(BabbleContext db) => this.db = db;

    // pasword should be hashed
    public User AddUser(string name, string password)
    {
      var user = db.Users.Add(new User { Name = name, Password = password });
      db.SaveChanges();
      return user.Entity;
    }

    public async Task RemoveUserAsync(int id)
    {
      User? user = db.Users.Find(id);
      if (user != null)
      {
        db.Users.Remove(user);
        await db.SaveChangesAsync();
      }
    }

    public User? GetUser(string name)
    {
      return db.Users.FirstOrDefault(u => u.Name == name);
    }

    //public async Task RemoveUserAsync(string name)
    //{
    //  User? user = db.Users.FirstOrDefault(u => u.Name == name);
    //  if (user != null)
    //  {
    //    db.Users.Remove(user);
    //    await db.SaveChangesAsync();
    //  }
    //}
  }
}