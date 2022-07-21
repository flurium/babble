using Server.Data.Models;
using Server.Models;
using Server.Services.Exceptions;

namespace Server.Services.Database
{
    internal interface IUserService
    {
        User AddUser(string name, string password);

        User? GetUser(string name);

        Task RemoveUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly BabbleContext db;

        public UserService(BabbleContext db) => this.db = db;

        // pasword should be hashed
        public User AddUser(string name, string password)
        {
            User user = new() { Name = name, Password = password };
            if (db.Users.Any(u => u.Name == user.Name)) throw new InfoException("User with this name already exists");

            user = db.Users.Add(user).Entity;
            db.SaveChanges();
            return user;
        }

        public User? GetUser(string name)
        {
            return db.Users.FirstOrDefault(u => u.Name == name);
        }

        public async Task RemoveUserAsync(int id)
        {
            User? user = db.Users.Find(id);
            if (user != null)
            {
                db.Users.Remove(user);
                await db.SaveChangesAsync();
            }
        }
    }
}