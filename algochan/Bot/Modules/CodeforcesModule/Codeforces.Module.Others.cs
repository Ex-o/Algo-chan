using System.Threading.Tasks;
using algochan.Helpers;
using algochan.OJ;
using Discord;
using Discord.Commands;
using System.Linq;
using cfapi.Objects;
using cfapi.Methods;
using System.Collections.Generic;

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

                if (status == null || status.Count == 0)
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

        [Command("setfav")]
        public async Task SetFavoriteProblem(string url)
        {
            var parsedUrl = Utility.IsValidCodeforcesProblemUrl(url);

            if (parsedUrl == null)
            {
                var embed = new EmbedBuilder()
                {
                    Color = Color.Red,
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Invalid Problem Url",
                        IconUrl = "https://cdn2.iconfinder.com/data/icons/security-2-1/512/security_fail-512.png",
                        Url = ""
                    },
                };

                await ReplyAsync("", false, embed);
                return;
            }
            else
            {
                if(_userManager.ProblemExists(Context.User.Id, parsedUrl))
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = Color.Red,
                        Author = new EmbedAuthorBuilder
                        {
                            Name = "Problem already added to your favorites.",
                            IconUrl = "https://cdn2.iconfinder.com/data/icons/security-2-1/512/security_fail-512.png",
                            Url = ""
                        }
                    };

                    await ReplyAsync("", false, embed);
                    return;
                }
                else
                {
                    _userManager.AddFavoriteProblem(Context.User.Id, parsedUrl);
                    await ReplyAsync(Context.User.Mention + " The problem has been added to your favorites!");
                }
            }
        }

        [Command("fav")]
        public async Task GetFavoriteProblems(string user)
        {
            if (!string.IsNullOrEmpty(user) && user.Count() >= 3)
            {
                if (user[0] == '<')
                    user = user.Replace("<", "").Replace("!", "").Replace(">", "").Replace("@", "");
            }

            var users = (await Context.Guild.GetUsersAsync()).Where(i => (i.Username != null && i.Username.ToLower().Contains(user.ToLower()))
                                                                          || (i.Nickname != null &&
                                                                          i.Nickname.ToLower().Contains(user.ToLower()))
                                                                          || i.Id.ToString() == user);

            bool flag = false;
            foreach (var currentUser in users)
            {
                var discordInfo = currentUser.Id;
                var favs = _userManager.GetAllFavorites(discordInfo);
                if (favs.Count != 0)
                {
                    flag = true;
                    var embed = new EmbedBuilder();
                    embed.ThumbnailUrl = "https://png.pngtree.com/svg/20170213/favorite_1267479.png";
                    embed.Color = Color.Blue;
                    embed.Author = new EmbedAuthorBuilder
                    {
                        Name = "List of favorite problems:",
                        IconUrl = "",
                    };
                    int cnt = 1;
                    foreach (var problem in favs)
                    {
                        embed.Fields.Add(new EmbedFieldBuilder()
                        {
                            Name = problem,

                            IsInline = false,
                            Value = "---------------------------"
                        });
                        cnt++;
                    }
                    await ReplyAsync("", false, embed);
                    break;
                }
            }
            if (!flag)
            {
                await ReplyAsync("This user has no favorite problems :(");
            }
        }
    }
}