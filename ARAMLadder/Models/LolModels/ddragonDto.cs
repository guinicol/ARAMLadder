using System.Collections.Generic;

namespace ARAMLadder.Models.LolModels
{
    public class DdragonDto<T>
    {
        public string type { get; set; }
        public string version { get; set; }
        public Dictionary<string, T> data { get; set; }
    }
}