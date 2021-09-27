using System.Collections.Generic;

namespace CardDrawBot.Models
{
    public class Song
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public string Bpm { get; set; }
        public string Jacket { get; set; }
        
        public List<Chart> Charts { get; set; }
    }
}