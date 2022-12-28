using LinkShortener.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;


namespace LinkShortener.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IConfiguration _configuration;
        public AdminController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var connectionString = _configuration.GetValue<string>("ConnectionString:DefaultConnectionString");
            var mongoClient = new MongoClient(connectionString);
            _mongoDatabase = mongoClient.GetDatabase("LinkShortener");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var admin = HttpContext.Session.GetString("admin");
            if (admin != null)
            {
                if (admin == "False")
                {
                    return RedirectToAction("LogOut", "Login");
                }
                var usersCollection = _mongoDatabase.GetCollection<User>("users");
                var shortlUrlCollection = _mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
                var users = await usersCollection.Find(Builders<User>.Filter.Eq(x => x.admin, false)).ToListAsync();
                var shortUrls = await shortlUrlCollection.Find(Builders<ShortenedUrl>.Filter.Empty).ToListAsync();
                var mostUrl = shortUrls.OrderByDescending(x => x.Click).Take(1);
                var mostUser = users.OrderByDescending(n => n.Urls).Take(1);
                ViewBag.Users = users;
                ViewBag.shortUrls = shortUrls;
                dynamic infos = new ExpandoObject();
                infos.MostUrl = mostUrl;
                infos.MostUser = mostUser;
                return View(infos);
            }
            return RedirectToAction("LogOut", "Login");
        }

        public async Task<IActionResult> InfoChart()
        {
            return Json(ClickList());
        }

        public List<User> ClickList()
        {
            List<User> cs = new List<User>();
            var userCollection = _mongoDatabase.GetCollection<User>("users");
            var users = userCollection.Find(Builders<User>.Filter.Empty).ToList();

            foreach (var item in users)
            {
                cs.Add(new User()
                {
                    userName = item.userName,
                    Urls = item.Urls
                });
            }
            return cs;
        }
    }
}
