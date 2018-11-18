using ARAMLadder.Models.LolModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAMLadder.Services
{
    public interface ILolStaticDataService
    {
        Task<ChampionDto> GetChampionAsync(int id);
        Task<ItemDto> GetItemAsync(int id);
        Task<ProfileIconDto> GetProfileIconAsync(int id);
        Task<RuneDto> GetRuneAsync(int familleId, int? id = null);
        Task<SpellDto> GetSpellAsync(int id);

        string GetProfileIconPath(string fileName);
        string GetItemIconPath(string fileName);
        string GetChampionIconPath(string fileName);
        string GetSpellIconPath(string fileName);
        string GetRuneIconPath(string fileName);
    }
}
