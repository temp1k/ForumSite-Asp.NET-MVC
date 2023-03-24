using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Identity.Client;

namespace ForumSite.ModelViews
{
    public class RegistrationModel
    {
        [BindRequired]
        public string Login { get; set; }
        [BindRequired]
        public string Password { get; set; }
        [BindRequired]
        public string RepeatPassword { get; set; }
        [BindRequired]
        public string Fio { get; set; }
        public string? Email { get; set; }

        public bool checkPasswords()
        {
            if (this.Password.Equals(this.RepeatPassword)) return true;
            else return false;
        }
    }
}
