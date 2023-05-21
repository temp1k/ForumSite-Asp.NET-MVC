using ForumSite.Models;
using ForumSite.ModelViews;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Transactions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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
            List<Friend> wantFriends = await db.Friends
                .Include(f => f.User).Where(f => f.RelationId == 2 && f.FriendId == user.IdUser)
                .ToListAsync();
            MainModel mainModel = new MainModel(user, topics, wantFriends);
            return View(mainModel);
        }

        public async Task<IActionResult> FindTopic(string nameTopic)
        {
            List<TopicDiscussion> topics = await db.TopicDiscussions
                .Include(t => t.User)
                .Where(t => EF.Functions.Like(t.Name!, $"%{nameTopic}%"))
                .ToListAsync();

            if (topics != null)
            {
                List<Friend> wantFriends = await db.Friends
                .Include(f => f.User).Where(f => f.RelationId == 2 && f.FriendId == user.IdUser)
                .ToListAsync();
                MainModel mainModel = new MainModel(user, topics, wantFriends);

                return View("Index", mainModel);
            }
            else
            {
                return RedirectToAction("Index", new { id = user.IdUser });
            }

            
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

        public async Task<IActionResult> FindUser(string? loginUser)
        {
            List<User> users = await db.Users
                .Include(u => u.FriendUsers)
                .Include(u => u.FriendFriendNavigations)
                .Where(u => EF.Functions.Like(u.Login!, $"%{loginUser}%") && u.IdUser != user.IdUser)
                .ToListAsync();

            if (users != null)
            {
                FriendsModel model = new FriendsModel
                {
                    users = users,
                    currentUser = user
                };

                return View("Friends", model);
            }
            else
            {
                return RedirectToAction("Friends");
            }
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

                Friend? checkFriend = await db.Friends.FirstOrDefaultAsync(f => f.UserId == newFriend.UserId && f.FriendId == newFriend.FriendId);
                if (checkFriend != null)
                {
                    checkFriend.RelationId= 2;
                    db.Friends.Update(checkFriend);
                }
                else db.Friends.Add(newFriend);


                await db.SaveChangesAsync();
            }

            User anotherUser = await db.Users.FirstAsync(u => u.IdUser == id);

            return RedirectToAction("Profile", new { login = anotherUser.Login });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmFriend(int id, string window)
        {
            if (ModelState.IsValid)
            {
                Friend confirmFriend = await db.Friends.FirstOrDefaultAsync(f => f.UserId == id && f.FriendId == user.IdUser);

                if (confirmFriend == null) return NotFound();

                confirmFriend.RelationId = 1;

                Friend? checkOldFriend = await db.Friends.FirstOrDefaultAsync(f => f.UserId == user.IdUser && f.FriendId == id);

                if (checkOldFriend != null)
                {
                    checkOldFriend.RelationId = 1;
                    db.Friends.Update(checkOldFriend);
                }
                else
                {
                    Friend newFriend = new Friend()
                    {
                        UserId = user.IdUser,
                        FriendId = id,
                        RelationId = 1
                    };
                    db.Friends.Add(newFriend);
                }
                
                db.Friends.Update(confirmFriend);
                await db.SaveChangesAsync();
            }

            User anotherUser = await db.Users.FirstAsync(u => u.IdUser == id);

            switch (window)
            {
                case "profile":
                    return RedirectToAction("Profile", new { login = anotherUser.Login });
                case "index":
                    return RedirectToAction("Index", new { id = user.IdUser });
                default:
                    return NoContent();
            }
        
        }

        public async Task<IActionResult> DeleteFriend(int id)
        {
            if (ModelState.IsValid)
            {
                Friend? deleteFriend = await db.Friends.FirstOrDefaultAsync(f => f.UserId == user.IdUser && f.FriendId == id);

                if (deleteFriend == null) return NotFound();

                Friend? changeFriend = await db.Friends.FirstOrDefaultAsync(f => f.UserId == id && f.FriendId == user.IdUser);
                if (changeFriend != null)
                {
                    changeFriend.RelationId = 4;
                    db.Friends.Update(changeFriend);
                }
                
                db.Friends.Remove(deleteFriend);
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


                    string? friendType = null;
                    Friend? awaitFriend = await db.Friends
                            .Include(f => f.Relation)
                            .FirstOrDefaultAsync(f => f.UserId == anotherUser.IdUser && f.FriendId == user.IdUser && f.Relation.Name == "Не подтверждено");

                    if (awaitFriend != null) friendType = "Запрос";
                    else
                    {
                        Friend? friend = await db.Friends
                        .Include(f => f.Relation)
                        .FirstOrDefaultAsync(f => f.UserId == user.IdUser && f.FriendId == anotherUser.IdUser);


                        if (friend != null)
                        {
                            switch (friend.RelationId)
                            {
                                case 4:
                                    friendType = "Удален";
                                    break;
                                default:
                                    friendType = friend.Relation.Name;
                                    break;
                            }

                        }
                    }
                    ProfileModel profileModel = new ProfileModel
                    {
                        user = anotherUser,
                        friends = await db.Friends.Include(f => f.FriendNavigation)
                            .Where(f => f.UserId == anotherUser.IdUser && f.RelationId == 1).ToListAsync(),
                        countMessages = db.Messages.Count(m => m.UserId == anotherUser.IdUser),
                        countTopics = db.TopicDiscussions.Count(t => t.UserId == anotherUser.IdUser),
                        isAdmin = isAdmin,
                        friendType = friendType,
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

        public async Task<IActionResult> YourProfile(string? yourLogin)
        {
            if (yourLogin.IsNullOrEmpty()) yourLogin = user.Login;

            User? profile = await db.Users.SingleOrDefaultAsync(u => u.Login == yourLogin);

            YourProfileModel model = null;

            if (profile != null)
            {
                model = new YourProfileModel
                {
                    user = await db.Users
                        .Include(u => u.TopicDiscussions)
                            .ThenInclude(t => t.Messages)
                        .SingleOrDefaultAsync(u => u.Login == yourLogin),
                    friends = await db.Friends.Include(f => f.FriendNavigation)
                            .Where(f => f.UserId == user.IdUser).ToListAsync(),
                    countMessages = db.Messages.Count(m => m.UserId == profile.IdUser),
                    countTopics = db.TopicDiscussions.Count(t => t.UserId == profile.IdUser)
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

        public async Task<IActionResult> DeleteMessage(int idMessage, int idTopic)
        {
            Message message = await db.Messages.FirstOrDefaultAsync(m => m.IdMessage == idMessage);
            db.Messages.Remove(message);
            await db.SaveChangesAsync();

            return RedirectToAction("Topic", new { id = idTopic });
        }

        public async Task<IActionResult> ChangeMessage(int id, int? value, string? text)
        {
            Message message = await db.Messages.FirstOrDefaultAsync(m => m.IdMessage == id);

            if (value != null)
            {
                message.Rating += value;

                db.Messages.Update(message);

                await db.SaveChangesAsync();
            }


            return RedirectToAction("Topic", new { id = message.TopicId });
        }

        public async Task<IActionResult> VerifyAccount(int id)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.IdUser == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmEmail(User user)
        {
            User _user = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
            if (_user == null)
            {
                return null;
            }

            _user.Email = user.Email;
            db.Users.Update(_user);
            await db.SaveChangesAsync();

            var json = JsonConvert.SerializeObject(_user);
            return Json(json);
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