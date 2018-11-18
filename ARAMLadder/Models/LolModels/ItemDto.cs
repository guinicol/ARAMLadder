namespace ARAMLadder.Models.LolModels
{
    public class ItemDto
    {
        public string name { get; set; }
        public string description { get; set; }
        public string colloq { get; set; }
        public string plaintext { get; set; }
        public string[] into { get; set; }
        public string[] from { get; set; }
        public Image image { get; set; }
        public Gold gold { get; set; }
        public string[] tags { get; set; }
        public Maps maps { get; set; }
    }

    public class Gold
    {
        public int _base { get; set; }
        public bool purchasable { get; set; }
        public int total { get; set; }
        public int sell { get; set; }
    }

    public class Maps
    {
        public bool _1 { get; set; }
        public bool _8 { get; set; }
        public bool _10 { get; set; }
        public bool _11 { get; set; }
        public bool _12 { get; set; }
        public bool _14 { get; set; }
    }
}
