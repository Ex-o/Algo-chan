using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace algochan.Bot.Modules
{
    public class UtilityModule : ModuleBase
    {
        [Command("help")]
        public async Task Help()
        {
            #region TODO::SERIALIZE

            var embed = new EmbedBuilder()
            {
                Color = Color.DarkPurple,
                Author = new EmbedAuthorBuilder
                {
                    Name = "algo-chan Commands",
                    IconUrl = "",
                    Url = ""
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "the admin is very unskilled!",
                    IconUrl = ""
                },
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "!codeforces sethandle yourhandle",
                        IsInline = false,
                        Value = "Sets your codeforces handle and grants you a color."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "!codeforces handle user",
                        IsInline = false,
                        Value = "Get the handles of other users in this server."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "!codeforces updateroles",
                        IsInline = false,
                        Value = "Update roles after a contest **(currently restricted to server admins)**"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "!codeforces contests",
                        IsInline = false,
                        Value = "Get a list of the upcoming contests on codeforces."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "!codeforces reminder on||off",
                        IsInline = false,
                        Value =
                            "algo-chan will remind you when a contest is about to start (1 hour earlier then every 25 minutes)."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "!say Something",
                        IsInline = false,
                        Value = "Make algo-chan say something. (Moderator)"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "!pow x^y",
                        IsInline = false,
                        Value = "y is the _positive_ exponent! **much wow**"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "!help",
                        IsInline = false,
                        Value = "Prints what you're reading right now basically **you don't say**"
                    }
                }
            };
#endregion

            await ReplyAsync("", false, embed);
        }

        [Command("pow")]
        [Summary("Pows a number.")]
        public async Task Square([Summary("The number to square.")] string num)
        {
            await Task.Factory.StartNew(() =>
            {
                Context.Channel.SendMessageAsync(
                    $"{BigInteger.Parse(num.Split('^')[0])}^{BigInteger.Parse(num.Split('^')[1])} = {BigInteger.Pow(BigInteger.Parse(num.Split('^')[0]), int.Parse(num.Split('^')[1]))}");
            });
        }

        [Command("say")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task SayHandle(string msg)
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(msg);
        }

        [Command("bassel")]
        [Alias("knows every thing", "knows it all", "ds king", "i luv trees", "typo master", "typo_master")]
        public async Task Bassel()
        {
            if (Context.Message.Content.Contains("bassel"))
                await ReplyAsync("tree lover*");
            else
                await ReplyAsync("Bassel?");
        }
    }
}