﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
  [Index(nameof(Name), IsUnique = true)]
  public class Group
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public List<UserGroup> UserGroups { get; set; }
  }
}
