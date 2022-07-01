using CrossLibrary;
using Server.Data;
using Server.Models;

namespace Server.Services
{
  public class DatabaseService : IUserService, IGroupService, IContactService
  {
    private ContactService contactService;
    private BabbleContext db;
    private GroupService groupService;
    private UserService userService;
    public DatabaseService()
    {
      db = new BabbleContext();
      userService = new UserService(db);
      groupService = new GroupService(db);
      contactService = new ContactService(db);
    }

    public Task<Contact> AcceptInviteAsync(int id) => contactService.AcceptInviteAsync(id);

    public Task AddGroupAsync(int uid, string groupName) => groupService.AddGroupAsync(uid, groupName);

    public User AddUser(string name, string password) => userService.AddUser(name, password);

    // work with groups
    public Task<bool> AddUserToGroupAsync(int uid, string groupName) => groupService.AddUserToGroupAsync(uid, groupName);

    public IEnumerable<Prop> GetContacts(int uid) => contactService.GetContacts(uid);

    // work with contacts
    public IEnumerable<Prop> GetInvites(int uid) => contactService.GetInvites(uid);

    // work with users
    public User? GetUser(string name) => userService.GetUser(name);
    public IEnumerable<Prop> GetUserGroups(int uid) => groupService.GetUserGroups(uid);

    public Task RemoveContactAsync(int id) => contactService.RemoveContactAsync(id);

    public Task RemoveUserAsync(int id) => userService.RemoveUserAsync(id);
    public Task RemoveUserFromGroupAsync(int uid, string groupName) => groupService.RemoveUserFromGroupAsync(uid, groupName);

    public Task RenameGroupAsync(int id, string newName) => groupService.RenameGroupAsync(id, newName);
    public Task<Prop> SendInviteAsync(int uidFrom, string unameTo) => contactService.SendInviteAsync(uidFrom, unameTo);
  }
}