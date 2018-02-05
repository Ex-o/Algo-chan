using System;
using System.Collections.Generic;
using algochan.API;

namespace algochan.Helpers
{
    internal static class RandomContestGenerator
    {
        public static List<Contest> Contests = new List<Contest>();
        private static readonly char[] idx = {'A', 'B', 'C', 'D'};

        //TODO::Cache err code 503 (contests which aren't available for submissions [onsite])
        public static Tuple<string, int, char> Get()
        {
            var contest = Contests[new Random().Next(0, Contests.Count - 1)];
            var problemIdx = idx[new Random().Next(0, 3)];
            return Tuple.Create($"http://codeforces.com/contest/{contest.id}/problem/{problemIdx}", contest.id,
                problemIdx);
        }
    }
}