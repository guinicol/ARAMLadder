using ARAMLadder.Data;
using ARAMLadder.Models;
using ARAMLadder.Models.LolModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace ARAMLadder.Controllers
{
    [Authorize]
    public class MatchController : Controller
    {
        private readonly UserManager<AramIdentityUser> userManager;
        private readonly ApplicationDbContext dbContext;
        public MatchController(UserManager<AramIdentityUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }
        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
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
    }
}