using System;
using System.Collections.Generic;

namespace ForumSite.Models;

public partial class RelationType
{
    public int IdRelation { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Friend> Friends { get; } = new List<Friend>();
}
