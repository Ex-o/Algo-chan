using System;
using System.Collections.Generic;

namespace algochan.Helpers
{
    internal static class RandomContestGenerator
    {
        public static List<int> Contests = new List<int>();
        private static readonly char[] idx = {'A', 'B', 'C', 'D'};

        public static Tuple<string, int, char> Get()
        {
            var contestID = Contests[new Random().Next(0, Contests.Count - 1)];
            var problemIdx = idx[new Random().Next(0, 3)];
            return Tuple.Create($"http://codeforces.com/contest/{contestID}/problem/{problemIdx}", contestID,
                problemIdx);
        }
    }
}