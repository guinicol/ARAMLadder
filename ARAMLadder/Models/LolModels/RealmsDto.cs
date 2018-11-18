namespace ARAMLadder.Models.LolModels
{
    public class RealmsDto
    {
        public PatchVersion n { get; set; }
        public string v { get; set; }
        public string l { get; set; }
        public string cdn { get; set; }
        public string dd { get; set; }
        public string lg { get; set; }
        public string css { get; set; }
        public int profileiconmax { get; set; }
        public object store { get; set; }
    }

    public class PatchVersion
    {
        public string item { get; set; }
        public string rune { get; set; }
        public string mastery { get; set; }
        public string summoner { get; set; }
        public string champion { get; set; }
        public string profileicon { get; set; }
        public string map { get; set; }
        public string language { get; set; }
        public string sticker { get; set; }
    }

}
