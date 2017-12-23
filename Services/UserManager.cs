using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using algochan.API;
using algochan.Helpers;
using algochan.OJ;
using Discord.WebSocket;

namespace algochan.Services
{
    public class UserManager
    {
        #region Fields

        private readonly Dictionary<string, User> _userList = new Dictionary<string, User>();
        private readonly OjManager _ojManager;
        private readonly DataManager _dataManager = new DataManager();
        private readonly IReadOnlyCollection<SocketGuild> _servers;

        //(!!)ServerID
        private SocketGuild MyServer => _servers.FirstOrDefault(i => i.Id == 000000000000);
        //private DateTime time = new DateTime();

        #endregion
        public UserManager(IReadOnlyCollection<SocketGuild> servers, OjManager ojManager)
        {
            _servers = servers;
            _ojManager = ojManager;
            Initialize();
            NotifyCheck();
        }

        public Dictionary<string, User> DebugGetAllUsers()
        {
            return _userList;
        }

        public void NotifyCheck()
        {
            Task.Factory.StartNew(() =>
            {
                var contests = _ojManager.GetContests(OnlineJudge.CF);
                foreach (var contest in contests)
                    if ((Utility.FromUnixTime(contest.startTimeSeconds) - DateTime.UtcNow).TotalMinutes <= 60)
                        if ((DateTime.Now - contest.time).TotalMinutes >= 20 && contest.time > DateTime.Now)
                            foreach (var user in _userList)
                            {
                                contest.time = DateTime.Now;
                                if (!user.Value.Subscriped) continue;

                                var guilduser =
                                    MyServer.Users.FirstOrDefault(i => i.Discriminator == user.Key.Split('#')[1]);

                                if (guilduser == null)
                                    continue;
                                var channel = guilduser.GetOrCreateDMChannelAsync().Result;

                                if (channel == null)
                                    continue;

                                channel.SendMessageAsync("Contest starts soon\n", false, Utility.BuildContest(contest));
                            }
            }).Repeat(new CancellationTokenSource().Token, TimeSpan.FromMinutes(1));
        }

        public bool UserExists(string discordInfo)
        {
            return _dataManager.UserExists(discordInfo);
        }

        public void AddUser(string discordInfo, string codeforcesHandle)
        {
            var jsonTxt = Utility.DownloadString($"http://codeforces.com/api/user.info?handles={codeforcesHandle}");
            var obj = new JavaScriptSerializer().Deserialize<UserObject>(jsonTxt);

            if (_dataManager.UserExists(discordInfo))
            {
                _userList[discordInfo] = obj.result[0];
            }
            else
            {
                if (obj.status == "OK")
                    _dataManager.AddUser(discordInfo, new JavaScriptSerializer().Serialize(obj.result[0]));
            }
        }

        public User GetUser(string discordInfo)
        {
            return _userList[discordInfo];
        }

        public void Initialize()
        {
            Task.Factory.StartNew(() =>
            {
                _dataManager.Initialize();
                var users = _dataManager.GetAllUsers();

                while (users.Read())
                    _userList[users["discordInfo"] as string]
                        = new JavaScriptSerializer().Deserialize<User>(
                            EncryptionHelper.Decrypt(users["serializedData"] as string));

                //users = _dataManager.GetSubscribers();

                //while(users.Read())
                //{
                //    var x = users["discordId"].ToString();
                //    var discordUser = MyServer.Users.FirstOrDefault(i => i.Id == ulong.Parse(x));
                //    if (discordUser == null) continue;
                //    var discordInfo = discordUser.Username + '#' + discordUser.DiscriminatorValue;
                //    _userList[discordInfo].Subscriped = true;
                //}
            });
        }

        internal async Task UpdateSerliazedObjects()
        {
            await Task.Factory.StartNew(async () =>
            {
                foreach (var user in _userList.ToArray())
                {
                    var jsonTxt =
                        Utility.DownloadString($"http://codeforces.com/api/user.info?handles={user.Value.handle}");
                    var obj = new JavaScriptSerializer().Deserialize<UserObject>(jsonTxt);

                    if (obj.status == "OK")
                    {
                        _userList[user.Key] = obj.result[0];
                        _dataManager.UpdateUser(user.Key, new JavaScriptSerializer().Serialize(obj.result[0]));
                        var role = Utility.RolePicker(obj.result[0].rating);
                        var srvUser = MyServer.Users.FirstOrDefault(i =>
                            i.DiscriminatorValue.ToString() == user.Key.Split('#')[1]);
                        if (srvUser == null) continue;
                        await srvUser.RemoveRolesAsync(srvUser.Roles.Where(i =>
                            !new[] {"Moderator", "Administrator", "@everyone"}.Any(i.Name.Contains)));
                        await srvUser.AddRoleAsync(MyServer.Roles.FirstOrDefault(i => i.Name == role));
                    }

                    await Task.Delay(500);
                }
            });
        }

        internal void RemoveSubscribe(ulong id)
        {
            _dataManager.RemoveSubscribe(id);
            var discordUser = MyServer.Users.FirstOrDefault(i => i.Id == id);

            if (discordUser == null) return;

            var discordInfo = discordUser.Username + '#' + discordUser.DiscriminatorValue;
            _userList[discordInfo].Subscriped = false;
        }

        internal void AddSubscribe(ulong id)
        {
            _dataManager.AddSubscribe(id);
            var discordUser = MyServer.Users.FirstOrDefault(i => i.Id == id);
            var discordInfo = discordUser.Username + '#' + discordUser.DiscriminatorValue;
            _userList[discordInfo].Subscriped = true;
        }
    }
}