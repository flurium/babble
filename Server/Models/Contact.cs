using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
  public class Contact
  {
    public int UserFromId { get; set; }
    public User UserFrom { get; set; }
    public int UserToId { get; set; }
    public User UserTo { get; set; }

    public bool isAccepted { get; set; }
  }
}
