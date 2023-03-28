using ForumSite.Models;

namespace ForumSite.ModelViews
{
    public class YourProfileModel
    {
        public User user { get; set; }

        public ICollection<Friend> friends { get; set; }

        public int countMessages { get; set; }

        public int countTopics { get; set; }

    }
}
