using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARAMLadder.Data;
using ARAMLadder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARAMLadder.Controllers
{
    public class LadderController : Controller
    {
        private UserManager<AramIdentityUser> userManager;
        private ApplicationDbContext dbContext;
        public LadderController(UserManager<AramIdentityUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }
        public IActionResult Index()
        {
            var ranking = new List<RankingViewModel>();
            var rank = dbContext.LoginGames.Include(x => x.AramIdentityUser).OrderByDescending(x => x.Games.GameCreation).GroupBy(x => x.AramIdentityUserId);
            foreach (var item in rank)
            {
                var user = item.First();
                ranking.Add(new RankingViewModel
                {
                    Score = user.Score,
                    Name = user.AramIdentityUser.riotName,
                    RiotId = user.AramIdentityUser.riotId
                });
            }

            return View(ranking.OrderByDescending(x => x.Score));
        }
    }
}