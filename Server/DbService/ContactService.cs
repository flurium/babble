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
    Task SendInviteAsync(string unameFrom, string unameTo);
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
      //var contacts = (from c in db.Contacts
      //               where c.UserFromId == uid || c.UserToId == uid
      //               select c).ToList();


      return db.Contacts
         .Where(c => c.UserFromId == uid || c.UserToId == uid)
         .Select(c => c.UserFromId == uid ? new { c.Id, Name = c.NameAtUserFrom } : new { c.Id, Name = c.NameAtUserTo });

      //contacts.ForEach(c =>
      //{
      //  if(c.UserFromId == uid)
      //  {
      //    res.Add( new {c.Id, Name = c.NameAtUserFrom});
      //  }
      //  else
      //  {
      //    res.Add( new {c.Id, Name = c.NameAtUserTo});
      //  }
      //});
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
    public async Task SendInviteAsync(string unameFrom, string unameTo)
    {
      User? userFrom = db.Users.FirstOrDefault(u => u.Name == unameFrom);
      User? userTo = db.Users.FirstOrDefault(u => u.Name == unameTo);
      if (userTo != null && userFrom != null)
      {
        db.Contacts.Add(new Contact { UserFrom = userFrom, UserTo = userTo, isAccepted = false });
        await db.SaveChangesAsync();
      }
    }
  }
}
