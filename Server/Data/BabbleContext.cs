using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
  public class BabbleContext : DbContext
  {
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<Contact> Contacts { get; set; }


    public string DbPath { get; }
    public BabbleContext()
    {
      var folder = Environment.SpecialFolder.LocalApplicationData;
      var path = Environment.GetFolderPath(folder);
      DbPath = Path.Join(path, "babble.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
  }
}
