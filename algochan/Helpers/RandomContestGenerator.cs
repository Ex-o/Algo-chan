using System;
using System.Collections.Generic;
using cfapi.Objects;

namespace algochan.Helpers
{
    internal static class RandomContestGenerator
    {
        public static List<Contest> Contests = new List<Contest>();
        private static readonly char[] idx = {'A', 'B', 'C', 'D'};

        public static Tuple<string, int, char> Get()
        {
            var contest = Contests[new Random().Next(0, Contests.Count - 1)];
            var problemIdx = idx[new Random().Next(0, 3)];
            return Tuple.Create($"http://codeforces.com/contest/{contest.Id}/problem/{problemIdx}", contest.Id,
                problemIdx);
        }
    }
}