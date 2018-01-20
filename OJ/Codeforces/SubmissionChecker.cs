using System.Collections.Generic;
using System.Web.Script.Serialization;
using algochan.API;
using algochan.Helpers;

namespace algochan.OJ
{
    public class SubmissionChecker
    {
        public static List<Submission> Get(string codeforcesHandle, int count)
        {
            var jsonTxt =
                Utility.DownloadString(
                    $"http://codeforces.com/api/user.status?handle={codeforcesHandle}&from=1&count={count}");
            var obj = new JavaScriptSerializer().Deserialize<SubmissionObject>(jsonTxt);
            return obj?.result;
        }
    }
}