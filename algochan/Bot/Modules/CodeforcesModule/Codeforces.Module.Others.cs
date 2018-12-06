using System.Threading.Tasks;
using algochan.Helpers;
using algochan.OJ;
using Discord;
using Discord.Commands;
using System.Linq;
using cfapi.Objects;
using cfapi.Methods;

namespace algochan.Bot.Modules.CodeforcesModule
{
    public partial class CodeforcesModule
    {
        [Command("reminder", RunMode = RunMode.Async)]
        public async Task Subscribe(string arg)
        {
            if (arg == "on")
                _userManager.AddSubscribe(Context.User.Id);
            else if (arg == "off")
                _userManager.RemoveSubscribe(Context.User.Id);

            await ReplyAsync("Alright");
        }

        [Command("contests", RunMode = RunMode.Async)]
        public async Task GetContests()
        {
            var contests = _ojManager.GetContests(OnlineJudge.CF);
            foreach (var contest in contests) await ReplyAsync("", false, Utility.BuildContest(contest));
        }

        [Command("updateroles")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task UpdateRoles()
        {
            await _userManager.UpdateSerliazedObjects();
            await ReplyAsync("Alright, don't cry when you go down tho!");
        }

        [Command("avrgdiff")]
        public async Task AverageDifficulty(string handle)
        {
            Task.Factory.StartNew(() =>
            {
                var req = new UserStatusRequest();
                var status = req.GetUserSubmissions(handle);

                if(status == null || status.Count == 0)
                {
                    ReplyAsync("Failed to calculate!");
                    return;
                }

                bool Compare(Problem a, Problem b)
                {
                    return a.Name == b.Name;
                }
                var acceptedSubmissions = status.Where(i => i.Verdict == SubmissionVerdict.OK).Select(i => i.Problem).GroupBy(i => i.Name).Select(i => i.First());

                float totalPoints = acceptedSubmissions.Sum(i => i.Points);
                ReplyAsync(totalPoints.ToString());
            });
        }
    }
}