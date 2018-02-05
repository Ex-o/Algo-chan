using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using algochan.API;
using algochan.Helpers;

namespace algochan.OJ
{
    public class Codeforces : OjBase
    {
        public Codeforces()
            : base("codeforces") { }

        public override void ParseContests(string api)
        {
            var jsonString = Utility.DownloadString(api);
            var obj = new JavaScriptSerializer().Deserialize<ContestObject>(jsonString);
            foreach (var contest in obj.result) AddContest(contest);

            RandomContestGenerator.Contests =
                obj.result.Where(i => i.phase == ContestPhase.FINISHED).ToList();

            //Call when contests update.
            //TODO::Call somewhere else.
            //ParseWebPages();

            ProblemSet.Contests = new JavaScriptSerializer().
                Deserialize<Dictionary<string, List<Problem>>>(File.ReadAllText("contests.txt"))
                .ToDictionary(k => int.Parse(k.Key), v => v.Value);
        }

        private void ParseWebPages()
        {
            //
            //Parsing http://codeforces.com/contests/*
            //
            char[] problemIndices = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U' };
            foreach (var contest in base.GetContests())
            {
                //TODO::Not potato.
                var idx = 0;
                var str = Utility.DownloadString($"http://codeforces.com/contest/{contest.id}");
                while (str.Contains($@"<a href=""/contest/{contest.id}/problem/{problemIndices[idx]}"">"))
                {
                    var pos = str.LastIndexOf($@"<a href=""/contest/{contest.id}/problem/{problemIndices[idx]}"">") + 62 - (3 - (int)Math.Floor(Math.Log10(contest.id) + 1));
                    if (pos == 0) continue;

                    string name = "";
                    while (str[pos] != '<' || str[pos + 1] != '!')
                    {
                        name += str[pos];
                        pos++;
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (!ProblemSet.Contests.ContainsKey(contest.id))
                            ProblemSet.Contests.Add(contest.id, new List<Problem>() { new Problem() { name = name } });
                        else
                            ProblemSet.Contests[contest.id].Add(new Problem() { name = name });
                    }
                    idx++;
                }
            }

            var properDic = ProblemSet.Contests.ToDictionary(k => k.Key.ToString(), v => v.Value);
            var serializer = new JavaScriptSerializer().Serialize(properDic);
            File.WriteAllText("contests.txt", serializer);
        }
        public override bool IsOnline(string api)
        {
            return true;
        }
    }
}