using System.Collections.Generic;
using algochan.OJ;
using algochan.Services;
using Discord.Commands;
using System.Threading.Tasks;

namespace algochan.Bot.Modules.CodeforcesModule
{
    [Group("codeforces"), Alias("cf")]
    public partial class CodeforcesModule : ModuleBase
    {
        private readonly OjManager _ojManager;
        private readonly UserManager _userManager;

        public CodeforcesModule(UserManager userManager, OjManager ojManager)
        {
            _userManager = userManager;
            _ojManager = ojManager;
        }

        [Command("vc")]
        public async Task Pick(string handles)
        {
            await Task.Factory.StartNew(async () =>
                ReplyAsync(await _userManager.VirtualContestPicker(handles.Split(' '))));
        }

        [Command("magic")]
        [RequireOwner]
        public async Task Magic(string strRole)
        {
            await _userManager.UpdateRole(Context.User.Id, strRole);
        }
    }
}