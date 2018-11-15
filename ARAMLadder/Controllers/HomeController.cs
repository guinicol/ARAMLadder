using ARAMLadder.Data;
using ARAMLadder.Models;
using ARAMLadder.Models.LolModels;
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
        public HomeController(UserManager<AramIdentityUser> userManager, ApplicationDbContext dbContext, IOptions<ApplicationConfiguration> options)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.options = options;
        }
        public async Task<IActionResult> Index(int? week = null, DayOfWeek day = DayOfWeek.Monday)
        {

            var riotIds = dbContext.Users.Select(u => u.riotId).ToList();
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await userManager.FindByNameAsync(User.Identity.Name);
                if (currentUser.riotId != 0)
                {
                    var matchIds = new List<long>();
                    using (var httpClient = new HttpClient())
                    {
                        var recentMatch = dbContext.LoginGames
                                .Include(lg => lg.Games)
                                .Where(g => g.AramIdentityUser.riotId == currentUser.riotId)
                                .OrderByDescending(x => x.Games.GameCreation).FirstOrDefault();
                        var begintime = recentMatch != null ? $"&beginTime={recentMatch.Games.GameCreation + 1}" : "";
                        foreach (var riotId in riotIds)
                        {

                            var resp = await httpClient.GetAsync($"https://euw1.api.riotgames.com/lol/match/v3/matchlists/by-account/{riotId}?api_key={options.Value.ApiRiotKey}&queue=450{begintime}");
                            if (resp.IsSuccessStatusCode)
                            {
                                var matches = JsonConvert.DeserializeObject<ListMatchDto>(await resp.Content.ReadAsStringAsync());
                                matchIds.AddRange(matches.matches.Select(x => x.gameId));
                            }
                        }

                        matchIds = matchIds.GroupBy(x => x)
                          .Where(g => g.Count() > 2)
                          .Select(y => y.Key)
                          .ToList();

                        var matchCount = 1;
                        foreach (var matchId in matchIds)
                        {

                            matchCount++;
                            if (matchCount == 20)
                            {
                                Thread.Sleep(1000);
                                matchCount = 1;
                            }
                            var resp = await httpClient.GetAsync($"https://euw1.api.riotgames.com/lol/match/v3/matches/{matchId}?api_key={options.Value.ApiRiotKey}");
                            if (resp.IsSuccessStatusCode)
                            {
                                var matchDto = JsonConvert.DeserializeObject<MatchDto>(await resp.Content.ReadAsStringAsync());
                                var match = dbContext.Games.FirstOrDefault(x => x.GameId == matchDto.gameId);
                                if (match == null)
                                {
                                    match = new Game
                                    {
                                        GameCreation = matchDto.gameCreation,
                                        GameDuration = matchDto.gameDuration,
                                        GameId = matchDto.gameId
                                    };

                                }
                                dbContext.Attach(match);

                                var allyTeamCode = 0;
                                foreach (var participant in matchDto.participantIdentities)
                                {
                                    var user = dbContext.Users.FirstOrDefault(x => x.riotId == participant.player.accountId);
                                    if (user != null)
                                    {
                                        var player = matchDto.participants.First(x => x.participantId == participant.participantId);
                                        var stats = player.stats;
                                        if (allyTeamCode == 0)
                                            allyTeamCode = player.teamId;
                                        var test = !dbContext.LoginGames.Any(x => x.GamesId == match.Id && x.AramIdentityUserId == user.Id);
                                        if (!dbContext.LoginGames.Any(x => x.GamesId == match.Id && x.AramIdentityUserId == user.Id))
                                        {
                                            var loginGame = new LoginGame
                                            {
                                                Games = match,
                                                AramIdentityUser = user,
                                                Assists = stats.assists,
                                                Kills = stats.kills,
                                                Deaths = stats.deaths,
                                                DoubleKills = stats.doubleKills,
                                                TripleKills = stats.tripleKills,
                                                QuadraKills = stats.quadraKills,
                                                PentaKills = stats.pentaKills,
                                                FirstBloodKill = stats.firstBloodKill,
                                                Win = stats.win
                                            };
                                            dbContext.Add(loginGame);
                                        }
                                    }
                                }
                                if (match.KillAlly == 0 && match.KillEnnemy == 0)
                                {
                                    foreach (var participant in matchDto.participants)
                                    {
                                        if (participant.teamId == allyTeamCode)
                                            match.KillAlly += participant.stats.kills;
                                        else
                                            match.KillEnnemy += participant.stats.kills;
                                    }
                                }
                            }
                            await dbContext.SaveChangesAsync();
                        }

                    }
                }
            }
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
                .Where(x => startDate < x.GameCreation && x.GameCreation < endDate);
            var model = new HomeViewModel
            {
                Games = games.ToList(),
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
