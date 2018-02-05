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

        public async Task<List<KeyValuePair<int, int>>> Pick()
        {
            return await Task.Factory.StartNew(() =>
            {
                List<User> users = new List<User>();
                List<KeyValuePair<int, int>> res = new List<KeyValuePair<int, int>>();
                foreach (var handle in _handles)
                {
                    var subs = SubmissionChecker.Get(handle, 1000000);
                    if(subs == null || subs.Count == 0) continue;
                    
                    users.Add(new User()
                    {
                        handle = handle,
                        Submissions = subs
                    });
                }

                var result = ProblemSet.Contests.Where(c =>
                    users.All(u => u.Submissions.All(s => c.Value.All(p => p.name != s.problem.name))));

                return result.ToDictionary(k => k.Key, v => v.Value.Count).ToList();
            });
        }
    }
}