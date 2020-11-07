using Newtonsoft.Json;

namespace adaptive_demo.Models
{   

    public class FeedbackItem
    {
        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public int rating { get; set; }
        [JsonProperty]
        public string comment { get; set; }
    }
}