using ForumSite.Models;

namespace ForumSite.ModelViews
{
    public class MainModel
    {
        public User user { get; set; }

        public List<TopicDiscussion> topics { get; set; }

        public MainModel(User user, List<TopicDiscussion> topics)
        {
            this.user = user;
            this.topics = topics;

        }
    }
}
