using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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

            public static List<string> GetDifficulties(string diffFilter)
            {
                return new List<string>
                    {
                        BASIC, EASY, HARD, WILD, FULL, TEAM
                    }.Join(diffFilter, 
                        s => char.ToLower(s[0]), 
                        c => c, 
                        (s, c) => s)
                    .ToList();
            }
            
        }
    }
}