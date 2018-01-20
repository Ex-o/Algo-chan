using System.Collections.Generic;
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


            ProblemSet.Contests = new Dictionary<int, List<Problem>>();
            jsonString = Utility.DownloadString("http://codeforces.com/api/problemset.problems");
            var parsed = new JavaScriptSerializer().Deserialize<ProblemObject>(jsonString);
            foreach (var problem in parsed.result.problems)
            {
                if (ProblemSet.Contests.ContainsKey(problem.contestId))
                    ProblemSet.Contests[problem.contestId].Add(problem);
                else
                {
                    if (problem.contestId == 871)
                    {

                    }
                    if (problem.index != "A" && ProblemSet.Contests.ContainsKey(problem.contestId + 1))
                    {
                        ProblemSet.Contests.Add(problem.contestId, ProblemSet.Contests[problem.contestId + 1]);
                        ProblemSet.Contests[problem.contestId].Add(problem);
                    }
                    else
                        ProblemSet.Contests.Add(problem.contestId, new List<Problem>() {problem});
                }
            }
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