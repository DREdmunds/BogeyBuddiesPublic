namespace BogeyBuddies.Models.Scores
{
    public class WeeklyScoreResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string WeekIdentifier { get; set; }
        public DateTime Date { get; set; }
        public int GrossScore { get; set; }
        public int Handicap { get; set; }
        public int NetScore { get; set; }
        public decimal WeeklyWinnings { get; set; }
    }
}
