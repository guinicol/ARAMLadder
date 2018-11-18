namespace ARAMLadder.Models.LolModels
{
    public class SpellDto
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string tooltip { get; set; }
        public int maxrank { get; set; }
        public float[] cooldown { get; set; }
        public string cooldownBurn { get; set; }
        public int[] cost { get; set; }
        public string costBurn { get; set; }
        //public int[][] effect { get; set; }
        public string[] effectBurn { get; set; }
        public object[] vars { get; set; }
        public string key { get; set; }
        public int summonerLevel { get; set; }
        public string[] modes { get; set; }
        public string costType { get; set; }
        public string maxammo { get; set; }
        public float[] range { get; set; }
        public string rangeBurn { get; set; }
        public Image image { get; set; }
        public string resource { get; set; }
    }

}
