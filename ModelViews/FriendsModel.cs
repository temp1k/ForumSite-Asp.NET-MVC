using ForumSite.Models;

namespace ForumSite.ModelViews
{
    public class FriendsModel
    {
        public ICollection<User> users { get; set; }

        public User currentUser { get; set; }
    }
}
