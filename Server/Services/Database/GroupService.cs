using CrossLibrary;
using Server.Data.Models;
using Server.Models;
using Server.Services.Exceptions;

namespace Server.Services.Database
{
    // prefix 'u' means 'user'

    internal interface IGroupService
    {
        Task<Prop> CreateGroupAsync(int uid, string groupName);

        Task<Prop> AddUserToGroupAsync(int uid, string groupName);

        IEnumerable<Prop> GetUserGroups(int uid);

        Task RemoveUserFromGroupAsync(int group, int user);

        Task RenameGroupAsync(int id, string newName);

        IEnumerable<int> GetGroupMembersIds(int id);
    }

    internal class GroupService : IGroupService
    {
        private readonly BabbleContext db;

        public GroupService(BabbleContext db) => this.db = db;

        public async Task<Prop> CreateGroupAsync(int uid, string groupName)
        {
            Group group = new() { Name = groupName };
            User? user = db.Users.Find(uid);

            if (user != null)
            {
                group = db.Groups.Add(group).Entity;
                db.UserGroups.Add(new UserGroup { Group = group, User = user });
                await db.SaveChangesAsync();

                return new Prop { Id = group.Id, Name = group.Name };
            }
            throw new InfoException("User not found!");
        }

        public async Task<Prop> AddUserToGroupAsync(int uid, string groupName)
        {
            Group? group = db.Groups.FirstOrDefault(g => g.Name == groupName);
            User? user = db.Users.Find(uid);

            if (group == null) throw new InfoException("Group is not found!");
            if (user == null) throw new InfoException("User is not found!");

            db.UserGroups.Add(new UserGroup { User = user, Group = group });
            await db.SaveChangesAsync();
            return new Prop { Id = group.Id, Name = group.Name };
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
        public async Task RemoveUserFromGroupAsync(int groupId, int userId)
        {
            Group? group = db.Groups.Find(groupId);
            if (group != null)
            {
                UserGroup? userGroup = db.UserGroups.FirstOrDefault(ug => ug.GroupId == group.Id && ug.UserId == userId);
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