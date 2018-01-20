using System.Collections.Generic;

namespace algochan.API
{
    public class User
    {
        public string handle { get; set; }
        public string email { get; set; }
        public string vkId { get; set; }
        public string openId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string organization { get; set; }
        public int contribution { get; set; }
        public string rank { get; set; }
        public int rating { get; set; }
        public string maxRank { get; set; }
        public int maxRating { get; set; }
        public int lastOnlineTimeSeconds { get; set; }
        public int registrationTimeSeconds { get; set; }
        public int friendOfCount { get; set; }
        public string avatar { get; set; }
        public string titlePhoto { get; set; }
        public bool Subscriped { get; set; }

        public List<Submission> Submissions;
    }
}