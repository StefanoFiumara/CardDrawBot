using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CardDrawBot.Models;
using Discord;
using Discord.Commands;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CardDrawBot.Commands.CommandModules
{
    public class CardDrawCommandsModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Emoji OkHand = new("\uD83D\uDC4C");
        private static readonly Emoji ThumbsDown = new("\uD83D\uDC4E");
       
        // TODO: Sub-commands for when we implement multiple games.
        //      e.g.
        //       * !draw smx 5 15-23
        //       * !draw ddra20 5 15-23
        //       * !draw extreme 5 15-23
        [Command("draw")]
        public async Task DrawAsync(int numSongs = 5, string range = "15-27", string diffFilter = "hw")
        {
            try
            {
                var json = await File.ReadAllTextAsync("SongData.json");
                var songData = JsonSerializer.Deserialize<List<Song>>(json, Constants.SERIALIZER_OPTIONS);
                Debug.Assert(songData != null, nameof(songData) + " != null");

                // TODO: error handling for the range parameter
                int min = int.Parse(range.Split("-")[0]);
                int max = int.Parse(range.Split("-")[1]);

                var diffs = Constants.Difficulty.GetDifficulties(diffFilter);
                
                var drawnCharts = DrawSongs(songData, numSongs, diffs, min, max);
                
                var opts = FormatDraftOptions(numSongs, min, max, diffs);
                var charts = FormatSongs(drawnCharts);

                var reply = $"Here is your song draw:\n{opts}{charts}";

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

        private static List<(Song Song, Chart Chart)> DrawSongs(List<Song> songData, int numSongs, List<string> diffs, int min, int max)
        {
            var filteredSongs = songData
                .Where(s => s.Charts
                    .Where(c => diffs.Contains(c.DiffClass))
                    .Any(c => c.Lvl >= min && c.Lvl <= max))
                .ToList();

            foreach (var song in filteredSongs)
            {
                song.Charts = song.Charts
                    .Where(c => diffs.Contains(c.DiffClass))
                    .Where(c => c.Lvl >= min && c.Lvl <= max)
                    .ToList();
            }

            var drawnSongs = filteredSongs.TakeRandom(numSongs);
            var drawnCharts = drawnSongs.Select(s => (Song: s, Chart: s.Charts.TakeRandom(1).Single())).ToList();

            return drawnCharts;
        }

        private static string FormatDraftOptions(int numSongs, int lvlMin, int lvlMax, List<string> difficulties)
        {
            var opts = new StringBuilder();
            opts.AppendLine("```");
            opts.AppendLine($"Song Count: {numSongs}");
            opts.AppendLine($"Level Range: {lvlMin}-{lvlMax}");
            opts.AppendLine($"Difficulties: {string.Join(", ", difficulties)}");
            opts.AppendLine("```");

            return opts.ToString();
        }
        private static string FormatSongs(List<(Song song, Chart chart)> songs)
        {
            var headers = new List<string> { "Name", "BPM", "Difficulty" };

            var headerPropMap = new Dictionary<string, Func<(Song s, Chart c), string>>()
            {
                { headers[0], t => t.s.Name },
                { headers[1], t => t.s.Bpm },
                { headers[2], t => $"{t.c.DiffClass} {t.c.Lvl}" },
            };

            return Utils.FormatTable(headers, songs, headerPropMap, useIndex: true);
        }
    }
}