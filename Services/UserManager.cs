using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using algochan.API;
using algochan.Helpers;
using algochan.OJ;
using Discord;
using Discord.WebSocket;

namespace algochan.Services
{
    public class UserManager
    {
        #region Fields
        private const long CpcId = 326795829664808960;
        private readonly Dictionary<ulong, User> _userList = new Dictionary<ulong, User>();
        private readonly OjManager _ojManager;
        private readonly DataManager _dataManager = new DataManager();
        private readonly IReadOnlyCollection<SocketGuild> _servers;
        //private DateTime time = new DateTime();
        #endregion
        private SocketGuild MyServer => _servers.FirstOrDefault(i => i.Id == CpcId);
        public bool UserExists(ulong discordId) => _dataManager.UserExists(discordId);
        private SocketGuildUser FindUser(ulong userDiscordId) => MyServer.Users.FirstOrDefault(i => i.Id == userDiscordId);
        private SocketGuildUser FindUser(int userDiscriminator) => MyServer.Users.FirstOrDefault(i => i.DiscriminatorValue == userDiscriminator);
        public UserManager(IReadOnlyCollection<SocketGuild> servers, OjManager ojManager)
        {
            _servers = servers;
            _ojManager = ojManager;

            Initialize();
            NotifyCheck();
        }
        public User GetUser(ulong discordId) => _userList[discordId];
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
                                    MyServer.Users.FirstOrDefault(i => i.Id == user.Key);//.Split('#')[1]);

                                if (guilduser == null)
                                    continue;

                                var channel = guilduser.GetOrCreateDMChannelAsync().Result;

                                if (channel == null)
                                    continue;

                                channel.SendMessageAsync("Contest starts soon\n", false, Utility.BuildContest(contest));
                            }
            }).Repeat(new CancellationTokenSource().Token, TimeSpan.FromMinutes(1));
        }
        public void AddUser(ulong discordId, string codeforcesHandle)
        {
            var jsonTxt = Utility.DownloadString($"http://codeforces.com/api/user.info?handles={codeforcesHandle}");
            var obj = new JavaScriptSerializer().Deserialize<UserObject>(jsonTxt);

            if (_dataManager.UserExists(discordId))
            {
                _userList[discordId] = obj.result[0];
            }
            else
            {
                if (obj.status == "OK")
                    _dataManager.AddUser(discordId, new JavaScriptSerializer().Serialize(obj.result[0]));
            }
        }

        public void Initialize()
        {
            Task.Factory.StartNew(() =>
            {
                _dataManager.Initialize();
                var users = _dataManager.GetAllUsers();
                while (users.Read())
                    _userList[Convert.ToUInt64(users["discordInfo"] as string)]
                        = new JavaScriptSerializer().Deserialize<User>(
                            EncryptionHelper.Decrypt(users["serializedData"] as string));

                //users = _dataManager.GetSubscribers();

                //while(users.Read())
                //{
                //    var x = users["discordId"].ToString();
                //    var discordUser = MyServer.Users.FirstOrDefault(i => i.Id == ulong.Parse(x));
                //    if (discordUser == null) continue;
                //    var discordId = discordUser.Username + '#' + discordUser.DiscriminatorValue;
                //    _userList[discordId].Subscriped = true;
                //}
            });
        }

        internal async Task RemoveRole(SocketGuildUser user)
        {
            try
            {
                await user.RemoveRolesAsync(user.Roles.Where(i =>
                    !new[] {"Moderator", "Administrator", "Virtual Participant", "@everyone"}.Any(i.Name.Contains)));
            }
            catch { }
        }
        internal async Task RemoveAllRoles()
        {
            foreach (var user in MyServer.Users)
                await RemoveRole(user);
        }
        internal async Task UpdateRole(ulong discordId, string role)
        {
            var srvUser = FindUser(discordId);

            var srvRole = MyServer.Roles.FirstOrDefault(i => i.Name == role);

            if (srvUser == null || srvRole == null)
                return;

            await RemoveRole(srvUser);
            await srvUser.AddRoleAsync(srvRole);
        }
        internal async Task UpdateSerliazedObjects()
        {
            await Task.Factory.StartNew(async () =>
            {
                await RemoveAllRoles();
                foreach (var user in _userList.ToArray())
                {
                    var jsonTxt =
                        Utility.DownloadString($"http://codeforces.com/api/user.info?handles={user.Value.handle}");
                    var obj = new JavaScriptSerializer().Deserialize<UserObject>(jsonTxt);

                    if(obj == null) continue;

                    if (obj.status == "OK")
                    {
                        _userList[user.Key] = obj.result[0];
                        _dataManager.UpdateUser(user.Key, new JavaScriptSerializer().Serialize(obj.result[0]));
                        var role = Utility.RolePicker(obj.result[0].rating);
                        await UpdateRole(user.Key, role);
                    }

                    await Task.Delay(500);
                }
            });
        }
        internal void RemoveSubscribe(ulong discordId)
        {
            _dataManager.RemoveSubscribe(discordId);
            var discordUser = MyServer.Users.FirstOrDefault(i => i.Id == discordId);

            if (discordUser == null) return;

            _userList[discordId].Subscriped = false;
        }
        internal void AddSubscribe(ulong discordId)
        {
            _dataManager.AddSubscribe(discordId);
            var discordUser = MyServer.Users.FirstOrDefault(i => i.Id == discordId);
            _userList[discordId].Subscriped = true;
        }
        internal async Task<string> VirtualContestPicker(string[] handles)
        {
            var picker = new VirtualContestPicker(handles);
            var res = await picker.Pick();
            return res;
        }
    }
}