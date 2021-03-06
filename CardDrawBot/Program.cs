using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using CardDrawBot.Commands;

namespace CardDrawBot
{
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