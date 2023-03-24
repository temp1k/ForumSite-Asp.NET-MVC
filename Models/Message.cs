using System;
using System.Collections.Generic;

namespace ForumSite.Models;

public partial class Message
{
    public int IdMessage { get; set; }

    public string TextMessage { get; set; } = null!;

    public bool? StatusChange { get; set; }

    public int? Rating { get; set; }

    public int UserId { get; set; }

    public int TopicId { get; set; }

    public virtual TopicDiscussion Topic { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
