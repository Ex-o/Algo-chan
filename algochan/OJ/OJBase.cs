using System.Collections.Generic;
using System.Linq;
using cfapi.Objects;

namespace algochan.OJ
{
    public class OjBase : IOnlineJudgeBase
    {
        private List<Contest> _contests;

        public OjBase(string name)
        {
            _contests = new List<Contest>();
            Name = name;
        }

        public string Name { get; }

        public bool IsInitialized { get; set; }
        public virtual void ParseContests()
        {
        }
        public virtual void ReloadContests()
        {
            _contests.Clear();
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
        public bool Contains(Contest contest)
        {
            return _contests.Find(i => i.Id == contest.Id) != null;
        }
    }
}