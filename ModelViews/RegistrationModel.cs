using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace ForumSite.ModelViews
{
    public class RegistrationModel
    {
        [BindRequired]
        [Remote(action: "CheckLogin", controller: "Autorization", ErrorMessage = "Такой логин уже существует")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Логин должен быть больше 5 символов")]
        public string Login { get; set; }
        [BindRequired]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть больше 7 символов")]
        public string Password { get; set; }
        [BindRequired]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
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
