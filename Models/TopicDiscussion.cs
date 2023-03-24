using System;
using System.Collections.Generic;

namespace ForumSite.Models;

public partial class TopicDiscussion
{
    public int IdTopic { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime DateCreation { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Message> Messages { get; } = new List<Message>();

    public virtual User User { get; set; } = null!;
}
