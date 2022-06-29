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
    public void SendInvite(string unameFrom, string unameTo);
    public void AcceptInvite(string unameFrom, string unameTo);
    public IEnumerable<Contact> GetInvites(string uname);
    public IEnumerable<Contact> GetContacts(string uname);
  }

  public class ContactService : IContactService
  {
    BabbleContext db;
    public ContactService(BabbleContext db) => this.db = db;

    // get invites, sended to the person
    public IEnumerable<Contact> GetInvites(string uname)
    {
      return from c in db.Contacts
             where c.UserTo.Name == uname && !c.isAccepted
             select c;
    }

    // get accepted contacts
    public IEnumerable<Contact> GetContacts(string uname)
    {
      return from c in db.Contacts
             where c.isAccepted && (c.UserFrom.Name == uname || c.UserTo.Name == uname)
             select c;
    }

    // accept invite
    public void AcceptInvite(string unameFrom, string unameTo)
    {
      User? userFrom = db.Users.FirstOrDefault(u => u.Name == unameFrom);
      User? userTo = db.Users.FirstOrDefault(u => u.Name == unameTo);

      if (userFrom != null && userTo != null)
      {
        AcceptInvite(userFrom, userTo);
      }
    }
    private void AcceptInvite(User userFrom, User userTo)
    {
      Contact? contact = db.Contacts.FirstOrDefault(c => c.UserFromId == userFrom.Id && c.UserToId == userTo.Id);
      if (contact != null)
      {
        contact.isAccepted = true;
        db.SaveChangesAsync();
      }
    }

    // send invite
    public void SendInvite(string unameFrom, string unameTo)
    {
      User? userFrom = db.Users.FirstOrDefault(u => u.Name == unameFrom);
      User? userTo = db.Users.FirstOrDefault(u => u.Name == unameTo);
      if (userTo != null && userFrom != null)
      {
        db.Contacts.AddAsync(new Contact { UserFrom = userFrom, UserTo = userTo, isAccepted = false });
        db.SaveChangesAsync();
      }
    }
  }
}
