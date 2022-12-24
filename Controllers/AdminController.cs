using LinkShortener.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
            if (admin == "False")
            {
                return RedirectToAction("Index", "Login");
            }
            var usersCollection = _mongoDatabase.GetCollection<User>("users");
            var originalUrlCollection = _mongoDatabase.GetCollection<OriginalUrl>("original-urls");
            var users = await usersCollection.Find(Builders<User>.Filter.Eq(x => x.admin, false)).ToListAsync();
            var orgUrls = await originalUrlCollection.Find(Builders<OriginalUrl>.Filter.Empty).ToListAsync();
            var mostUrl = orgUrls.OrderByDescending(x => x.Click).FirstOrDefault();
            var mostUser = users.OrderByDescending(x => x.Urls).FirstOrDefault();
            ViewBag.Users = users;
            ViewBag.OriginalUrls = originalUrlCollection;
            dynamic infos = new ExpandoObject();
            infos.MostUrl = mostUrl;
            infos.MostUser = mostUser;
            return View(infos);
        }
    }
}
