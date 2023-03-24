using System;
using System.Collections.Generic;

namespace ForumSite.Models;

public partial class Friend
{
    public int IdFriends { get; set; }

    public int UserId { get; set; }

    public int FriendId { get; set; }

    public int RelationId { get; set; }

    public virtual User FriendNavigation { get; set; } = null!;

    public virtual RelationType Relation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
