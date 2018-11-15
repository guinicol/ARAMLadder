namespace ARAMLadder.Models.LolModels
{

    public class ListMatchDto
    {
        public Match[] matches { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public int totalGames { get; set; }
    }

    public class Match
    {
        public string platformId { get; set; }
        public long gameId { get; set; }
        public int champion { get; set; }
        public int queue { get; set; }
        public int season { get; set; }
        public long timestamp { get; set; }
        public string role { get; set; }
        public string lane { get; set; }
    }

}
