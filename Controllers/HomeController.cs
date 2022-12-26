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
        private const string ServiceUrl = "http://linkshortener-dev.eba-csrgruii.eu-central-1.elasticbeanstalk.com/";
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var connectionString = _configuration.GetValue<string>("ConnectionString:DefaultConnectionString");
            var mongoClient = new MongoClient(connectionString);
            _mongoDatabase = mongoClient.GetDatabase("LinkShortener");
        }
        [Authorize]
        public async Task<IActionResult> Index(string user)
        {
            user = HttpContext.Session.GetString("user");
            var userUrlCollection = _mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
            var userUrl = userUrlCollection.AsQueryable().Where(x => x.User == user);
            var links = await userUrlCollection.Find(Builders<ShortenedUrl>.Filter.Eq(x => x.User, user)).ToListAsync();
            if (user != null)
            {
                foreach (var item in userUrl)
                {
                    if (item.LastDay < DateTime.Now)
                    {
                        var filter = Builders<ShortenedUrl>.Filter.And(
                        Builders<ShortenedUrl>.Filter.Where(x => x.User == item.User),
                        Builders<ShortenedUrl>.Filter.Where(x => x.OriginalUrl == item.OriginalUrl)
                        );
                        userUrlCollection.DeleteOneAsync(filter);
                    }
                }
                ViewBag.UserUrls = userUrl;
                return View();
            }

            return RedirectToAction("LogOut", "Login");

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
                            User = user,
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
                            User = user,
                            Click = 1
                        };
                        userInfo.Urls += 1;
                        var filter = Builders<User>.Filter.Eq(s => s.userName, user);
                        var update = Builders<User>.Update.Set(s => s.Urls, userInfo.Urls);
                        usersCollection.UpdateOneAsync(filter, update);
                    }
                    await shortenedUrlCollection.InsertOneAsync(shortenedUrl);
                }
                else
                {
                    userInfo.Urls += 1;
                    var filter = Builders<User>.Filter.Eq(s => s.userName, user);
                    var update = Builders<User>.Update.Set(s => s.Urls, userInfo.Urls);
                    usersCollection.UpdateOneAsync(filter, update);
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
                        User = user,
                        Click = 1
                    };
                    originalUrl = new OriginalUrl
                    {
                        title = longUrl
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
                        User = user,
                        Click = 1
                    };
                    originalUrl = new OriginalUrl
                    {
                        title = longUrl
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
        public async Task<IActionResult> ClickCounter(string orgUrl)
        {
            string user = HttpContext.Session.GetString("user");
            var shortenedUrlCollection = _mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
            var shortenedUrl = await shortenedUrlCollection.AsQueryable().FirstOrDefaultAsync(x => x.OriginalUrl == orgUrl && x.User == user);
            shortenedUrl.Click += 1;
            var filter = Builders<ShortenedUrl>.Filter.And
                (
                    Builders<ShortenedUrl>.Filter.Where(x => x.User == user),
                    Builders<ShortenedUrl>.Filter.Where(x => x.OriginalUrl == orgUrl)
                );
            var update = Builders<ShortenedUrl>.Update.Set(s => s.Click, shortenedUrl.Click);
            shortenedUrlCollection.UpdateOneAsync(filter, update);
            return Redirect(shortenedUrl.OriginalUrl);
        }
    }
}