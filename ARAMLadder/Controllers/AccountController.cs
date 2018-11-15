using ARAMLadder.Data;
using ARAMLadder.Models;
using ARAMLadder.Models.LolModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace ARAMLadder.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IOptions<ApplicationConfiguration> options;
        private readonly UserManager<AramIdentityUser> userManager;
        public AccountController(UserManager<AramIdentityUser> userManager, IOptions<ApplicationConfiguration> options)
        {
            this.userManager = userManager;
            this.options = options;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.RiotName = (await userManager.FindByNameAsync(User.Identity.Name)).riotName;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRiotId(string riotName)
        {
            AramIdentityUser user;
            if (User != null && User.Identity != null)
            {
                user = await userManager.FindByNameAsync(User.Identity.Name);

                using (var httpClient = new HttpClient())
                {
                    var resp = await httpClient.GetAsync($"https://euw1.api.riotgames.com/lol/summoner/v3/summoners/by-name/{riotName}?api_key={options.Value.ApiRiotKey}");
                    if (resp.IsSuccessStatusCode)
                    {
                        PlayerDto player = JsonConvert.DeserializeObject<PlayerDto>(await resp.Content.ReadAsStringAsync());
                        user.riotId = player.accountId;
                        user.riotName = player.name;
                    }
                }
                await userManager.UpdateAsync(user);
            }
            ViewBag.RiotName = riotName;
            return View("Index");
        }
    }
}