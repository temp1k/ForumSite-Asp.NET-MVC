﻿using ForumSite.Models;
using ForumSite.ModelViews;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;

namespace ForumSite.Controllers
{
    [Authorize]
    public class HomeController : LogBaseController
    {
        private readonly ILogger<HomeController> _logger;
        private ForumContext db;
        private static User user = null!;
        private static bool isAdmin = false;

        public HomeController(ILogger<HomeController> logger, ForumContext context)
        {
            _logger = logger;

            db = context;
        }

        public IActionResult ReturnIndex()
        {
            return RedirectToAction("Index", new { id = user.IdUser});
        }

        [Route("Home")]
        public async Task<IActionResult> Index(int id)
        {
            user = await db.Users.Include(u => u.Admins).FirstOrDefaultAsync(u => u.IdUser == id);
            isAdmin = db.Admins.Any(a => a.UserId == user.IdUser);
            List<TopicDiscussion> topics = await db.TopicDiscussions.Include(t => t.User).ToListAsync();
            MainModel mainModel = new MainModel(user, topics);
            return View(mainModel);
        }

        public async Task<IActionResult> Exit()
        {
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Autorization", "Autorization");
        }

        //public string Index()
        //{
        //    ITimeService? timeService = HttpContext.RequestServices.GetService<ITimeService>();
        //    return timeService?.Time ?? "Undefiened";
        //}
        public IActionResult About() => View();

        public async Task<IActionResult> Topic(int id)
        {
            TopicDiscussion? topic = await db.TopicDiscussions.Include(t => t.User)
                .Include(t => t.Messages)
                    .ThenInclude(m => m.User)
                .FirstAsync(t => t.IdTopic == id);
            TopicModel topicModel = new()
            {
                user = user,
                topic = topic,
                isAdmin = isAdmin
            };

            return View(topicModel);
        }



        [HttpGet]
        public IActionResult CreateTopic()
        {
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTopic(TopicDiscussion topic)
        {
            db.TopicDiscussions.Add(topic);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { id = topic.UserId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(Message message)
        {
            db.Messages.Add(message);
            await db.SaveChangesAsync();
            await Topic(message.TopicId);
            return RedirectToAction("Topic", new { id = message.TopicId });
        }

        public async Task<IActionResult> Friends()
        {
            ICollection<User> users = await db.Users
                .Include(u => u.FriendUsers)
                .Include(u => u.FriendFriendNavigations)
                .Where(u => u.IdUser != user.IdUser).ToListAsync();
            if (users != null)
            {
                FriendsModel model = new FriendsModel
                {
                    users = users,
                    currentUser = user
                };

                return View(model);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(int id)
        {
            if (ModelState.IsValid)
            {
                Friend newFriend = new Friend
                {
                    UserId = user.IdUser,
                    FriendId = id,
                    RelationId = 2
                };

                db.Friends.Add(newFriend);
                await db.SaveChangesAsync();
            }

            User anotherUser = await db.Users.FirstAsync(u => u.IdUser == id);

            return RedirectToAction("Profile", new { login = anotherUser.Login });
        }

        public async Task<IActionResult> Profile(string login)
        {
            if (login != null)
            {
                User? anotherUser = await db.Users
                    .Include(u => u.TopicDiscussions)
                    .Include(u => u.Messages)
                    .SingleOrDefaultAsync(u => u.Login == login);

                if (anotherUser != null)
                {
                    if (anotherUser.IdUser == user.IdUser)
                    {
                        return RedirectToAction("YourProfile", new { yourLogin = anotherUser.Login});
                    }

                    User currentUser = await db.Users
                        .Include(u => u.FriendUsers)
                        .Include(u => u.FriendFriendNavigations)
                        .FirstOrDefaultAsync(u => u.IdUser == user.IdUser);

                    bool isFriend = await db.Friends.AnyAsync(f => f.UserId == anotherUser.IdUser && f.FriendId == currentUser.IdUser
                            || f.UserId == currentUser.IdUser && f.FriendId == anotherUser.IdUser);

                    ProfileModel profileModel = new ProfileModel
                    {
                        user = anotherUser,
                        friends = await db.Friends.Include(f => f.FriendNavigation)
                            .Where(f => f.UserId == anotherUser.IdUser).ToListAsync(),
                        countMessages = db.Messages.Count(m => m.UserId == anotherUser.IdUser),
                        countTopics = db.TopicDiscussions.Count(t => t.UserId == anotherUser.IdUser),
                        isAdmin = isAdmin,
                        isFriend = isFriend,
                        currentUser = currentUser
                    };
                    return View(profileModel);
                }
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(string loginBlock)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == loginBlock);

            if(user != null)
            {
                if (!user.Block) user.Block = true;
                else user.Block = false;

                db.Users.Update(user);
                await db.SaveChangesAsync();

                return RedirectToAction("Profile", new { login = user.Login });
            }
            else
            {
                return BadRequest();
            }
           
        }

        public async Task<IActionResult> YourProfile(string yourLogin)
        {
            User? user = await db.Users.SingleOrDefaultAsync(u => u.Login == yourLogin);

            YourProfileModel model = null;

            if (user != null)
            {
                model = new YourProfileModel
                {
                    user = await db.Users
                        .Include(u => u.TopicDiscussions)
                            .ThenInclude(t => t.Messages)
                        .SingleOrDefaultAsync(u => u.Login == yourLogin),
                    friends = await db.Friends.Include(f => f.FriendNavigation)
                            .Where(f => f.UserId == user.IdUser).ToListAsync(),
                    countMessages = db.Messages.Count(m => m.UserId == user.IdUser),
                    countTopics = db.TopicDiscussions.Count(t => t.UserId == user.IdUser)
                };
            }
            else
            {
                model = new YourProfileModel
                {
                    user = new User { Login = "Такого пользователя не существует" }
                };
            }

            return View(model);
        }

        private async void DeleteUser(int id)
        {
            //if (ModelState.IsValid)
            //{
            //    User user = await db.Users.FirstOrDefaultAsync(u => u.IdUser == id);
            //    user.
            //}
        }

        public async Task<IActionResult> DeleteMessage(int idMessage, int idTopic)
        {
            Message message = await db.Messages.FirstOrDefaultAsync(m => m.IdMessage == idMessage);
            db.Messages.Remove(message);
            await db.SaveChangesAsync();

            return RedirectToAction("Topic", new { id = idTopic });
        }

        public IActionResult GetFile()
        {
            string file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Hello.txt");
            string file_type = "text/plain";
            string file_name = "Hello.txt";
            return PhysicalFile(file_path, file_type, file_name);
        }

        public IActionResult GetBytes()
        {
            string file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Hello.txt");
            byte[] mas = System.IO.File.ReadAllBytes(file_path);
            string file_type = "text/plain";
            string file_name = "Hello.txt";
            return File(mas, file_type, file_name);
        }

        public IActionResult GetVirtualFile()
        {
            return File("Files/Hello.txt", "application/octet-stream", "Hello.txt");
        }
    }
}