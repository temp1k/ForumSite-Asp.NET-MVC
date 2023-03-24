using System;
using System.Collections.Generic;

namespace ForumSite.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Fio { get; set; } = null!;

    public string? Email { get; set; }

    public bool Block { get; set; }

    public DateTime DateRegistration { get; set; }

    public virtual ICollection<Admin> Admins { get; } = new List<Admin>();

    public virtual ICollection<Friend> FriendFriendNavigations { get; } = new List<Friend>();

    public virtual ICollection<Friend> FriendUsers { get; } = new List<Friend>();

    public virtual ICollection<Message> Messages { get; } = new List<Message>();

    public virtual ICollection<TopicDiscussion> TopicDiscussions { get; } = new List<TopicDiscussion>();
}
