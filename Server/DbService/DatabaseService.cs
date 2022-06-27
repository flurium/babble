using Server.Data;
using Server.Models;

namespace Server.DbService
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
    public void AddUser(string name, string password) => userService.AddUser(name, password);
    public void RemoveUser(string name) => userService.RemoveUser(name);
    public void RemoveUser(int id) => userService.RemoveUser(id);

    // work with groups
    public bool AddUserToGroup(string uname, string groupName) => groupService.AddUserToGroup(uname, groupName);
    public bool AddUserToGroup(int uid, string groupName) => groupService.AddUserToGroup(uid, groupName);
    public void RenameGroup(string groupName, string newName) => groupService.RenameGroup(groupName, newName);
    public IEnumerable<Group> GetUserGroups(int uid) => groupService.GetUserGroups(uid);
    public IEnumerable<Group> GetUserGroups(string uname) => groupService.GetUserGroups(uname);
    public void RemoveUserFromGroup(int uid, string groupName) => groupService.RemoveUserFromGroup(uid, groupName);
    public void RemoveUserFromGroup(string uname, string groupName) => groupService.RemoveUserFromGroup(uname, groupName);
    public void AddGroup(int uid, string groupName) => groupService.AddGroup(uid, groupName);
    public void AddGroup(string uname, string groupName) => groupService.AddGroup(uname, groupName);

    // work with contacts
    public void SendInvite(string unameFrom, string unameTo) => contactService.SendInvite(unameFrom, unameTo);
    public void AcceptInvite(string unameFrom, string unameTo) => contactService.AcceptInvite(unameFrom, unameTo);
    public IEnumerable<Contact> GetInvites(string uname) => contactService.GetInvites(uname);
    public IEnumerable<Contact> GetContacts(string uname) => contactService.GetContacts(uname);
  }
}