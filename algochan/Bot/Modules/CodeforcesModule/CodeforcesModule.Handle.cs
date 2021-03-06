﻿using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace algochan.Bot.Modules.CodeforcesModule
{
    public partial class CodeforcesModule 
    {
        [Command("handle", RunMode = RunMode.Async)]
        public async Task Handle(string user)
        {
            if (!string.IsNullOrEmpty(user) && user.Count() >= 3)
            {
                if (user[0] == '<')
                    user = user.Replace("<", "").Replace("!", "").Replace(">", "").Replace("@", "");
            }
            else
            {
                await ReplyAsync("Invalid user name. Make sure the length is 3+ characters!");
                return;
            }

            var users = (await Context.Guild.GetUsersAsync()).Where(i => (i.Username != null && i.Username.ToLower().Contains(user.ToLower()))
                                                                           || (i.Nickname != null &&
                                                                           i.Nickname.ToLower().Contains(user.ToLower()))
                                                                           || i.Id.ToString() == user);
            bool flag = false;
            foreach (var currentUser in users)
            {
                var discordInfo = currentUser.Id;

                if (_userManager.UserExists(discordInfo))
                {
                    flag = true;
                    await ReplyAsync($"http://codeforces.com/profile/{_userManager.GetUser(discordInfo).Handle}");
                    break;
                }
            }
            if (!flag)
            {
                await ReplyAsync("There's no such user!");
            }
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