using CrossLibrary;
using Server.Data;
using Server.Models;

namespace Server.Services
{
  // prefix 'u' means 'user'

  public interface IGroupService
  {
    Task AddGroupAsync(int uid, string groupName);

    Task<bool> AddUserToGroupAsync(int uid, string groupName);

    IEnumerable<Prop> GetUserGroups(int uid);

    Task RemoveUserFromGroupAsync(int uid, string groupName);

    Task RenameGroupAsync(int id, string newName);

    IEnumerable<int> GetGroupMembersIds(int id);
  }

  public class GroupService : IGroupService
  {
    private BabbleContext db;

    public GroupService(BabbleContext db) => this.db = db;

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
    public IEnumerable<Prop> GetUserGroups(int uid)
    {
      return db.UserGroups.Where(ug => ug.UserId == uid).Select(ug => new Prop { Id = ug.GroupId, Name = ug.Group.Name });
    }

    // id = group id
    public IEnumerable<int> GetGroupMembersIds(int id)
    {
      return db.UserGroups.Where(ug => ug.GroupId == id).Select(ug => ug.UserId);
    }

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

    public async Task RenameGroupAsync(int id, string newName)
    {
      Group? group = db.Groups.Find(id);
      if (group != null)
      {
        group.Name = newName;
        await db.SaveChangesAsync();
      }
    }
  }
}