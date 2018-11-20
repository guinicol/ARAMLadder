using ARAMLadder.Data;
using ARAMLadder.Models.LolModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ARAMLadder.Services
{
    public class LolStaticDataService : ILolStaticDataService
    {
        private IOptions<ApplicationConfiguration> options;

        private RealmsDto realms;
        private const string dragonBaseUrl = "https://ddragon.leagueoflegends.com/";


        public LolStaticDataService(IOptions<ApplicationConfiguration> options)
        {
            this.options = options;
            var t = ConfigureAsync();
            t.Wait();
        }
        public async Task ConfigureAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var resp = await httpClient.GetAsync($"{dragonBaseUrl}realms/euw.json");
                if (resp.IsSuccessStatusCode)
                {
                    realms = JsonConvert.DeserializeObject<RealmsDto>(await resp.Content.ReadAsStringAsync());
                }
                resp = await httpClient.GetAsync($"{dragonBaseUrl}api/versions.json");
                if(resp.IsSuccessStatusCode)
                {
                    var v = JsonConvert.DeserializeObject<List<string>>(await resp.Content.ReadAsStringAsync());
                    var lastVersion = v.First();
                    realms.n.champion = lastVersion;
                    realms.n.rune = lastVersion;
                    realms.n.profileicon = lastVersion;
                    realms.n.item = lastVersion;
                    realms.n.summoner = lastVersion;
                }
            }
        }

        public async Task<ProfileIconDto> GetProfileIconAsync(int id)
        {
            ProfileIconDto profileIcon=null;
            using (var httpClient = new HttpClient())
            {
                var resp = await httpClient.GetAsync($"{realms.cdn}/{realms.n.champion}/data/{realms.l}/profileicon.json");
                if (resp.IsSuccessStatusCode)
                {
                    var profileIcons = JsonConvert.DeserializeObject<DdragonDto<ProfileIconDto>>(await resp.Content.ReadAsStringAsync()).data;
                    profileIcon = profileIcons[id.ToString()];

                }
            }
            return profileIcon;
        }

        public async Task<ChampionDto> GetChampionAsync(int id)
        {
            ChampionDto returnChamp = null;
            using (var httpClient = new HttpClient())
            {
                var resp = await httpClient.GetAsync($"{realms.cdn}/{realms.n.champion}/data/{realms.l}/champion.json");
                if (resp.IsSuccessStatusCode)
                {
                    var champs = JsonConvert.DeserializeObject<DdragonDto<ChampionDto>>(await resp.Content.ReadAsStringAsync()).data;
                    returnChamp = champs.FirstOrDefault(x => x.Value.key == id.ToString()).Value;

                }
            }
            return returnChamp;
        }
        public async Task<ItemDto> GetItemAsync(int id)
        {
            ItemDto returnItem = null;
            using (var httpClient = new HttpClient())
            {
                var resp = await httpClient.GetAsync($"{realms.cdn}/{realms.n.champion}/data/{realms.l}/item.json");
                if (resp.IsSuccessStatusCode)
                {
                    var items = JsonConvert.DeserializeObject<DdragonDto<ItemDto>>(await resp.Content.ReadAsStringAsync()).data;
                    returnItem = items[id.ToString()];

                }
            }
            return returnItem;
        }

        public string GetProfileIconPath(string fileName)
        {
            return $"{realms.cdn}/{realms.n.champion}/img/profileicon/{fileName}";
        }

        public string GetChampionIconPath(string fileName)
        {
            return $"{realms.cdn}/{realms.n.profileicon}/img/champion/{fileName}";

        }
        public string GetItemIconPath(string fileName)
        {
            return $"{realms.cdn}/{realms.n.item}/img/item/{fileName}";

        }

        public async Task<RuneDto> GetRuneAsync(int familleId, int? id=null)
        {
            RuneDto returnItem = null;
            using (var httpClient = new HttpClient())
            {
                var resp = await httpClient.GetAsync($"{realms.cdn}/{realms.n.item}/data/{realms.l}/runesReforged.json");
                if (resp.IsSuccessStatusCode)
                {
                    var runeData = JsonConvert.DeserializeObject<List<RunesDto>>(await resp.Content.ReadAsStringAsync());
                    var familleRune = runeData.FirstOrDefault(x => x.id == familleId);
                    if (familleRune != null)
                    {
                        foreach (var rune in familleRune.slots)
                        {
                            returnItem = rune.runes.FirstOrDefault(x => x.id == id);
                            if (returnItem != null)
                                return returnItem;
                        }
                        returnItem = new RuneDto
                        {
                            icon = familleRune.icon,
                            id = familleRune.id,
                            key = familleRune.key,
                            name = familleRune.name
                        };
                    }
                }
            }
            return returnItem;
        }

        public async Task<SpellDto> GetSpellAsync(int id)
        {
            SpellDto returnItem = null;
            using (var httpClient = new HttpClient())
            {
                var resp = await httpClient.GetAsync($"{realms.cdn}/{realms.n.champion}/data/{realms.l}/summoner.json");
                if (resp.IsSuccessStatusCode)
                {
                    var items = JsonConvert.DeserializeObject<DdragonDto<SpellDto>>(await resp.Content.ReadAsStringAsync()).data;
                    returnItem = items.FirstOrDefault(x => x.Value.key == id.ToString()).Value;

                }
            }
            return returnItem;
        }

        public string GetSpellIconPath(string fileName)
        {
            return $"{realms.cdn}/{realms.n.item}/img/spell/{fileName}";
        }

        public string GetRuneIconPath(string fileName)
        {
            return $"{realms.cdn}/img/{fileName}";
        }
    }
}
