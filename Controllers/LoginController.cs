using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LinkShortener.Models;

namespace LinkShortener.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<User> _users;
        public LoginController(ILogger<LoginController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var connectionString = _configuration.GetValue<string>("ConnectionString:DefaultConnectionString");
            var mongoClient = new MongoClient(connectionString);
            _mongoDatabase = mongoClient.GetDatabase("LinkShortener");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            var userCollection = _mongoDatabase.GetCollection<User>("users");
            var infou = await userCollection.AsQueryable().FirstOrDefaultAsync(x => x.userName == user.userName && x.password == user.password);
            if (infou != null)
            {
                HttpContext.Session.SetString("admin", infou.admin.ToString());
                HttpContext.Session.SetString("user", infou.userName.ToString());
                HttpContext.Session.SetString("pass", infou.password.ToString());
                if (infou.admin == false)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, infou.userName)
                };
                    var useridentity = new ClaimsIdentity(claims, "Login");
                    ClaimsPrincipal principal = new ClaimsPrincipal(useridentity);
                    await HttpContext.SignInAsync(principal);
                    return RedirectToAction("Index", "Home");
                }
                else
                {

                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, infou.userName)
                };
                    var useridentity = new ClaimsIdentity(claims, "Login");
                    ClaimsPrincipal principal = new ClaimsPrincipal(useridentity);
                    await HttpContext.SignInAsync(principal);
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }
    }
}
