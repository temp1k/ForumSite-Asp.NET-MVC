using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ForumSite.ModelViews
{
    public class LoginModel
    {
        [BindRequired]
        
        public string Login { get; set; }
        [BindRequired]
        public string Password { get; set; }
    }
}
