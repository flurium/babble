using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
  public class BabbleContext : DbContext
  {
    public BabbleContext()
    {
      DbPath = Path.Join(GetRelativePath(), "babble.db");
      Console.WriteLine(DbPath);
    }

    public DbSet<Contact> Contacts { get; set; }
    public string DbPath { get; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    private string GetRelativePath()
    {
      // for example
      // C:\Users\roman\Git\flurium\babble\Server\bin\Debug\net6.0
      // should become
      // C:\Users\roman\Git\flurium\babble\Server

      string current = Directory.GetCurrentDirectory();

      string search = "Server";
      int serverFolderIndex = current.IndexOf(search);

      if (serverFolderIndex == -1)
      {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      }

      return current.Substring(0, serverFolderIndex + search.Length) + "\\Database";
    }
  }
}