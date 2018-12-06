using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using algochan.OJ;
using algochan.Services;
using Discord.Commands;
using System.Threading.Tasks;
using cfapi.Objects;
using algochan.Helpers;
using Discord;

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
        async void ContestPicker(string div, int count, string handles)
        {
            List<KeyValuePair<int, int>> availableContests = new List<KeyValuePair<int, int>>();

            availableContests = await _userManager.VirtualContestPicker(handles.Split(' '));
            //var rndSelection = availableContests.OrderBy(x => Guid.NewGuid()).ToList().Take(5);
            var globalContests = RandomContestGenerator.Contests;
            List<Contest> rndSelection = globalContests.Where(i => availableContests.Any(x => x.Value >= count && x.Key == i.Id && i.Name.Contains(div.ToLower() == "div1" ? "Div. 1" : "Div. 2"))).OrderBy(x => Guid.NewGuid()).ToList().Take(6).ToList();
            var embed = new EmbedBuilder();
            embed.ThumbnailUrl = "https://26.org.uk/wp-content/uploads/2016/06/Trophyicon_1198.png";
            embed.Color = Color.Blue;
            embed.Author = new EmbedAuthorBuilder
            {
                Name = "List of available contests:",
                IconUrl = "",
            };
            embed.Footer = new EmbedFooterBuilder
            {
                Text = "Enjoy your practice.",

            };
            foreach (var contest in rndSelection)
            {
                embed.Fields.Add(new EmbedFieldBuilder()
                {
                    Name = $"[{contest.Name}]\n(http://codeforces.com/contest/{contest.Id})",

                    IsInline = false,
                    Value = $"{availableContests.Find(i => i.Key == contest.Id).Value} problems."
                });
            }
            try
            {
                await ReplyAsync("", false, embed);
            }
            catch { }
        }
        [Command("vc")]
        public async Task Pick(string div, int count, string handles)
        {
            Task.Factory.StartNew(() =>
            {
                ContestPicker(div, count, handles);
            });
        }

        [Command("magic")]
        [RequireOwner]
        public async Task Magic(string strRole)
        {
            await _userManager.UpdateRole(Context.User.Id, strRole);
        }
    }
}