using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using algochan.Helpers;
using System.Threading.Tasks;
using cfapi;
using cfapi.Methods;
using cfapi.Objects;
using algochan.Services;
using Microsoft.CSharp;

namespace algochan.OJ
{
    public class VirtualContestPicker
    {
        struct CustomUser
        {
            public string handle;
            public List<Submission> submissions;
        }
        private List<string> _handles;

        public VirtualContestPicker(string[] handles)
        {
            _handles = handles.ToList();
        }

        public async Task<List<KeyValuePair<int, int>>> Pick()
        {
            return await Task.Factory.StartNew(async () =>
            {
                var users = new List<CustomUser>();
                List<KeyValuePair<int, int>> res = new List<KeyValuePair<int, int>>();
                foreach (var handle in _handles)
                {
                    var req = new UserStatusRequest();
                    var submissions = await req.GetUserSubmissionsAsync(handle, 1, 10000000);
                    if (submissions == null || submissions.Count == 0) continue;

                    users.Add(new CustomUser()
                    {
                        handle = handle ,
                        submissions = submissions
                    });
                }
                var result = Globals.ProblemSet.Problems.Where(c =>
                users.All(u => u.submissions.All(s => c.Name != s.Problem.Name && c.ContestId != s.Problem.ContestId)));

                var contestList = new Dictionary<int, int>();
                
                foreach(var problem in result)
                {
                    if (!contestList.ContainsKey(problem.ContestId))
                        contestList.Add(problem.ContestId, 1);
                    else
                        contestList[problem.ContestId]++;
                }
                return contestList.ToList();
            }).Result;
        }
    }
}