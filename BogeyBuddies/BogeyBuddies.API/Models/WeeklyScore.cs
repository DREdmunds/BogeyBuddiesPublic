using Newtonsoft.Json;
using System;

namespace BogeyBuddies.API.Models
{
    public class WeeklyScore
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("partitionKey")]
        public string UserId { get; set; } 

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("weekIdentifier")]
        public string WeekIdentifier { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("grossScore")]
        public int GrossScore { get; set; }

        [JsonProperty("handicap")]
        public int Handicap { get; set; }

        [JsonProperty("netScore")]
        public int NetScore { get; set; }

        [JsonProperty("weeklyWinnings")]
        public decimal WeeklyWinnings { get; set; }
    }
}
