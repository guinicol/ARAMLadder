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

            var riotIds = dbContext.Users.Select(u => u.riotId).ToList();
            if (User.Identity.IsAuthenticated && week == null)
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
                                            var champ = dbContext.Champions.FirstOrDefault(x => x.Id == player.championId);
                                            if (champ == null)
                                            {
                                                var champData = await lolStatic.GetChampionAsync(player.championId);
                                                champ = new Champion()
                                                {
                                                    Icon = champData.image.full,
                                                    Key = champData.id,
                                                    Id = int.Parse(champData.key),
                                                    Name = champData.name
                                                };
                                                dbContext.Add(champ);
                                                dbContext.SaveChanges();
                                            }
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
                                                Win = stats.win,
                                                Level=stats.champLevel,
                                                Champion = champ
                                            };
                                            var items = await GetItemsFromStatsAsync(stats);
                                            foreach (var item in items)
                                            {
                                                var lgItem = new LoginGameItem
                                                {
                                                    Item = item,
                                                    LoginGame = loginGame
                                                };
                                                await dbContext.AddAsync(lgItem);

                                            }
                                            await dbContext.AddAsync(loginGame);
                                            var runes = await GetRunesFromStatsAsync(loginGame, stats);
                                            await dbContext.AddRangeAsync(runes);
                                            var spells = await GetSpellsFromParticipantAsync(loginGame, player);
                                            await dbContext.AddRangeAsync(spells);
                                            await dbContext.SaveChangesAsync();
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
        private async Task<List<Item>> GetItemsFromStatsAsync(Stats stats)
        {
            var items = new List<Item>();
            var item0 = await GetItemFromIdAsync(stats.item0);
            if (item0 != null)
                items.Add(item0);
            var item1 = await GetItemFromIdAsync(stats.item1);
            if (item1 != null)
                items.Add(item1);
            var item2 = await GetItemFromIdAsync(stats.item2);
            if (item2 != null)
                items.Add(item2);
            var item3 = await GetItemFromIdAsync(stats.item3);
            if (item3 != null)
                items.Add(item3);
            var item4 = await GetItemFromIdAsync(stats.item4);
            if (item4 != null)
                items.Add(item4);
            var item5 = await GetItemFromIdAsync(stats.item5);
            if (item5 != null)
                items.Add(item5);
            var item6 = await GetItemFromIdAsync(stats.item6);
            if (item6 != null)
                items.Add(item6);
            return items;
        }
        private async Task<List<LoginGameRune>> GetRunesFromStatsAsync(LoginGame lg, Stats stats)
        {
            var runes = new List<LoginGameRune>();
            var rune = await GetRuneFromIdAsync(stats.perkPrimaryStyle);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 0
                });
            rune = await GetRuneFromIdAsync(stats.perkPrimaryStyle, stats.perk0);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 1
                });
            rune = await GetRuneFromIdAsync(stats.perkPrimaryStyle, stats.perk1);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 2
                });
            if (rune != null)
                rune = await GetRuneFromIdAsync(stats.perkPrimaryStyle, stats.perk2);
            runes.Add(new LoginGameRune
            {
                Rune = rune,
                LoginGame = lg,
                Position = 3
            });
            rune = await GetRuneFromIdAsync(stats.perkPrimaryStyle, stats.perk3);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 4
                });
            rune = await GetRuneFromIdAsync(stats.perkSubStyle);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 5
                });
            rune = await GetRuneFromIdAsync(stats.perkSubStyle, stats.perk4);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 6
                });
            rune = await GetRuneFromIdAsync(stats.perkSubStyle, stats.perk5);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 7
                });
            return runes;
        }
        private async Task<List<LoginGameSpell>> GetSpellsFromParticipantAsync(LoginGame lg, Participant participant)
        {
            var spells = new List<LoginGameSpell>();

            var spell = await GetSpellFromIdAsync(participant.spell1Id);
            if (spell != null)
                spells.Add(new LoginGameSpell
                {
                    Spell = spell,
                    LoginGame = lg,
                    Position = 1
                });
            spell = await GetSpellFromIdAsync(participant.spell2Id);
            if (spell != null)
                spells.Add(new LoginGameSpell
                {
                    Spell = spell,
                    LoginGame = lg,
                    Position = 2
                });

            return spells;
        }

        private async Task<Item> GetItemFromIdAsync(int id)
        {
            var item = dbContext.Items.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                try
                {
                    var itemData = await lolStatic.GetItemAsync(id);
                    item = new Item
                    {
                        Icon = itemData.image.full,
                        Id = id,
                        Name = itemData.name
                    };
                    dbContext.Add(item);
                    await dbContext.SaveChangesAsync();
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
            return item;
        }
        private async Task<Spell> GetSpellFromIdAsync(int id)
        {
            var spell = dbContext.Spells.FirstOrDefault(x => x.Id == id);
            if (spell == null)
            {
                try
                {
                    var itemData = await lolStatic.GetSpellAsync(id);
                    spell = new Spell
                    {
                        Icon = itemData.image.full,
                        Id = id,
                        Name = itemData.name
                    };
                    dbContext.Add(spell);
                    await dbContext.SaveChangesAsync();
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
            return spell;
        }
        private async Task<Rune> GetRuneFromIdAsync(int famillyId, int? id = null)
        {
            var rune = dbContext.Runes.FirstOrDefault(x => x.Id == (id ?? famillyId));
            if (rune == null)
            {
                try
                {
                    var runeData = await lolStatic.GetRuneAsync(famillyId, id);
                    rune = new Rune
                    {
                        Icon = runeData.icon,
                        Id = runeData.id,
                        Name = runeData.name
                    };
                    dbContext.Add(rune);
                    await dbContext.SaveChangesAsync();
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
            return rune;
        }

        public async Task<IActionResult> UpdateChampId()
        {
            var games = dbContext.Games
                .Include(x => x.LoginGames)
                .ThenInclude(lg => lg.Items)
                .Include(x => x.LoginGames)
                .ThenInclude(lg => lg.Runes)
                .Include(x => x.LoginGames)
                .ThenInclude(lg => lg.Spells);
            using (HttpClient httpClient = new HttpClient())
            {
                foreach (var game in games)
                {
                    var resp = await httpClient.GetAsync($"https://euw1.api.riotgames.com/lol/match/v3/matches/{game.GameId}?api_key={options.Value.ApiRiotKey}");
                    if (resp.IsSuccessStatusCode)
                    {
                        var matchDto = JsonConvert.DeserializeObject<MatchDto>(await resp.Content.ReadAsStringAsync());
                        foreach (var p in matchDto.participantIdentities)
                        {
                            var user = dbContext.Users.FirstOrDefault(x => x.riotId == p.player.accountId);
                            if (user != null)
                            {
                                var lg = game.LoginGames.First(x => x.AramIdentityUserId == user.Id);
                                var player = matchDto.participants.First(x => x.participantId == p.participantId);
                                var champ = dbContext.Champions.FirstOrDefault(x => x.Id == player.championId);
                                if (champ == null)
                                {
                                    var champData = await lolStatic.GetChampionAsync(player.championId);
                                    champ = new Champion()
                                    {
                                        Icon = champData.image.full,
                                        Key = champData.id,
                                        Id = int.Parse(champData.key),
                                        Name = champData.name
                                    };
                                    dbContext.Add(champ);
                                    dbContext.SaveChanges();
                                }
                                lg.Champion = champ;
                                lg.Level = player.stats.champLevel;

                                if (lg.Items.Count == 0)
                                {
                                    var items = await GetItemsFromStatsAsync(player.stats);
                                    foreach (var item in items)
                                    {
                                        var lgItem = new LoginGameItem
                                        {
                                            Item = item,
                                            LoginGame = lg
                                        };
                                        dbContext.Add(lgItem);
                                    }
                                }
                                if (lg.Runes.Count == 0)
                                {
                                    var runes = await GetRunesFromStatsAsync(lg, player.stats);
                                    await dbContext.AddRangeAsync(runes);
                                }
                                if (lg.Spells.Count == 0)
                                {
                                    var spells = await GetSpellsFromParticipantAsync(lg, player);
                                    await dbContext.AddRangeAsync(spells);
                                }
                                dbContext.Update(lg);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
            await dbContext.SaveChangesAsync();
            return Redirect("Index");
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
