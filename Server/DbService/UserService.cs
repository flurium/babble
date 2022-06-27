using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Data;
using Server.Models;

namespace Server.DbService
{
  interface IUserService
  {
    public void AddUser(string name, string password);
    public void RemoveUser(string name);
    public void RemoveUser(int id);
  }

  public class UserService : IUserService
  {
    BabbleContext db;
    public UserService(BabbleContext db) => this.db = db;

    // pasword should be hashed
    public void AddUser(string name, string password)
    {
      db.Users.Add(new User{ Name = name, Password = password });
      db.SaveChanges();
    }

    public void RemoveUser(string name)
    {
      User? user = db.Users.FirstOrDefault(u => u.Name == name);
      if (user != null)
      {
        db.Users.Remove(user);
        db.SaveChanges();
      }
    }

    public void RemoveUser(int id)
    {
      User? user = db.Users.Find(id);
      if (user != null)
      {
        db.Users.Remove(user);
        db.SaveChanges();
      }
    }
  }
}
