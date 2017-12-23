using System.Linq;
using System.Web.Script.Serialization;
using algochan.API;
using algochan.Helpers;

namespace algochan.OJ
{
    public class Codeforces : OjBase
    {
        public Codeforces()
            : base("codeforces")
        {
        }

        public override void ParseContests(string api)
        {
            var jsonString = Utility.DownloadString(api);
            var obj = new JavaScriptSerializer().Deserialize<ContestObject>(jsonString);
            foreach (var contest in obj.result) AddContest(contest);
            RandomContestGenerator.Contests =
                obj.result.Where(i => i.phase == ContestPhase.FINISHED).Select(i => i.id).ToList();
        }

        public override bool IsOnline(string api)
        {
            return true;
        }

        /*
        public int VCPick(List<string> users)
        {
            foreach(var contest in this.GetContests())
            {
                
            }
        }
        */
    }
}