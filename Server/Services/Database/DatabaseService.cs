using CrossLibrary;
using Server.Data.Models;
using Server.Models;

namespace Server.Services.Database
{
    public class DatabaseService : IUserService, IGroupService, IContactService
    {
        private readonly ContactService contactService;
        private readonly BabbleContext db;
        private readonly GroupService groupService;
        private readonly UserService userService;

        public DatabaseService()
        {
            db = new BabbleContext();
            userService = new UserService(db);
            groupService = new GroupService(db);
            contactService = new ContactService(db);
        }

        public BabbleContext BabbleContext { get; set; }

        public UserService UserService { get; set; }

        public ContactService ContactService { get; set; }

        public GroupService GroupService { get; set; }

        public BabbleContext context
        {
            get => default;
            set
            {
            }
        }

        public Task<Contact> AcceptInviteAsync(int id) => contactService.AcceptInviteAsync(id);

        public Task<Prop> CreateGroupAsync(int uid, string groupName) => groupService.CreateGroupAsync(uid, groupName);

        public User AddUser(string name, string password) => userService.AddUser(name, password);

        // work with groups
        public Task<Prop> AddUserToGroupAsync(int uid, string groupName) => groupService.AddUserToGroupAsync(uid, groupName);

        public Contact GetContact(int uidFromm, int uidTo) => contactService.GetContact(uidFromm, uidTo);

        public IEnumerable<Prop> GetContacts(int uid) => contactService.GetContacts(uid);

        public IEnumerable<int> GetGroupMembersIds(int id) => groupService.GetGroupMembersIds(id);

        // work with contacts
        public IEnumerable<Prop> GetInvites(int uid) => contactService.GetInvites(uid);

        // work with users
        public User? GetUser(string name) => userService.GetUser(name);

        public IEnumerable<Prop> GetUserGroups(int uid) => groupService.GetUserGroups(uid);

        public Task RemoveContact(int uidFrom, int uidTo) => contactService.RemoveContact(uidFrom, uidTo);

        public Task RemoveContactAsync(int id) => contactService.RemoveContactAsync(id);

        public Task RemoveUserAsync(int id) => userService.RemoveUserAsync(id);

        public Task RemoveUserFromGroupAsync(int uid, string groupName) => groupService.RemoveUserFromGroupAsync(uid, groupName);

        public Task RenameContact(int uidFrom, int uidTo, string newName) => contactService.RenameContact(uidFrom, uidTo, newName);

        public Task RenameGroupAsync(int id, string newName) => groupService.RenameGroupAsync(id, newName);

        public Task<Contact> SendInviteAsync(int uidFrom, string unameTo) => contactService.SendInviteAsync(uidFrom, unameTo);
    }
}