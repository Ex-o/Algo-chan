using System.Collections.Generic;
using System.Linq;
using cfapi.Objects;

namespace algochan.OJ
{
    public class OjManager
    {
        private readonly List<OjBase> _onlineJudges = new List<OjBase>();

        public void AddJudge(OjBase oj)
        {
            _onlineJudges.Add(oj);
        }

        public void RemoveJudge(OjBase oj)
        {
            _onlineJudges.Remove(oj);
        }

        public IReadOnlyList<OjBase> GetJudgesList()
        {
            return _onlineJudges;
        }

        /// <summary>
        ///     Will only work with codeforces in the list.
        ///     Needs to be done properly.
        /// </summary>
        public void InitializeJudges()
        {
            foreach (var oj in _onlineJudges) {
                oj.ParseContests();
                oj.IsInitialized = true;
            }
        }

        public IReadOnlyList<Contest> GetContests(OnlineJudge onlineJudge)
        {
            switch (onlineJudge)
            {
                case OnlineJudge.CF:
                    return _onlineJudges.Find(i => i.Name == "codeforces").GetContests()
                        .Where(i => i.Phase == ContestPhase.BEFORE).ToList();
            }

            return new List<Contest>();
        }
    }
}