using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using cfapi.Objects;
using cfapi.Methods;
using algochan.Helpers;
using algochan.OJ;
using Discord;
using Discord.WebSocket;

namespace algochan.Services
{
    public class UserManager : IDisposable
    {
        #region Fields
        private const long CpcId = 326795829664808960;
        private readonly Dictionary<ulong, User> _userList = new Dictionary<ulong, User>();
        private readonly List<ulong> _subscribersList = new List<ulong>();
        private readonly Dictionary<int, DateTime> _contestNotificationsHistory = new Dictionary<int, DateTime>();
        private readonly OjManager _ojManager;
        private readonly DataManager _dataManager = new DataManager();
        private readonly IReadOnlyCollection<SocketGuild> _servers;
        //private DateTime time = new DateTime();
        #endregion
        private SocketGuild MyServer => _servers.FirstOrDefault(i => i.Id == CpcId);
        public bool UserExists(ulong discordId) => _dataManager.UserExists(discordId);
        public bool ProblemExists(ulong discordId, string problemUrl) => _dataManager.ProblemExists(discordId, problemUrl);
        public void AddFavoriteProblem(ulong discordId, string problemUrl) => _dataManager.AddFavoriteProblem(discordId, problemUrl);
        public SocketGuildUser FindUser(ulong userDiscordId, ulong serverId) => _servers.FirstOrDefault(i => i.Id == serverId).Users.FirstOrDefault(i => i.Id == userDiscordId);
        private SocketGuildUser FindUser(int userDiscriminator) => MyServer.Users.FirstOrDefault(i => i.DiscriminatorValue == userDiscriminator);

        public UserManager(IReadOnlyCollection<SocketGuild> servers, OjManager ojManager)
        {
            _servers = servers;
            _ojManager = ojManager;

            Initialize();
        }
        public User GetUser(ulong discordId) => _userList[discordId];
        public void NotifyCheck()
        {
            if (Globals.Initialized)
                return;

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        _ojManager.GetJudgesList().First(i => i.Name == "codeforces").ReloadContests();
                        var contests = _ojManager.GetContests(OnlineJudge.CF);
                        foreach (var contest in contests)
                        {
                            if (!_contestNotificationsHistory.ContainsKey(contest.Id))
                            {
                                _contestNotificationsHistory.Add(contest.Id, DateTime.UtcNow - new TimeSpan(0, 30, 45));
                            }
                            var minutesLeft = (Utility.FromUnixTime(contest.StartTime) - DateTime.UtcNow).TotalMinutes;
                            var minutesFromLastNotification = (DateTime.UtcNow - _contestNotificationsHistory[contest.Id]).TotalMinutes;
                            if (minutesLeft <= 60)
                                if ((minutesFromLastNotification >= 20 || (minutesLeft > 0 && minutesLeft <= 3)) && Utility.FromUnixTime(contest.StartTime) > DateTime.UtcNow)
                                {
                                    foreach (var user in _userList)
                                    {
                                        try
                                        {
                                            if (!_subscribersList.Contains(user.Key)) continue;

                                            var guildUser =
                                                MyServer.Users.FirstOrDefault(i => i.Id == user.Key);

                                            if (guildUser == null)
                                                continue;

                                            var channel = guildUser.GetOrCreateDMChannelAsync().Result;

                                            if (channel == null)
                                                continue;
                                            await channel.SendMessageAsync($"{contest.Name} starts in ``{(int)(Utility.FromUnixTime(contest.StartTime) - DateTime.UtcNow).TotalMinutes} minutes``.\n", false, Utility.BuildContest(contest));
                                            await Task.Delay(50);
                                        }
                                        catch
                                        {
                                         
                                        }
                                    }
                                    _contestNotificationsHistory[contest.Id] = DateTime.UtcNow;
                                    await _servers.First(i => i.Id == CpcId).TextChannels.First(i => i.Name == "general-cp").SendMessageAsync($"{contest.Name} starts in ``{(int)(Utility.FromUnixTime(contest.StartTime) - DateTime.UtcNow).TotalMinutes} minutes``.\n", false, Utility.BuildContest(contest));
                                    
                                }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString() + "\nNotifications fucked up!");
                    }
                    await Task.Delay(60000 * 5);
                }
            });
        }

        internal async Task TrollNoam()
        {
            foreach(var user in MyServer.Users)
            {
                await user.ModifyAsync(x => { x.Nickname = "Noam orz"; });
            }
        }
        public async Task AddUser(ulong discordId, string codeforcesHandle)
        {
            var req = new UserInfoRequest();
            var user = await req.GetUserInfoAsync(codeforcesHandle);

            if (user == null) return;

            if (_dataManager.UserExists(discordId))
            {
                _dataManager.UpdateUser(discordId, new JavaScriptSerializer().Serialize(user));
                _userList[discordId] = user;
            }
            else
            {
                _dataManager.AddUser(discordId, new JavaScriptSerializer().Serialize(user));
                _userList[discordId] = user;
            }
        }

        public void Initialize()
        {
            
            Task.Factory.StartNew(() =>
            {
                _dataManager.Initialize();
                var users = _dataManager.GetAllUsers();
                while (users.Read())
                {
                    try
                    {
                        var discordId = Convert.ToUInt64(users["discordInfo"] as string);
                        _userList[discordId]
                            = new JavaScriptSerializer().Deserialize<User>(
                                EncryptionHelper.Decrypt(users["serializedData"] as string));
                    }
                    catch { }
                }

                _dataManager.Migrate();
                users = _dataManager.GetSubscribers();

                while (users.Read())
                {
                    var discordId = (Convert.ToUInt64(users["discordId"]));

                    if (discordId != 0)
                        _subscribersList.Add((ulong)discordId);
                }
                Console.WriteLine("Initilization Complete!");
                NotifyCheck();
                Globals.Initialized = true;
            });
            
        }
        public List<string> GetAllFavorites(ulong discordId)
        {
            var ret = new List<string>();
            var favorites = _dataManager.GetAllFavorites(discordId);
            while (favorites.Read())
                ret.Add(favorites["problem"] as string);

            return ret;
        }
        internal async Task RemoveRole(SocketGuildUser user)
        {
            try
            {
                await user.RemoveRolesAsync(user.Roles.Where(i =>
                    new[] {"Pupil", "Specialist", "Expert", "Candidate Master", "Master", "International Master", "Grandmaster", "International Grandmaster", "Legendary Grandmaster", "Newbie"}.Any(i.Name.Contains)));
            }
            catch { }
        }
        internal async Task RemoveRole(ulong userId)
        {
            try
            {
                var user = MyServer.Users.FirstOrDefault(i => i.Id == userId);
                await user.RemoveRolesAsync(user.Roles.Where(i =>
                    new[] { "Pupil", "Specialist", "Expert", "Candidate Master", "Master", "International Master", "Grandmaster", "International Grandmaster", "Legendary Grandmaster", "Newbie" }.Any(i.Name.Contains)));
            }
            catch { }
        }
        internal async Task RemoveAllRoles()
        {   
            foreach (var server in _servers)
            foreach (var user in server.Users)
            {
                try
                {
                    await RemoveRole(user);
                }
                catch { }
            }
        }
        internal async Task UpdateRole(ulong discordId, string role)
        {
            foreach(var server in _servers)
            {
                var srvUser = server.Users.FirstOrDefault(i => i.Id == discordId);
                var srvRole = server.Roles.FirstOrDefault(i => i.Name == role);

                if (srvUser == null || srvRole == null)
                    continue;

                try
                {
                    if (srvUser.Roles.Count(i => i.Name == srvRole.Name) != 0)
                        continue;

                    if (srvRole.Name != "Real Div1" && srvRole.Name != "1500+")
                    {

                        await RemoveRole(srvUser);
                    }

                    await Task.Delay(250);
                    await srvUser.AddRoleAsync(srvRole);
                }
                catch
                {
                }    
            }
        }

        internal async Task AddRealDiv1()
        {
            await Task.Factory.StartNew(async () =>
           {
               foreach (var user in MyServer.Users)
               {
                   if (_userList.ContainsKey(user.Id) && _userList[user.Id].Rating > 1500)
                   {
                       await UpdateRole(user.Id, "1500+");
                       await Task.Delay(1000);
                   }

                   //var roles = MyServer.GetUser(user.Id)?.Roles;

                   //foreach (var role in roles)
                   //{
                   //    if (role.Name == "Candidate Master" ||
                   //        role.Name == "Master" ||
                   //        role.Name == "International Master" ||
                   //        role.Name == "Grandmaster" ||
                   //        role.Name == "International Grandmaster" ||
                   //        role.Name == "Legendary Grandmaster")
                   //    {
                   //        await UpdateRole(user.Id, "Real Div1");
                   //        break;
                   //    }
                   //}
                   
               }
           });
        }

        internal async Task FixChangedHandles()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var user in _userList.ToArray())
                {
                    HttpWebRequest request =
                        (HttpWebRequest) WebRequest.Create($"http://codeforces.com/profile/{user.Value.Handle}");
                    request.AllowAutoRedirect = false;
                    using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    {
                        Console.WriteLine(
                            $"http://codeforces.com/profile/{user.Value.Handle} ===> {response.Headers["Location"]}");

                        if(response.Headers["Location"] == null) continue;
                        
                        var data = response.Headers["Location"].Split('/');
                        user.Value.Handle = data.Last();
                        _dataManager.UpdateUser(user.Key, new JavaScriptSerializer().Serialize(user.Value));
                    }

                }
            });
        }
        internal async Task UpdateSerliazedObjects()
        {
            await Task.Factory.StartNew(async () =>
            {
                //await RemoveAllRoles();
                foreach (var user in _userList.ToArray())
                {
                    var req = new UserInfoRequest();
                    var obj = await req.GetUserInfoAsync(user.Value.Handle);

                    if(obj == null) continue;

                    _userList[user.Key] = obj;
                    _dataManager.UpdateUser(user.Key, new JavaScriptSerializer().Serialize(obj));
                    var role = Utility.RolePicker(obj.Rating);
                    await UpdateRole(user.Key, role);
                    
                    if (role == "Candidate Master" ||
                           role == "Master" ||
                           role == "International Master" ||
                           role == "Grandmaster" ||
                           role == "International Grandmaster" ||
                           role == "Legendary Grandmaster")
                    {
                        await UpdateRole(user.Key, "Real Div1");
                    }

                    if(user.Value.Rating > 1500)
                    {
                        await UpdateRole(user.Key, "1500+");
                    }

                    Console.WriteLine($"Updated: [{user.Value.Handle}][{user.Key}][{role}]");
                }
            });
        }
        internal void RemoveSubscribe(ulong discordId)
        {
            _dataManager.RemoveSubscribe(discordId);
            var discordUser = MyServer.Users.FirstOrDefault(i => i.Id == discordId);
            _subscribersList.Remove(discordId);
            if (discordUser == null) return;

            //_userList[discordId].Subscriped = false;
        }
        internal void AddSubscribe(ulong discordId)
        {
            _dataManager.AddSubscribe(discordId);
            //var discordUser = MyServer.Users.FirstOrDefault(i => i.Id == discordId);
            //if(_userList.ContainsKey(discordId))
            //    _userList[discordId].Subscriped = true;
            //else
            //{
            //    _userList.Add(discordId, new User() { Subscriped = true });
            //}
        }
        internal async Task<List<KeyValuePair<int , int>>> VirtualContestPicker(string[] handles)
        {
            var picker = new VirtualContestPicker(handles);
            var res = await picker.Pick();
            return res;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}