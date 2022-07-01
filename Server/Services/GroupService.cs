using Server.Data;
using Server.Models;

namespace Server.Services
{
  // prefix 'u' means 'user'

  public interface IGroupService
  {
    Task AddGroupAsync(int uid, string groupName);

    //void AddGroup(string uname, string groupName);

    //void RenameGroup(string groupName, string newName);
    Task RenameGroupAsync(int id, string newName);

    //bool AddUserToGroupAsync(string uname, string groupName);
    Task<bool> AddUserToGroupAsync(int uid, string groupName);

    IEnumerable<dynamic> GetUserGroups(int uid);

    //IEnumerable<Group> GetUserGroups(string uname);

    Task RemoveUserFromGroupAsync(int uid, string groupName);

    // void RemoveUserFromGroup(string uname, string groupName);
  }

  public class GroupService : IGroupService
  {
    private BabbleContext db;

    public GroupService(BabbleContext db) => this.db = db;

    //public void RenameGroup(string groupName, string newName)
    //{
    //  Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
    //  if(group != null)
    //  {
    //    group.Name = newName;
    //    db.SaveChangesAsync();
    //  }
    //}
    public async Task RenameGroupAsync(int id, string newName)
    {
      Group? group = db.Groups.Find(id);
      if (group != null)
      {
        group.Name = newName;
        await db.SaveChangesAsync();
      }
    }

    public async Task AddGroupAsync(int uid, string groupName)
    {
      Group group = new Group { Name = groupName };
      User? user = db.Users.Find(uid);

      if (user != null)
      {
        db.Groups.Add(group);
        db.UserGroups.Add(new UserGroup { Group = group, User = user });
        await db.SaveChangesAsync();

        //db.UserGroups.Add(new UserGroup { Group = group, UserId = uid });
      }
    }

    // Try to not use it
    //public void AddGroup(string uname, string groupName)
    //{
    //  Group group = new Group { Name = groupName };
    //  User? user = db.Users.FirstOrDefault(u => u.Name == uname);

    //  if (user != null)
    //  {
    //    db.Groups.Add(group);
    //    db.UserGroups.Add(new UserGroup { Group = group, User = user }); // maybe not working, should test
    //    db.SaveChanges();
    //  }
    //}

    //public async Task<bool> AddUserToGroupAsync(string uname, string groupName)
    //{
    //  Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
    //  User? user = db.Users.FirstOrDefault(u => u.Name == uname);

    //  if (group != null && user != null)
    //  {
    //    db.UserGroups.Add(new UserGroup { User = user, Group = group });
    //    await db.SaveChangesAsync();
    //    return true;
    //  }
    //  return false;
    //}

    public async Task<bool> AddUserToGroupAsync(int uid, string groupName)
    {
      Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
      User? user = db.Users.Find(uid);

      if (group != null && user != null)
      {
        db.UserGroups.Add(new UserGroup { User = user, Group = group });
        await db.SaveChangesAsync();
        return true;
      }
      return false;
    }

    // return example [ {Id = 1, Name = "aboba"}, {Id = 4, Name = "adasd"} ]
    public IEnumerable<dynamic> GetUserGroups(int uid)
    {
      return from ug in db.UserGroups
             where ug.UserId == uid
             select new { ug.Group.Id, ug.Group.Name };
    }

    // throw exeption "UserNotFound" if user with this name is not found
    //public IEnumerable<Group> GetUserGroups(string uname)
    //{
    //  User? user = db.Users.FirstOrDefault(u => u.Name == uname);
    //  if (user != null)
    //  {
    //    return from ug in db.UserGroups
    //           where ug.UserId == user.Id
    //           select ug.Group;
    //  }
    //  throw new Exception("UserNotFound");
    //}

    // TODO: add throw exeptions
    public async Task RemoveUserFromGroupAsync(int uid, string groupName)
    {
      Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
      if (group != null)
      {
        UserGroup? userGroup = db.UserGroups.FirstOrDefault(ug => ug.GroupId == group.Id && ug.UserId == uid);
        if (userGroup != null)
        {
          db.UserGroups.Remove(userGroup);

          if (db.UserGroups.Count(ug => ug.GroupId == group.Id) <= 1)
          {
            db.Groups.Remove(group);
          }

          await db.SaveChangesAsync();
        }
      }
    }

    //public void RemoveUserFromGroup(string uname, string groupName)
    //{
    //  Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
    //  User? user = db.Users.FirstOrDefault(u => u.Name == uname);
    //  if (group != null && user != null)
    //  {
    //    UserGroup? userGroup = db.UserGroups.FirstOrDefault(ug => ug.GroupId == group.Id && ug.UserId == user.Id);
    //    //UserGroup? userGroup = db.UserGroups.FirstOrDefault(ug => ug.Group.Name == groupName && ug.User.Name == uname);

    //    if (userGroup != null)
    //    {
    //      db.UserGroups.Remove(userGroup);

    //      if(db.UserGroups.Count(ug => ug.GroupId == group.Id) <= 1)
    //      {
    //        db.Groups.Remove(group);
    //      }

    //      db.SaveChanges();
    //    }

    //  }
    //}
  }
}