using System.Collections.Generic;

namespace algochan.API
{
    //WEIRD API JSON STUFF
    public class ContestObject
    {
        public string status { get; set; }
        public List<Contest> result { get; set; }
    }

    public class SubmissionObject
    {
        public string status { get; set; }
        public List<Submission> result { get; set; }
    }

    public class UserObject
    {
        public string status { get; set; }
        public List<User> result { get; set; }
    }
}