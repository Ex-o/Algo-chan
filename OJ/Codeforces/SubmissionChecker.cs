using System.Web.Script.Serialization;
using algochan.API;

namespace algochan.Helpers
{
    public class SubmissionChecker
    {
        public static Submission Get(string codeforcesHandle)
        {
            var jsonTxt =
                Utility.DownloadString(
                    $"http://codeforces.com/api/user.status?handle={codeforcesHandle}&from=1&count=1");
            var obj = new JavaScriptSerializer().Deserialize<SubmissionObject>(jsonTxt);
            return obj.result[0];
        }
    }
}