using ARAMLadder.Data;
using ARAMLadder.Models;
using ARAMLadder.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace ARAMLadder.Controllers
{
    [Authorize]
    public class MatchController : Controller
    {
        private readonly UserManager<AramIdentityUser> userManager;
        private readonly ApplicationDbContext dbContext;
        private IOptions<ApplicationConfiguration> options;
        private ILolStaticDataService lolStatic;
        private IRiotAPIService riotAPI;
        private readonly IBackgroundTaskQueue queue;
        private readonly IServiceProvider services;

        public MatchController(UserManager<AramIdentityUser> userManager,
            ApplicationDbContext dbContext,
            IOptions<ApplicationConfiguration> options,
            ILolStaticDataService lolStatic,
            IRiotAPIService riotAPI,
            IBackgroundTaskQueue queue,
            IServiceProvider services)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.options = options;
            this.lolStatic = lolStatic;
            this.riotAPI = riotAPI;
            this.queue = queue;
            this.services = services;
        }
        public async System.Threading.Tasks.Task<IActionResult> Index(int? id = null)
        {
            AramIdentityUser user = null;
            if (id != null)
                user = await dbContext.Users.FirstOrDefaultAsync(x => x.riotId == id);
            if (user == null)
                user = await userManager.FindByNameAsync(User.Identity.Name);

            var matches = dbContext.LoginGames
                .Include(lg => lg.Games)
                .Include(lg => lg.Champion)
                .Include(lg => lg.Items)
                .ThenInclude(lgi => lgi.Item)
                .Include(lg => lg.Spells)
                .ThenInclude(lgi => lgi.Spell)
                .Include(lg => lg.Runes)
                .ThenInclude(lgi => lgi.Rune)
                .OrderByDescending(x => x.Games.GameCreation)
                .Where(g => g.AramIdentityUserId == user.Id);
            return View(matches);

        }
        public void LaunchRefreshMatch(bool refreshAll = false)
        {
            if (User.Identity.IsAuthenticated)
            {
                queue.QueueBackgroundWorkItem(async token =>
                {
                    await riotAPI.RefreshMatch(User.Identity.Name, refreshAll);
                });
            }
        }
        public bool IsResfreshingMatch()
        {
            return riotAPI.IsResfreshingMatch();
        }




        //public IActionResult UpdateElo()
        //{
        //    var games = dbContext.LoginGames
        //        .Include(x => x.Games).GroupBy(x => x.AramIdentityUserId);
        //    foreach (var localGame in games)
        //    {
        //        var userGame = localGame.OrderBy(x => x.Games.GameCreation);
        //        LoginGame lastGame = null;
        //        foreach (var item in userGame)
        //        {
        //            var elo = GetEloCalc(lastGame, item);
        //            item.LoseStreak = elo.loseStreak;
        //            item.WinStreak = elo.winStreak;
        //            item.PointLose = elo.pointLose;
        //            item.PointWin = elo.pointWin;
        //            item.Score = elo.score;
        //            dbContext.Update(item);
        //            lastGame = item;
        //        }
        //    }
        //    dbContext.SaveChanges();
        //    return Redirect("Index");
        //}
    }
}