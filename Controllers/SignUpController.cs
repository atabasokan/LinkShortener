using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;
using LinkShortener.Models;

namespace LinkShortener.Controllers
{
    public class SignUpController : Controller
    {
        private readonly ILogger<SignUpController> _logger;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IConfiguration _configuration;

        public SignUpController(ILogger<SignUpController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var connectionString = _configuration.GetValue<string>("ConnectionString:DefaultConnectionString");
            var mongoClient = new MongoClient(connectionString);
            _mongoDatabase = mongoClient.GetDatabase("LinkShortener");
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            var userCollection = _mongoDatabase.GetCollection<User>("users");
            var info = await userCollection.AsQueryable().FirstOrDefaultAsync(z => z.userName == user.userName);
            if (info == null)
            {
                info = new User
                {
                    userName = user.userName,
                    password = user.password,
                    admin = false,
                    Urls = 0
                };
                await userCollection.InsertOneAsync(info);
                return RedirectToAction("Index", "Login");
            }
            else
            {
                TempData["ErrorMes"] = "Kullanıcı adı zaten kayıtlı.";
                return View();
            }
        }
    }
}
