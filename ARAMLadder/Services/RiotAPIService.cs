using ARAMLadder.Data;
using ARAMLadder.Models;
using ARAMLadder.Models.LolModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ARAMLadder.Services
{
    public class RiotAPIService : IRiotAPIService
    {
        //private readonly UserManager<AramIdentityUser> userManager;
        private readonly IServiceScopeFactory scopeFactory;
        //private readonly ApplicationDbContext dbContext;
        //private IOptions<ApplicationConfiguration> options;
        //private ILolStaticDataService lolStatic;
        public RiotAPIService(IServiceScopeFactory scopeFactory
            //ApplicationDbContext dbContext,
            //IOptions<ApplicationConfiguration> options,
            //ILolStaticDataService lolStatic,
            //UserManager<AramIdentityUser> userManager
            )
        {
            this.scopeFactory = scopeFactory;
            //this.dbContext = dbContext;
            //this.options = options;
            //this.lolStatic = lolStatic;
            //this.userManager = userManager;
        }
        public bool IsResfreshingMatch()
        {
            return SemaphoreSlim.CurrentCount == 0;
        }
        private static SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);
        public async Task RefreshMatch(string userName, bool refreshAll = false)
        {
            if (SemaphoreSlim.CurrentCount != 0)
            {
                SemaphoreSlim.Wait();
                using (var scope = scopeFactory.CreateScope()) {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AramIdentityUser>>();
                    var lolStatic = scope.ServiceProvider.GetRequiredService<ILolStaticDataService>();
                    var options = scope.ServiceProvider.GetRequiredService<IOptions<ApplicationConfiguration>>();
                    try
                    {
                        var riotIds = dbContext.Users.Select(u => u.riotId).ToList();
                        var currentUser = await userManager.FindByNameAsync(userName);
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

                                    var resp = await httpClient.GetAsync($"https://euw1.api.riotgames.com/lol/match/v4/matchlists/by-account/{riotId}?api_key={options.Value.ApiRiotKey}&queue=450{begintime}");
                                    if (resp.IsSuccessStatusCode)
                                    {
                                        var matches = JsonConvert.DeserializeObject<ListMatchDto>(await resp.Content.ReadAsStringAsync());
                                        matchIds.AddRange(matches.matches.Select(x => x.gameId));
                                    }
                                }

                                var matchCount = 1;
                                foreach (var matchId in matchIds)
                                {

                                    matchCount++;
                                    if (matchCount == 20)
                                    {
                                        Thread.Sleep(1000);
                                        matchCount = 1;
                                    }
                                    var resp = await httpClient.GetAsync($"https://euw1.api.riotgames.com/lol/match/v4/matches/{matchId}?api_key={options.Value.ApiRiotKey}");
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
                                                    var lastGame = dbContext.LoginGames.OrderByDescending(x => x.Games.GameCreation).FirstOrDefault(x => x.AramIdentityUserId == user.Id);

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
                                                        Level = stats.champLevel,
                                                        Champion = champ
                                                    };
                                                    var elo = GetEloCalc(lastGame, loginGame);
                                                    loginGame.WinStreak = elo.winStreak;
                                                    loginGame.LoseStreak = elo.loseStreak;
                                                    loginGame.PointWin = elo.pointWin;
                                                    loginGame.PointLose = elo.pointLose;
                                                    loginGame.Score = elo.score;
                                                    var items = await GetItemsFromStatsAsync(dbContext, lolStatic, stats);
                                                    foreach (var item in items)
                                                    {
                                                        var lgItem = new LoginGameItem
                                                        {
                                                            Item = item,
                                                            LoginGame = loginGame
                                                        };
                                                        await dbContext.AddAsync(lgItem);

                                                    }
                                                    dbContext.Add(loginGame);
                                                    var runes = await GetRunesFromStatsAsync(dbContext, lolStatic, loginGame, stats);
                                                    dbContext.AddRange(runes);
                                                    var spells = await GetSpellsFromParticipantAsync(dbContext, lolStatic, loginGame, player);
                                                    dbContext.AddRange(spells);
                                                    dbContext.SaveChanges();
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
                    finally
                    {
                        SemaphoreSlim.Release();
                    }
                }

                UpdateElo();
            }
        }
        private async Task<List<Item>> GetItemsFromStatsAsync(ApplicationDbContext dbContext, ILolStaticDataService lolStatic, Stats stats)
        {
            var items = new List<Item>();
            var item0 = await GetItemFromIdAsync(dbContext, lolStatic, stats.item0);
            if (item0 != null)
                items.Add(item0);
            var item1 = await GetItemFromIdAsync(dbContext, lolStatic, stats.item1);
            if (item1 != null)
                items.Add(item1);
            var item2 = await GetItemFromIdAsync(dbContext, lolStatic, stats.item2);
            if (item2 != null)
                items.Add(item2);
            var item3 = await GetItemFromIdAsync(dbContext, lolStatic, stats.item3);
            if (item3 != null)
                items.Add(item3);
            var item4 = await GetItemFromIdAsync(dbContext, lolStatic, stats.item4);
            if (item4 != null)
                items.Add(item4);
            var item5 = await GetItemFromIdAsync(dbContext, lolStatic, stats.item5);
            if (item5 != null)
                items.Add(item5);
            var item6 = await GetItemFromIdAsync(dbContext, lolStatic, stats.item6);
            if (item6 != null)
                items.Add(item6);
            return items;
        }
        private async Task<List<LoginGameRune>> GetRunesFromStatsAsync(ApplicationDbContext dbContext, ILolStaticDataService lolStatic, LoginGame lg, Stats stats)
        {
            var runes = new List<LoginGameRune>();
            var rune = await GetRuneFromIdAsync(dbContext, lolStatic, stats.perkPrimaryStyle);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 0
                });
            rune = await GetRuneFromIdAsync(dbContext, lolStatic, stats.perkPrimaryStyle, stats.perk0);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 1
                });
            rune = await GetRuneFromIdAsync(dbContext, lolStatic, stats.perkPrimaryStyle, stats.perk1);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 2
                });
            if (rune != null)
                rune = await GetRuneFromIdAsync(dbContext, lolStatic, stats.perkPrimaryStyle, stats.perk2);
            runes.Add(new LoginGameRune
            {
                Rune = rune,
                LoginGame = lg,
                Position = 3
            });
            rune = await GetRuneFromIdAsync(dbContext, lolStatic, stats.perkPrimaryStyle, stats.perk3);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 4
                });
            rune = await GetRuneFromIdAsync(dbContext, lolStatic, stats.perkSubStyle);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 5
                });
            rune = await GetRuneFromIdAsync(dbContext, lolStatic, stats.perkSubStyle, stats.perk4);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 6
                });
            rune = await GetRuneFromIdAsync(dbContext, lolStatic, stats.perkSubStyle, stats.perk5);
            if (rune != null)
                runes.Add(new LoginGameRune
                {
                    Rune = rune,
                    LoginGame = lg,
                    Position = 7
                });
            return runes;
        }
        private async Task<List<LoginGameSpell>> GetSpellsFromParticipantAsync(ApplicationDbContext dbContext, ILolStaticDataService lolStatic, LoginGame lg, Participant participant)
        {
            var spells = new List<LoginGameSpell>();

            var spell = await GetSpellFromIdAsync(dbContext, lolStatic, participant.spell1Id);
            if (spell != null)
                spells.Add(new LoginGameSpell
                {
                    Spell = spell,
                    LoginGame = lg,
                    Position = 1
                });
            spell = await GetSpellFromIdAsync(dbContext, lolStatic, participant.spell2Id);
            if (spell != null)
                spells.Add(new LoginGameSpell
                {
                    Spell = spell,
                    LoginGame = lg,
                    Position = 2
                });

            return spells;
        }

        private async Task<Item> GetItemFromIdAsync(ApplicationDbContext dbContext, ILolStaticDataService lolStatic, int id)
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
        private async Task<Spell> GetSpellFromIdAsync(ApplicationDbContext dbContext, ILolStaticDataService lolStatic, int id)
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
        private async Task<Rune> GetRuneFromIdAsync(ApplicationDbContext dbContext, ILolStaticDataService lolStatic, int famillyId, int? id = null)
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
                    dbContext.SaveChanges();
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
            return rune;
        }
        public void UpdateElo()
        {
            if (SemaphoreSlim.CurrentCount != 0)
            {
                SemaphoreSlim.Wait();
                try
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var games = dbContext.LoginGames
                        .Include(x => x.Games).GroupBy(x => x.AramIdentityUserId);
                        foreach (var localGame in games)
                        {
                            var userGame = localGame.OrderBy(x => x.Games.GameCreation);
                            LoginGame lastGame = null;
                            foreach (var item in userGame)
                            {
                                var elo = GetEloCalc(lastGame, item);
                                item.LoseStreak = elo.loseStreak;
                                item.WinStreak = elo.winStreak;
                                item.PointLose = elo.pointLose;
                                item.PointWin = elo.pointWin;
                                item.Score = elo.score;
                                dbContext.Update(item);
                                lastGame = item;
                            }
                        }
                        dbContext.SaveChanges();
                    }
                }
                finally
                {
                    SemaphoreSlim.Release();
                }
            }
        }
        private EloCalc GetEloCalc(LoginGame lastGame, LoginGame newGame)
        {
            var EloCalc = new EloCalc
            {
                winStreak = 0,
                loseStreak = 0,
                pointWin = lastGame != null ? lastGame.PointWin : 20,
                pointLose = lastGame != null ? lastGame.PointLose : 20,
                score = lastGame != null ? lastGame.Score : 500
            };


            if (newGame.Win)
            {
                EloCalc.winStreak = lastGame != null ? lastGame.WinStreak + 1 : 1;
                EloCalc.pointWin += (EloCalc.winStreak % 2 == 0 ? 1 : 0);
                EloCalc.pointLose -= (EloCalc.winStreak % 2 == 0 ? 1 : 0);
                //Cap Max
                EloCalc.pointWin = EloCalc.pointWin <= 30 ? EloCalc.pointWin : 30;
                EloCalc.pointLose = EloCalc.pointLose <= 30 ? EloCalc.pointLose : 30;
                //Cap Min
                EloCalc.pointWin = EloCalc.pointWin >= 10 ? EloCalc.pointWin : 10;
                EloCalc.pointLose = EloCalc.pointLose >= 10 ? EloCalc.pointLose : 10;

                //Fin Cap
                EloCalc.score += EloCalc.pointWin;

                EloCalc.score += newGame.DoubleKills / 2;
                EloCalc.score += newGame.TripleKills;
                EloCalc.score += newGame.QuadraKills * 2;
                EloCalc.score += newGame.PentaKills * 3;
                EloCalc.score += newGame.FirstBloodKill ? 3 : 0;


            }
            else
            {
                EloCalc.loseStreak = lastGame != null ? lastGame.LoseStreak + 1 : 1;
                EloCalc.pointWin -= (EloCalc.loseStreak % 2 == 0 ? 1 : 0);
                EloCalc.pointLose += (EloCalc.loseStreak % 2 == 0 ? 1 : 0);
                EloCalc.score -= EloCalc.pointLose;
                EloCalc.score += newGame.DoubleKills / 2;
                EloCalc.score += newGame.TripleKills;
                EloCalc.score += newGame.QuadraKills * 2;
                EloCalc.score += newGame.PentaKills * 3;
                EloCalc.score += newGame.FirstBloodKill ? 3 : 0;
            }
            return EloCalc;
        }
        private class EloCalc
        {
            internal int winStreak;
            internal int loseStreak;
            internal int pointWin;
            internal int pointLose;
            internal int score;
        }
    }
}
