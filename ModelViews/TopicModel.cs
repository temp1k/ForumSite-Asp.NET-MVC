using ForumSite.Models;

namespace ForumSite.ModelViews
{
    public class TopicModel
    {
        public User user { get; set; } = new();
        public TopicDiscussion topic { get; set; } = new();

        public bool isAdmin { get; set; }

    }
}
