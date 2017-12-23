using algochan.OJ;
using algochan.Services;
using Discord.Commands;

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
    }
}