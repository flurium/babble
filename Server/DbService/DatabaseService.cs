using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Data;
using Server.Models;

namespace Server.DbService
{
  public class DatabaseService : IUserService, IGroupService
  {
    BabbleContext db;
    UserService userService;
    GroupService groupService;

    public DatabaseService()
    {
      db = new BabbleContext();
      userService = new UserService(db);
      groupService = new GroupService(db);
    }

    // work with users
    public void AddUser(string name, string password) => userService.AddUser(name, password);
    public void RemoveUser(string name) => userService.RemoveUser(name);
    public void RemoveUser(int id) => userService.RemoveUser(id);


    // work with groups
    public bool AddUserToGroup(string uname, string groupName) => groupService.AddUserToGroup(uname, groupName);
    public bool AddUserToGroup(int uid, string groupName) => groupService.AddUserToGroup(uid, groupName);

    public IEnumerable<Group> GetUserGroups(int uid) => groupService.GetUserGroups(uid);
    public IEnumerable<Group> GetUserGroups(string uname) => groupService.GetUserGroups(uname);

    public void RemoveUserFromGroup(int uid, string groupName) => groupService.RemoveUserFromGroup(uid, groupName);
    public void RemoveUserFromGroup(string uname, string groupName) => groupService.RemoveUserFromGroup(uname, groupName);

    public void AddGroup(int uid, string groupName) => groupService.AddGroup(uid, groupName);
    public void AddGroup(string uname, string groupName) => groupService.AddGroup(uname, groupName);
  }
}
