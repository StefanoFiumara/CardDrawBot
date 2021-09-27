using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using CardDrawBot.Commands;

namespace CardDrawBot
{
    public static class Constants
    {
        // TODO: Do any of these need the pulled into a configuration file?
        public const string TOKEN_FILE = "DiscordToken.txt";
        public const ulong ADMIN_USER_ID = 104988834017607680; // Fano
        
        public static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static class Styles
        {
            public const string SOLO = "solo";
            public const string TEAM = "team";
        }

        public static class Difficulty
        {
            public const string BASIC = "basic";
            public const string EASY = "easy";
            public const string HARD = "hard";
            public const string WILD = "wild";
            public const string FULL = "full";
            public const string TEAM = "team";
            
        }
    }
    public static class Program
    {
        private static DiscordSocketClient _client;
        private static CommandService _commands;

        public static async Task Main(string[] args)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig() { AlwaysDownloadUsers = true });
            _commands = new CommandService();

            var serviceCollection = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<CommandHandler>()
                ;

            var services = serviceCollection.BuildServiceProvider();
            var commandHandler = services.GetService<CommandHandler>();
            Debug.Assert(commandHandler != null, nameof(commandHandler) + " != null");
            
            await commandHandler.InstallCommandsAsync();

            _client.Log += Log;

            var token = await File.ReadAllTextAsync(Constants.TOKEN_FILE);
            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        
        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}