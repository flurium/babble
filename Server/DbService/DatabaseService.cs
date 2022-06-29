﻿using Server.Data;
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
    public Task AddUserAsync(string name, string password) => userService.AddUserAsync(name, password);
    //public void RemoveUserAsync(string name) => userService.RemoveUserAsync(name);
    public Task RemoveUserAsync(int id) => userService.RemoveUserAsync(id);

    // work with groups
    //public bool AddUserToGroupAsync(string uname, string groupName) => groupService.AddUserToGroupAsync(uname, groupName);
    public Task<bool> AddUserToGroupAsync(int uid, string groupName) => groupService.AddUserToGroupAsync(uid, groupName);
    //public void RenameGroup(string groupName, string newName) => groupService.RenameGroup(groupName, newName);
    public Task RenameGroupAsync(int id, string newName) => groupService.RenameGroupAsync(id, newName);

    public IEnumerable<Group> GetUserGroups(int uid) => groupService.GetUserGroups(uid);
    //public IEnumerable<Group> GetUserGroups(string uname) => groupService.GetUserGroups(uname);
    public Task RemoveUserFromGroupAsync(int uid, string groupName) => groupService.RemoveUserFromGroupAsync(uid, groupName);
    //public void RemoveUserFromGroup(string uname, string groupName) => groupService.RemoveUserFromGroup(uname, groupName);
    public Task AddGroupAsync(int uid, string groupName) => groupService.AddGroupAsync(uid, groupName);
    //public void AddGroup(string uname, string groupName) => groupService.AddGroup(uname, groupName);

    // work with contacts
    public Task SendInviteAsync(string unameFrom, string unameTo) => contactService.SendInviteAsync(unameFrom, unameTo);
    public Task AcceptInviteAsync(string unameFrom, string unameTo) => contactService.AcceptInviteAsync(unameFrom, unameTo);
    public IEnumerable<Contact> GetInvites(string uname) => contactService.GetInvites(uname);
    public IEnumerable<Contact> GetContacts(string uname) => contactService.GetContacts(uname);
  }
}