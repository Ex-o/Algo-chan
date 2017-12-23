using System;
using System.Reflection;
using System.Threading.Tasks;
using algochan.OJ;
using algochan.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace algochan.Bot
{
    public class Algochan
    {
        public Algochan(string token, OjManager ojManager)
        {
            _token = token;
            _ojManager = ojManager;
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true
            });

            _client.Log += Logger;
            _client.Ready += InitCommands;
        }

        public async Task Run()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
            await _client.SetGameAsync("!help");
            await Task.Delay(-1);
        }

        private async Task InitCommands()
        {
            _servives = _map.AddSingleton(_client).AddSingleton(new UserManager(_client.Guilds, _ojManager))
                .AddSingleton(_ojManager).BuildServiceProvider();
            await _cmdservice.AddModulesAsync(Assembly.GetEntryAssembly());

            _client.MessageReceived += HandleCommand;
        }

        private async Task HandleCommand(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;

            var argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) ||
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            var context = new CommandContext(_client, message);
            var result = await _cmdservice.ExecuteAsync(context, argPos, _servives);


            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync("Something is not right!");
        }

        private Task Logger(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }

        #region Fields

        private readonly string _token;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdservice = new CommandService();
        private IServiceProvider _servives;
        private readonly IServiceCollection _map = new ServiceCollection();
        private readonly OjManager _ojManager;

        #endregion
    }
}