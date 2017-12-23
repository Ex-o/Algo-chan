using System.Threading.Tasks;
using algochan.Helpers;
using algochan.OJ;
using Discord.Commands;

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
        [RequireOwner]
        public async Task UpdateRoles()
        {
            await _userManager.UpdateSerliazedObjects();
        }
    }
}