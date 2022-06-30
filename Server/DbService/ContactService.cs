using Server.Data;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DbService
{

  public interface IContactService
  {
    //Task SendInviteAsync(string unameFrom, string unameTo);
    Task SendInviteAsync(int uidFrom, int uidTo);

    Task RemoveContactAsync(int id);

    Task AcceptInviteAsync(string unameFrom, string unameTo);
    IEnumerable<dynamic> GetInvites(int uid);
    //public IEnumerable<Contact> GetInvites(string uname);
    IEnumerable<dynamic> GetContacts(int uid);
    //public IEnumerable<Contact> GetContacts(string uname);
  }

  public class ContactService : IContactService
  {
    BabbleContext db;
    public ContactService(BabbleContext db) => this.db = db;

    // todo: rewrite
    // get invites, sended to the person
    public IEnumerable<dynamic> GetInvites(int uid)
    {
      return from c in db.Contacts
             where c.UserToId == uid && !c.isAccepted
             select new { c.Id, Name = c.UserFrom.Name };
    }
    //public IEnumerable<Contact> GetInvites(string uname)
    //{
    //  return from c in db.Contacts
    //         where c.UserTo.Name == uname && !c.isAccepted
    //         select c;
    //}

    // get accepted contacts
    //public IEnumerable<Contact> GetContacts(string uname)
    //{
    //  return from c in db.Contacts
    //         where c.isAccepted && (c.UserFrom.Name == uname || c.UserTo.Name == uname)
    //         select c;
    //}

    public IEnumerable<dynamic> GetContacts(int uid)
    {
      return db.Contacts
         .Where(c => c.UserFromId == uid || c.UserToId == uid)
         .Select(c => c.UserFromId == uid ? new { c.Id, Name = c.NameAtUserFrom } : new { c.Id, Name = c.NameAtUserTo });
    }

    // accept invite
    public async Task AcceptInviteAsync(string unameFrom, string unameTo)
    {
      User? userFrom = db.Users.FirstOrDefault(u => u.Name == unameFrom);
      User? userTo = db.Users.FirstOrDefault(u => u.Name == unameTo);

      if (userFrom != null && userTo != null)
      {
        await AcceptInviteAsync(userFrom, userTo);
      }
    }
    private async Task AcceptInviteAsync(User userFrom, User userTo)
    {
      Contact? contact = db.Contacts.FirstOrDefault(c => c.UserFromId == userFrom.Id && c.UserToId == userTo.Id);
      if (contact != null)
      {
        contact.isAccepted = true;
        contact.NameAtUserFrom = userTo.Name;
        contact.NameAtUserTo = userFrom.Name;
        await db.SaveChangesAsync();
      }
    }

    // send invite
    public async Task SendInviteAsync(int uidFrom, int uidTo)
    {
      User? userFrom = db.Users.Find(uidFrom);
      if(userFrom == null) throw new Exception("UserFromNotFound"); // user, who send invite, doesn't exist

      User? userTo = db.Users.Find(uidTo);
      if (userTo == null) throw new Exception("UserToNotFound"); // user, who should accept invite, doesn't exist

      if (db.Contacts.Any(c => (c.UserFromId == userFrom.Id && c.UserToId == userTo.Id) || (c.UserFromId == userTo.Id && c.UserToId == userFrom.Id)))
        throw new Exception("InviteAlreadyExist");

      db.Contacts.Add(new Contact { UserFrom = userFrom, UserTo = userTo, isAccepted = false });
      
      await db.SaveChangesAsync();
    }

    public async Task RemoveContactAsync(int id)
    {
      Contact? contact = db.Contacts.Find(id);
      if (contact == null) throw new Exception("ContactNotFound");
     
      db.Contacts.Remove(contact);
      await db.SaveChangesAsync();
    }


    //public async Task SendInviteAsync(string unameFrom, string unameTo)
    //{
    //  User? userFrom = db.Users.FirstOrDefault(u => u.Name == unameFrom);
    //  User? userTo = db.Users.FirstOrDefault(u => u.Name == unameTo);
    //  if (userTo != null && userFrom != null)
    //  {
    //    db.Contacts.Add(new Contact { UserFrom = userFrom, UserTo = userTo, isAccepted = false });
    //    await db.SaveChangesAsync();
    //  }
    //}
  }
}
