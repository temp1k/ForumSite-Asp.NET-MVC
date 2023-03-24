using System;
using System.Collections.Generic;

namespace ForumSite.Models;

public partial class Admin
{
    public int IdAdmin { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
