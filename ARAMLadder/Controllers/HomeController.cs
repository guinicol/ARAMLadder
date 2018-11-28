using ARAMLadder.Data;
using ARAMLadder.Models;
using ARAMLadder.Models.LolModels;
using ARAMLadder.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ARAMLadder.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<AramIdentityUser> userManager;
        private ApplicationDbContext dbContext;
        private readonly IOptions<ApplicationConfiguration> options;
        private ILolStaticDataService lolStatic;
        public HomeController(UserManager<AramIdentityUser> userManager, ApplicationDbContext dbContext, IOptions<ApplicationConfiguration> options, ILolStaticDataService lolStatic)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.options = options;
            this.lolStatic = lolStatic;
        }
        public async Task<IActionResult> Index(int? week = null, DayOfWeek day = DayOfWeek.Monday)
        {

            
            var date = week == null ? DateTime.Today : FirstDateOfWeekISO8601(DateTime.Today.Year, week.Value);
            switch (day)
            {
                case DayOfWeek.Sunday:
                    date = date.AddDays(6);
                    break;
                case DayOfWeek.Saturday:
                    date = date.AddDays(5);
                    break;
                case DayOfWeek.Friday:
                    date = date.AddDays(4);
                    break;
                case DayOfWeek.Thursday:
                    date = date.AddDays(3);
                    break;
                case DayOfWeek.Wednesday:
                    date = date.AddDays(2);
                    break;
                case DayOfWeek.Tuesday:
                    date = date.AddDays(1);
                    break;
            }
            ViewBag.Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            ViewBag.Day = date.DayOfWeek;
            long startDate = new DateTimeOffset(date).ToUnixTimeMilliseconds();
            long endDate = new DateTimeOffset(date.AddDays(1)).ToUnixTimeMilliseconds();
            var games = dbContext.Games
                .Include(g => g.LoginGames)
                .ThenInclude(lg => lg.AramIdentityUser)
                .Include(g => g.LoginGames)
                .ThenInclude(lg => lg.Champion)
                .Include(g => g.LoginGames)
                .ThenInclude(lg => lg.Items)
                .ThenInclude(lgi => lgi.Item)
                .Include(g => g.LoginGames)
                .ThenInclude(lg => lg.Spells)
                .ThenInclude(lgi => lgi.Spell)
                .Include(g => g.LoginGames)
                .ThenInclude(lg => lg.Runes)
                .ThenInclude(lgi => lgi.Rune)
                .Where(x => startDate < x.GameCreation && x.GameCreation < endDate)
                .OrderByDescending(x => x.GameCreation)
                .ToList();
            var model = new HomeViewModel
            {
                Games = games,
                nbWin = games.Count(x => x.LoginGames.First().Win),
                nbLose = games.Count(x => !x.LoginGames.First().Win)
            };
            return View(model);
        }
        
        
        

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            // Use first Thursday in January to get first week of the year as
            // it will never be in Week 52/53
            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            // As we're adding days to a date in Week 1,
            // we need to subtract 1 in order to get the right date for week #1
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }

            // Using the first Thursday as starting week ensures that we are starting in the right year
            // then we add number of weeks multiplied with days
            var result = firstThursday.AddDays(weekNum * 7);

            // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
            return result.AddDays(-3);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }


}
