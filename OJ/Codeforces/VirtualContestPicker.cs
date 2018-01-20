using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using algochan.API;
using algochan.Helpers;
using System.Threading.Tasks;

namespace algochan.OJ
{
    public class VirtualContestPicker
    {
        private List<string> _handles;

        public VirtualContestPicker(string[] handles)
        {
            _handles = handles.ToList();
        }

        public async Task<string> Pick()
        {
            return await Task.Factory.StartNew(() =>
            {
                List<User> users = new List<User>();
                foreach (var handle in _handles)
                {
                    users.Add(new User() {handle = handle});
                    users[users.Count - 1].Submissions = SubmissionChecker.Get(handle, 1000000);
                }

                foreach (var contest in ProblemSet.Contests)
                {
                    bool ok = true;
                    foreach (var problem in contest.Value)
                    {
                        foreach (var handle in _handles)
                        {
                            var user = users.Find(i => i.handle == handle);

                            if (user.Submissions != null &&
                                user.Submissions.Exists(i => i.problem.name == problem.name))
                            {
                                ok = false;
                                break;
                            }

                            if (!ok) break;
                        }

                        if (!ok) break;
                    }

                    if (ok)
                    {
                        return $"http://codeforces.com/contest/{contest.Key.ToString()}/";
                    }
                }

                return "Not found!";
            });
        }
    }
}