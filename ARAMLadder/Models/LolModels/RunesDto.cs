namespace ARAMLadder.Models.LolModels
{
    public class RunesDto
    {
        public int id { get; set; }
        public string key { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public Slot[] slots { get; set; }
    }

    public class Slot
    {
        public RuneDto[] runes { get; set; }
    }

    public class RuneDto
    {
        public int id { get; set; }
        public string key { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public string shortDesc { get; set; }
        public string longDesc { get; set; }
    }

}
