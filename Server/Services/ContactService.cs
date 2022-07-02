using CrossLibrary;
using Server.Data;
using Server.Models;

namespace Server.Services
{
  public interface IContactService
  {
    Task<Contact> AcceptInviteAsync(int id);

    Contact GetContact(int uidFromm, int uidTo);
    IEnumerable<Prop> GetContacts(int uid);

    IEnumerable<Prop> GetInvites(int uid);

    Task RemoveContactAsync(int id);

    Task<Prop> SendInviteAsync(int uidFrom, string unameTo);
  }

  public class ContactService : IContactService
  {
    private BabbleContext db;

    public ContactService(BabbleContext db) => this.db = db;

    // accept invite
    public async Task<Contact> AcceptInviteAsync(int id)
    {
      Contact? contact = db.Contacts.Find(id);
      if (contact == null) throw new Exception("Contact isn't found");

      contact.isAccepted = true;
      contact.NameAtUserFrom = contact.UserTo.Name;
      contact.NameAtUserTo = contact.UserFrom.Name;
      await db.SaveChangesAsync();
      return contact;
    }

    public Contact GetContact(int uidFrom, int uidTo)
    {
      Contact? contact = db.Contacts.FirstOrDefault(
        c => (c.UserFromId == uidFrom && c.UserToId == uidTo) || (c.UserToId == uidFrom && c.UserFromId == uidTo)
        );
      if (contact == null) throw new Exception("Contact isn't found");
      return contact;
    }

    public IEnumerable<Prop> GetContacts(int uid)
    {
      return db.Contacts
         .Where(c => c.UserFromId == uid || c.UserToId == uid)
         .Select(c => c.UserFromId == uid ?
         new Prop { Id = c.UserToId, Name = c.NameAtUserFrom }
         : new Prop { Id = c.UserFromId, Name = c.NameAtUserTo });
    }

    // todo: rewrite
    // get invites, sended to the person
    public IEnumerable<Prop> GetInvites(int uid)
    {
      return db.Contacts.Where(c => c.UserToId == uid && !c.isAccepted).Select(c => new Prop { Id = c.Id, Name = c.UserFrom.Name });
    }

    public async Task RemoveContactAsync(int id)
    {
      Contact? contact = db.Contacts.Find(id);
      if (contact == null) throw new Exception("ContactNotFound");

      db.Contacts.Remove(contact);
      await db.SaveChangesAsync();
    }

    public async Task<Prop> SendInviteAsync(int uidFrom, string unameTo)
    {
      User? userFrom = db.Users.Find(uidFrom);
      if (userFrom == null) throw new Exception("User From Not Found"); // user, who send invite, doesn't exist

      User? userTo = db.Users.FirstOrDefault(u => u.Name == unameTo);
      if (userTo == null) throw new Exception("User To Not Found"); // user, who should accept invite, doesn't exist

      if (db.Contacts.Any(c => c.UserFromId == userFrom.Id && c.UserToId == userTo.Id || c.UserFromId == userTo.Id && c.UserToId == userFrom.Id))
        throw new Exception("Invite Already Exist");

      var contact = db.Contacts.Add(new Contact { UserFrom = userFrom, UserTo = userTo, isAccepted = false });

      await db.SaveChangesAsync();
      return new Prop { Id = contact.Entity.Id, Name = contact.Entity.UserFrom.Name };
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
  }
}