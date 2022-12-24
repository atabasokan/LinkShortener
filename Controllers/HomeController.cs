using LinkShortener.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LinkShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IConfiguration _configuration;
        private const string ServiceUrl = "https://localhost:5000";
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var connectionString = _configuration.GetValue<string>("ConnectionString:DefaultConnectionString");
            var mongoClient = new MongoClient(connectionString);
            _mongoDatabase = mongoClient.GetDatabase("LinkShortener");
        }
        [Authorize]

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string u)
        {
            var userUrlCollection = _mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
            var userUrl = userUrlCollection.AsQueryable().Where(x => x.User == HttpContext.Session.GetString("user"));
            var shortenedUrlCollection = _mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
            var shortenedUrl = await shortenedUrlCollection
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.ShortCode == u);

            if (shortenedUrl == null)
            {
                ViewBag.UserUrls = userUrl;
                return View();
            }
            return Redirect(shortenedUrl.OriginalUrl);
        }

        [HttpPost]
        public async Task<IActionResult> ShortenUrl(string longUrl, string speChar, int lastDay, string user)
        {
            user = HttpContext.Session.GetString("user");
            var shortenedUrlCollection = _mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
            var originalUrlColleciton = _mongoDatabase.GetCollection<OriginalUrl>("original-urls");
            var usersCollection = _mongoDatabase.GetCollection<User>("users");

            var shortenedUrl = await shortenedUrlCollection.AsQueryable().FirstOrDefaultAsync(x => x.OriginalUrl == longUrl);
            var originalUrl = await originalUrlColleciton.AsQueryable().FirstOrDefaultAsync(x => x.title == longUrl);
            var userInfo = await usersCollection.AsQueryable().FirstOrDefaultAsync(x => x.userName == user);

            if (shortenedUrl != null)
            {
                if (shortenedUrl.User != user)
                {
                    string shortCode = GetRandomAlphanumericString();
                    if (speChar == null)
                    {
                        shortenedUrl = new ShortenedUrl
                        {
                            CreatedAt = DateTime.UtcNow,
                            OriginalUrl = longUrl,
                            ShortCode = shortCode,
                            SpeChar = "",
                            LastDay = DateTime.UtcNow.AddDays(lastDay),
                            ShortUrl = $"{ServiceUrl}/{shortCode}",
                            User = user
                        };


                    }
                    else
                    {
                        shortenedUrl = new ShortenedUrl
                        {
                            CreatedAt = DateTime.UtcNow,
                            OriginalUrl = longUrl,
                            ShortCode = shortCode,
                            SpeChar = speChar,
                            LastDay = DateTime.UtcNow.AddDays(lastDay),
                            ShortUrl = $"{ServiceUrl}/{speChar}",
                            User = user
                        };
                    }

                    originalUrl.Click += 1;
                    var filter = Builders<OriginalUrl>.Filter.Eq(s => s.title, shortenedUrl.OriginalUrl);
                    var update = Builders<OriginalUrl>.Update.Set(s => s.Click, originalUrl.Click);
                    originalUrlColleciton.UpdateOneAsync(filter, update);
                    await shortenedUrlCollection.InsertOneAsync(shortenedUrl);
                }
                else
                {
                    originalUrl.Click += 1;
                    var filter = Builders<OriginalUrl>.Filter.Eq(s => s.title, shortenedUrl.OriginalUrl);
                    var update = Builders<OriginalUrl>.Update.Set(s => s.Click, originalUrl.Click);
                    originalUrlColleciton.UpdateOneAsync(filter, update);
                }
            }
            else
            {
                string shortCode = GetRandomAlphanumericString();
                if (speChar == null)
                {
                    shortenedUrl = new ShortenedUrl
                    {
                        CreatedAt = DateTime.UtcNow,
                        OriginalUrl = longUrl,
                        ShortCode = shortCode,
                        SpeChar = "",
                        LastDay = DateTime.UtcNow.AddDays(lastDay),
                        ShortUrl = $"{ServiceUrl}/{shortCode}",
                        User = user
                    };
                    originalUrl = new OriginalUrl
                    {
                        title = longUrl,
                        Click = 1
                    };
                    userInfo.Urls += 1;
                    var filter = Builders<User>.Filter.Eq(s => s.userName, user);
                    var update = Builders<User>.Update.Set(s => s.Urls, userInfo.Urls);
                    usersCollection.UpdateOneAsync(filter, update);

                }
                else
                {
                    shortenedUrl = new ShortenedUrl
                    {
                        CreatedAt = DateTime.UtcNow,
                        OriginalUrl = longUrl,
                        ShortCode = shortCode,
                        SpeChar = speChar,
                        LastDay = DateTime.UtcNow.AddDays(lastDay),
                        ShortUrl = $"{ServiceUrl}/{speChar}",
                        User = user
                    };
                    originalUrl = new OriginalUrl
                    {
                        title = longUrl,
                        Click = 1
                    };
                    userInfo.Urls += 1;
                    var filter = Builders<User>.Filter.Eq(s => s.userName, user);
                    var update = Builders<User>.Update.Set(s => s.Urls, userInfo.Urls);
                    usersCollection.UpdateOneAsync(filter, update);
                }
                await originalUrlColleciton.InsertOneAsync(originalUrl);
                await shortenedUrlCollection.InsertOneAsync(shortenedUrl);
            }

            return View(shortenedUrl);
        }
        public static string GetRandomAlphanumericString()
        {
            Random random = new Random();
            const string alphanumericCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789";
            return new string(alphanumericCharacters.Select(c => alphanumericCharacters[random.Next(alphanumericCharacters.Length)]).Take(6).ToArray());
        }
    }
}