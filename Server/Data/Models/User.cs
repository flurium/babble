using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
  public class User
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; } // hashed password
    public List<UserGroup> UserGroups { get; set; }
  }
}
