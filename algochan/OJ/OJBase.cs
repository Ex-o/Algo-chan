﻿using System.Collections.Generic;
using cfapi.Objects;

namespace algochan.OJ
{
    public class OjBase : IOnlineJudgeBase
    {
        private readonly List<Contest> _contests;

        public OjBase(string name)
        {
            _contests = new List<Contest>();
            Name = name;
        }

        public string Name { get; }

        public virtual void ParseContests()
        {
        }

        public virtual bool IsOnline(string api)
        {
            return true;
        }

        public void AddContest(Contest contest)
        {
            _contests.Add(contest);
        }

        public void RemoveContest(Contest contest)
        {
            _contests.Remove(contest);
        }

        public IReadOnlyList<Contest> GetContests()
        {
            return _contests;
        }
    }
}