using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using cfapi.Objects;
using cfapi.Methods;
using algochan.Helpers;
using algochan.Services;

namespace algochan.OJ
{
    public class Codeforces : OjBase
    {
        public static ProblemSet ProblemSet { get; set; }

        public Codeforces()
            : base("codeforces")
        {
            var req = new ProblemsRequest();
            ProblemSet = req.GetProblemSetAsync().Result;
        }

        public override void ParseContests()
        {
            var req = new ContestListRequest();
            var contests = req.GetContestList();

            foreach (var contest in contests) AddContest(contest);

            RandomContestGenerator.Contests =
                contests.Where(i => i.Phase == ContestPhase.FINISHED).ToList();

            if (!IsInitialized)
            {
                LoadContestsFromFile();
            }
        }

        public override void ReloadContests()
        {
            base.ReloadContests();
            ParseContests();
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
                var str = Utility.DownloadString($"http://codeforces.com/contest/{contest.Id}");
                while (str.Contains($@"<a href=""/contest/{contest.Id}/problem/{problemIndices[idx]}"">"))
                {
                    var pos = str.LastIndexOf($@"<a href=""/contest/{contest.Id}/problem/{problemIndices[idx]}"">") + 62 - (3 - (int)Math.Floor(Math.Log10(contest.Id) + 1));
                    if (pos == 0) continue;

                    string name = "";
                    while (str[pos] != '<' || str[pos + 1] != '!')
                    {
                        name += str[pos];
                        pos++;
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (!Globals.ContestsProblemsList.ContainsKey(contest.Id))
                            Globals.ContestsProblemsList.Add(contest.Id, new List<Problem>() { new Problem() { Name = name } });
                        else
                            Globals.ContestsProblemsList[contest.Id].Add(new Problem() { Name = name });
                    }
                    idx++;
                }
            }

            var properDic = Globals.ContestsProblemsList.ToDictionary(k => k.Key.ToString(), v => v.Value);
            var serializer = new JavaScriptSerializer().Serialize(properDic);
            File.WriteAllText("contests.txt", serializer);
        }
        public override bool IsOnline(string api)
        {
            return true;
        }

        private void LoadContestsFromFile()
        {
            //Call when contests update.
            //TODO::Call somewhere else.
            Globals.ContestsProblemsList = new Dictionary<int, List<Problem>>();
            //ParseWebPages();

            var req2 = new ProblemsRequest();
            Globals.ProblemSet = req2.GetProblemSetAsync().Result;
            Globals.ContestsProblemsList = new JavaScriptSerializer().
                Deserialize<Dictionary<string, List<Problem>>>(File.ReadAllText("contests.txt"))
               .ToDictionary(k => int.Parse(k.Key), v => v.Value);
        }
    }
}