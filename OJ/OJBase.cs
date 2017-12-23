using System.Collections.Generic;
using algochan.API;

namespace algochan.OJ
{
    public class OjBase : IOnlineJudge
    {
        private readonly List<Contest> _contests;

        public OjBase(string name)
        {
            _contests = new List<Contest>();
            Name = name;
        }

        public string Name { get; }

        public virtual void ParseContests(string api)
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