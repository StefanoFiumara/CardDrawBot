using System;
using System.Collections.Generic;

namespace CardDrawBot
{
    public static class Extensions
    {
        public static List<T> TakeRandom<T>(this List<T> list, int amount)
        {
            var rng = new Random();
            int resultCount = Math.Min(amount, list.Count);
            var result = new List<T>(resultCount);
            
            for (int i = 0; i < resultCount; i++)
            {
                var random = rng.Next(0, list.Count);
                var item = list[random];
                
                result.Add(item);
                list.Remove(item);
            }

            return result;
        }
    }
}