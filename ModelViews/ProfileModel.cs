using ForumSite.Models;

namespace ForumSite.ModelViews
{
    public class ProfileModel
    {
        public User user { get; set; }

        public ICollection<Friend> friends { get; set; } = null!;

        public int countMessages { get; set; }

        public int countTopics { get; set; }

        public bool isAdmin { get; set; } = false;

        public string friendType { get; set; } = null;

        public User currentUser { get; set; } = null!;
    }
}
