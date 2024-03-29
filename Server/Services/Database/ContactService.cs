﻿using CrossLibrary;
using Server.Data.Models;
using Server.Models;
using Server.Services.Exceptions;

namespace Server.Services.Database
{
    internal interface IContactService
    {
        Task<Contact> AcceptInviteAsync(int id);

        Task DenyInvite(int id);

        Contact GetContact(int uidFrom, int uidTo);

        IEnumerable<Prop> GetContacts(int uid);

        IEnumerable<Prop> GetInvites(int uid);

        Task RemoveContactAsync(int id);

        Task<Contact> SendInviteAsync(int uidFrom, string unameTo);

        Task RenameContact(int uidFrom, int uidTo, string newName);

        Task RemoveContact(int uidFrom, int uidTo);
    }

    internal class ContactService : IContactService
    {
        private readonly BabbleContext db;

        public ContactService(BabbleContext db) => this.db = db;

        // accept invite
        public async Task<Contact> AcceptInviteAsync(int id)
        {
            Contact? contact = db.Contacts.Find(id);
            if (contact == null) throw new InfoException("Contact isn't found");

            contact.isAccepted = true;
            contact.NameAtUserFrom = contact.UserTo.Name;
            contact.NameAtUserTo = contact.UserFrom.Name;
            await db.SaveChangesAsync();
            return contact;
        }

        public async Task DenyInvite(int id)
        {
            Contact? contact = db.Contacts.Find(id);
            if (contact == null) throw new InfoException("Contact isn't found");
            db.Contacts.Remove(contact);
            await db.SaveChangesAsync();
        }

        public Contact GetContact(int uidFrom, int uidTo)
        {
            Contact? contact = db.Contacts.FirstOrDefault(
              c => c.UserFromId == uidFrom && c.UserToId == uidTo || c.UserToId == uidFrom && c.UserFromId == uidTo
              );
            if (contact == null) throw new InfoException("Contact isn't found");
            return contact;
        }

        public IEnumerable<Prop> GetContacts(int uid)
        {
            return db.Contacts
               .Where(c => c.isAccepted && (c.UserFromId == uid || c.UserToId == uid))
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

        public async Task RemoveContact(int uidFrom, int uidTo)
        {
            Contact contact = GetContact(uidFrom, uidTo);
            db.Contacts.Remove(contact);
            await db.SaveChangesAsync();
        }

        public async Task RemoveContactAsync(int id)
        {
            Contact? contact = db.Contacts.Find(id);
            if (contact == null) throw new InfoException("ContactNotFound");

            db.Contacts.Remove(contact);
            await db.SaveChangesAsync();
        }

        public async Task RenameContact(int uidFrom, int uidTo, string newName)
        {
            Contact contact = GetContact(uidFrom, uidTo);
            if (contact.UserFromId == uidFrom)
            {
                contact.NameAtUserFrom = newName;
            }
            else
            {
                contact.NameAtUserTo = newName;
            }
            await db.SaveChangesAsync();
        }

        public async Task<Contact> SendInviteAsync(int uidFrom, string unameTo)
        {
            User? userFrom = db.Users.Find(uidFrom);
            if (userFrom == null) throw new InfoException("User Not Found"); // user, who send invite, doesn't exist

            User? userTo = db.Users.FirstOrDefault(u => u.Name == unameTo);
            if (userTo == null) throw new InfoException("User Not Found"); // user, who should accept invite, doesn't exist

            if (userFrom.Id == userTo.Id) throw new InfoException("You can't send invite to yourself");

            if (db.Contacts.Any(c => (c.UserFromId == userFrom.Id && c.UserToId == userTo.Id) || (c.UserFromId == userTo.Id && c.UserToId == userFrom.Id)))
            {
                throw new InfoException("Invite Already Exist");
            }

            var contact = db.Contacts.Add(new Contact { UserFrom = userFrom, UserTo = userTo, isAccepted = false });

            await db.SaveChangesAsync();
            return contact.Entity;
        }
    }
}