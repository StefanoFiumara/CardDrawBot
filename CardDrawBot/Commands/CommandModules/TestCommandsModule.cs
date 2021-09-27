using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CardDrawBot.Models;
using Discord;
using Discord.Commands;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CardDrawBot.Commands.CommandModules
{
    public class TestCommandsModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Emoji OkHand = new("\uD83D\uDC4C");
        private static readonly Emoji ThumbsDown = new("\uD83D\uDC4E");
        
        [Command("test")]
        [Summary("Test connection")]
        public async Task PingAsync()
        {
            var messageDetails = $"Received ping message in {Context.Channel.Name} from {Context.User.Mention}";
            await ReplyAsync(messageDetails);
        }

        [Command("draw")]
        public async Task DrawAsync(int numSongs = 5, string range = "15-27")
        {
            try
            {
                var json = await File.ReadAllTextAsync("SongData.json");
                var data = JsonSerializer.Deserialize<List<Song>>(json, Constants.SERIALIZER_OPTIONS);
                Debug.Assert(data != null, nameof(data) + " != null");

                // TODO: error handling and/or verify range parameter is formatted correctly.
                int min = int.Parse(range.Split("-")[0]);
                int max = int.Parse(range.Split("-")[1]);

                var filteredSongs = data
                    .Where(s => s.Charts
                        .Where(c => c.DiffClass is Constants.Difficulty.HARD or Constants.Difficulty.WILD)
                        .Any(c => c.Lvl >= min && c.Lvl <= max))
                    .ToList();

                // TODO: filter by other than hard/wild?
                foreach (var song in filteredSongs)
                {
                    song.Charts = song.Charts
                        .Where(c => c.DiffClass is Constants.Difficulty.HARD or Constants.Difficulty.WILD)
                        .Where(c => c.Lvl >= min && c.Lvl <= max)
                        .ToList();
                }

                var drawnSongs = filteredSongs.TakeRandom(numSongs);
            
                Chart DrawChart(Song s) 
                    => s.Charts
                        .TakeRandom(1)
                        .Single();

                var drawnCharts = drawnSongs.Select(s => (Song: s, Chart: DrawChart(s))).ToList();

                var charts = string.Join("\n", drawnCharts.Select((p, i) => $"{i+1}: {p.Song.Name} ({p.Chart.DiffClass} {p.Chart.Lvl})"));
                var reply = $"Here is your song draw:\n{charts}";

                await ReplyAsync(reply);
                await Context.Message.AddReactionAsync(OkHand);
            }
            catch (Exception e)
            {
                await Context.Message.AddReactionAsync(ThumbsDown);
                await ReplyAsync($"Sorry! Some unexpected error happened while generating your card draw! A crash log has be DM'd to the admins.\nError Message: {e.Message}");
                    
                var admin = Context.Client.GetUser(Constants.ADMIN_USER_ID);
                await admin.SendMessageAsync($"{Context.User.Username} ran into an unhandled exception while requesting a card draw.\nThe command they tried to execute was: `{Context.Message.Content}`\n\nFull exception: \n```\n{e}\n```");
            }

        }
    }
}