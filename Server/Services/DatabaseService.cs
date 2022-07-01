using CrossLibrary;
using Server.Data;
using Server.Models;

namespace Server.Services
{
  public class DatabaseService : IUserService, IGroupService, IContactService
  {
    private BabbleContext db;
    private UserService userService;
    private GroupService groupService;
    private ContactService contactService;

    public DatabaseService()
    {
      db = new BabbleContext();
      userService = new UserService(db);
      groupService = new GroupService(db);
      contactService = new ContactService(db);
    }

    // work with users
    public User? GetUser(string name) => userService.GetUser(name);

    public User AddUser(string name, string password) => userService.AddUser(name, password);

    public Task RemoveUserAsync(int id) => userService.RemoveUserAsync(id);

    // work with groups
    public Task<bool> AddUserToGroupAsync(int uid, string groupName) => groupService.AddUserToGroupAsync(uid, groupName);

    public Task RenameGroupAsync(int id, string newName) => groupService.RenameGroupAsync(id, newName);

    public IEnumerable<Prop> GetUserGroups(int uid) => groupService.GetUserGroups(uid);

    public Task RemoveUserFromGroupAsync(int uid, string groupName) => groupService.RemoveUserFromGroupAsync(uid, groupName);

    public Task AddGroupAsync(int uid, string groupName) => groupService.AddGroupAsync(uid, groupName);

    // work with contacts
    public Task SendInviteAsync(int uidFrom, int uidTo) => contactService.SendInviteAsync(uidFrom, uidTo);

    public Task AcceptInviteAsync(string unameFrom, string unameTo) => contactService.AcceptInviteAsync(unameFrom, unameTo);

    public IEnumerable<Prop> GetInvites(int uid) => contactService.GetInvites(uid);

    public IEnumerable<Prop> GetContacts(int uid) => contactService.GetContacts(uid);

    public Task RemoveContactAsync(int id) => contactService.RemoveContactAsync(id);

    public Task<Contact> SendInviteAsync(int uidFrom, string unameTo) => contactService.SendInviteAsync(uidFrom, unameTo);

    public Task<Contact?> AcceptInviteAsync(int id) => contactService.AcceptInviteAsync(id);
  }
}