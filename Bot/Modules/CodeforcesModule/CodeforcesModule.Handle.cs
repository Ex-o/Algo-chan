using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace algochan.Bot.Modules.CodeforcesModule
{
    public partial class CodeforcesModule 
    {
        [Command("handle", RunMode = RunMode.Async)]
        public async Task Handle(string user)
        {
            if (!string.IsNullOrEmpty(user) && user.Count() >= 2)
                if (user[0] == '<')
                    user = user.Replace("<", "").Replace("!", "").Replace(">", "").Replace("@", "");
            var userInfo = Context.Guild.GetUsersAsync().Result.First(i => i.Username.ToLower().Contains(user.ToLower())
                                                                           || i.Nickname != null &&
                                                                           i.Nickname.ToLower().Contains(user.ToLower())
                                                                           || i.Id.ToString() == user);

            var discordInfo = $"{userInfo.Username}#{userInfo.Discriminator}";

            if (_userManager.UserExists(discordInfo))
                await ReplyAsync($"http://codeforces.com/profile/{_userManager.GetUser(discordInfo).handle}");
            else
                await ReplyAsync("There's no such user!");
        }


        /*
        [Command("vcp")]
        public async Task VirtualContestPicker(List<string> users)
        {

        }
        /*
        [Command("subscribelist")]
        public async Task SubList()
        {
            Task.Factory.StartNew(() =>
           {
               foreach (var user in UserManager.Instance.DebugGetAllUsers())
               {
                   if (user.Value.Subscriped)
                   {
                       ReplyAsync(user.Key);
                   }
               }
           });
        }
        */
    }
}