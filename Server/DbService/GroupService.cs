using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Data;
using Server.Models;

namespace Server.DbService
{

  // prefix 'u' means 'user'

  public interface IGroupService
  {
    void AddGroup(int uid, string groupName);
    void AddGroup(string uname, string groupName);

    bool AddUserToGroup(string uname, string groupName);
    bool AddUserToGroup(int uid, string groupName);

    IEnumerable<Group> GetUserGroups(int uid);
    IEnumerable<Group> GetUserGroups(string uname);

    void RemoveUserFromGroup(int uid, string groupName);
    void RemoveUserFromGroup(string uname, string groupName);
  }

  public class GroupService : IGroupService
  {
    BabbleContext db;

    public GroupService(BabbleContext db) => this.db = db;

    public void AddGroup(int uid, string groupName)
    {
      Group group = new Group { Name = groupName };
      User? user = db.Users.Find(uid);

      if (user != null)
      {
        db.Groups.Add(group);
        db.UserGroups.Add(new UserGroup { Group = group, User = user }); // maybe not working, should test
        db.SaveChanges();

        //db.UserGroups.Add(new UserGroup { Group = group, UserId = uid });
      }
    }

    public void AddGroup(string uname, string groupName)
    {
      Group group = new Group { Name = groupName };
      User? user = db.Users.FirstOrDefault(u => u.Name == uname);

      if (user != null)
      {
        db.Groups.Add(group);
        db.UserGroups.Add(new UserGroup { Group = group, User = user }); // maybe not working, should test
        db.SaveChanges();
      }
    }

    public bool AddUserToGroup(string uname, string groupName)
    {
      Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
      User? user = db.Users.FirstOrDefault(u => u.Name == uname);

      if (group != null && user != null)
      {
        db.UserGroups.Add(new UserGroup { User = user, Group = group });
        db.SaveChanges();
        return true;
      }
      return false;
    }

    public bool AddUserToGroup(int uid, string groupName)
    {
      Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
      User? user = db.Users.Find(uid);

      if (group != null && user != null)
      {
        db.UserGroups.Add(new UserGroup { User = user, Group = group });
        db.SaveChanges();
        return true;
      }
      return false;
    }

    // maybe change to only group names
    public IEnumerable<Group> GetUserGroups(int uid)
    {
      return from ug in db.UserGroups
             where ug.UserId == uid
             select ug.Group;
    }

    // throw exeption "UserNotFound" if user with this name is not found
    public IEnumerable<Group> GetUserGroups(string uname)
    {
      User? user = db.Users.FirstOrDefault(u => u.Name == uname);
      if (user != null)
      {
        return from ug in db.UserGroups
               where ug.UserId == user.Id
               select ug.Group;
      }
      throw new Exception("UserNotFound");
    }

    // TODO: add throw exeptions
    public void RemoveUserFromGroup(int uid, string groupName)
    {
      Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
      if (group != null)
      {
        UserGroup? userGroup = db.UserGroups.FirstOrDefault(ug => ug.GroupId == group.Id && ug.UserId == uid);
        if(userGroup != null)
        {
          db.UserGroups.Remove(userGroup);

          if (db.UserGroups.Count(ug => ug.GroupId == group.Id) <= 1)
          {
            db.Groups.Remove(group);
          }

          db.SaveChanges();
        }
      }
    }


    public void RemoveUserFromGroup(string uname, string groupName)
    {
      Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
      User? user = db.Users.FirstOrDefault(u => u.Name == uname);
      if (group != null && user != null)
      {
        UserGroup? userGroup = db.UserGroups.FirstOrDefault(ug => ug.GroupId == group.Id && ug.UserId == user.Id);
        //UserGroup? userGroup = db.UserGroups.FirstOrDefault(ug => ug.Group.Name == groupName && ug.User.Name == uname);

        if (userGroup != null)
        {
          db.UserGroups.Remove(userGroup);

          if(db.UserGroups.Count(ug => ug.GroupId == group.Id) <= 1)
          {
            db.Groups.Remove(group);
          }

          db.SaveChanges();
        }

      }
    }
  }
}
