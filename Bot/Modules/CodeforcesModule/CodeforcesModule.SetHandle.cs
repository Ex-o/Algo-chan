using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using algochan.Helpers;
using Discord;
using Discord.Commands;

namespace algochan.Bot.Modules.CodeforcesModule
{
    
    public partial class CodeforcesModule
    {
        [Command("sethandle", RunMode = RunMode.Async)]
        public async Task SetHandle(string codeforcesHandle)
        {
            var discordInfo = Context.Message.Author.ToString();

            var rndContest = RandomContestGenerator.Get();

            #region ReplyBuild

            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Verification",
                    IconUrl =
                        "https://cdn1.iconfinder.com/data/icons/basic-ui-icon-rounded-colored/512/icon-41-512.png",
                    Url = ""
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "You have 1 minute.",
                    IconUrl = "https://d30y9cdsu7xlg0.cloudfront.net/png/273613-200.png"
                },
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Please visit the link below and submit a compilation error:",
                        IsInline = false,
                        Value = $"{rndContest.Item1}"
                    }
                }
            };

            #endregion

            await ReplyAsync("", false, embed);

            await Task.Delay(60000);

            var lastSub = SubmissionChecker.Get(codeforcesHandle);
            if (lastSub.contestId == rndContest.Item2 && lastSub.problem.index == rndContest.Item3.ToString())
            {
                _userManager.AddUser(discordInfo, codeforcesHandle);

                #region ReplyBuild

                embed.Color = Color.Green;
                embed.Author = new EmbedAuthorBuilder
                {
                    Name = "Verification: Success",
                    IconUrl =
                        "https://cdn1.iconfinder.com/data/icons/basic-ui-icon-rounded-colored/512/icon-41-512.png",
                    Url = ""
                };
                embed.Footer = new EmbedFooterBuilder
                {
                    Text = "Here, eat this.",
                    IconUrl = "https://image.flaticon.com/icons/png/128/164/164659.png"
                };
                embed.Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Your codeforces handle has been set to:",
                        IsInline = false,
                        Value = $"{codeforcesHandle}"
                    }
                };

                #endregion

                await ReplyAsync("", false, embed);
                _userManager.AddUser(discordInfo, codeforcesHandle);
                var rating = _userManager.GetUser(discordInfo).rating;
                var role = Utility.RolePicker(rating);

                var user = (IGuildUser) Context.User;
                await user.AddRoleAsync(user.Guild.Roles.First(w => w.Name == role));
            }
            else
            {
                #region ReplyBuild

                embed = new EmbedBuilder()
                {
                    Color = Color.Red,
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Verification: Failed",
                        IconUrl = "https://cdn2.iconfinder.com/data/icons/security-2-1/512/security_fail-512.png",
                        Url = ""
                    },
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "YOU HAD ONE JOB!",
                        IconUrl = ""
                    },
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Just try again",
                            IsInline = false,
                            Value = "I am sure you can do it."
                        }
                    }
                };

                #endregion

                await ReplyAsync("", false, embed);
            }
        }
    }
}