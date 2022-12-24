using LinkShortener.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
            var shortenedUrlCollection = _mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
            return View();
        }
    }
}
