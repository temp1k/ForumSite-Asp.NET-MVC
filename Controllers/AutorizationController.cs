using ForumSite.Models;
using ForumSite.ModelViews;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ForumSite.Controllers
{
    public class AutorizationController : Controller
    {
        private ForumContext db;

        public AutorizationController(ForumContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Autorization()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Autorization(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Login == loginModel.Login && u.Password == loginModel.Password);
                if (user != null)
                {
                    if (!user.Block)
                    {
                        Admin isAdmin = await db.Admins.FirstOrDefaultAsync(a => a.UserId == user.IdUser);

                        List<Claim> listClaim = new List<Claim>();
                        listClaim.Add(new Claim(ClaimTypes.Name, user.Login));
                        if (isAdmin != null)
                        {
                            listClaim.Add(new Claim(ClaimTypes.Role, "Administrator"));
                        }
                        else
                        {
                            listClaim.Add(new Claim(ClaimTypes.Role, "User"));
                        }

                        var claims = listClaim;
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));


                        return RedirectToAction("Index", "Home", new { id = user.IdUser });
                    }
                    else
                    {
                        ModelState.AddModelError(" ", "Ваша учетная запись заблокирована.");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(" ", "Неккоректно введед логин или пароль.");
                    return View();
                }

            }
            ModelState.AddModelError(" ", "Все поля должны быть заполнены.");
            return RedirectToAction("Autorization");

        }

        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationModel registrationModel)
        {
            if (ModelState.IsValid)
            {
                if (registrationModel.checkPasswords())
                {
                    User user = new()
                    {
                        Login = registrationModel.Login,
                        Password = registrationModel.Password,
                        Email = registrationModel.Email,
                        Fio = registrationModel.Fio,
                        DateRegistration = DateTime.Now
                    };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Autorization");
                }
            }
            return RedirectToAction("Registration");
        }
    }
}
