using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CardDrawBot
{
    public static class Utils
    {
        public static string FormatTable<T>(List<string> headers, List<T> items, Dictionary<string, Func<T, string>> headerPropertyMap, bool useIndex = false)
        {
            Debug.Assert(!headerPropertyMap.Keys.Except(headers).Any(), "Missing header mappings for FormatTable");

            var sb = new StringBuilder();
            sb.AppendLine("```");

            var formattedHeaders = CalculateColumnPadding(headers, items, headerPropertyMap, useIndex);

            sb.AppendLine(string.Join(" | ", formattedHeaders.Select(f => f.Value.paddedHeader)));

            var underlines = new List<string>();
            foreach (var header in formattedHeaders)
            {
                var line = new string('=', header.Value.padding);
                underlines.Add(line);
            }

            sb.AppendLine(string.Join(" | ", underlines));
            
            foreach (var item in items)
            {
                var formattedProps = new List<string>();

                if (useIndex)
                {
                    var padding = formattedHeaders["#"].padding;
                    formattedProps.Add($"{items.IndexOf(item) + 1}".PadRight(padding));
                }
                
                foreach (var header in headers)
                {
                    formattedProps.Add(headerPropertyMap[header](item).PadRight(formattedHeaders[header].padding));
                }

                sb.AppendLine(string.Join(" | ", formattedProps));
            }
            
            sb.AppendLine("```");
            return sb.ToString();
        }

        private static SortedDictionary<string, (string paddedHeader, int padding)> CalculateColumnPadding<T>(List<string> headers, List<T> items, Dictionary<string, Func<T, string>> headerPropertyMap, bool useIndex)
        {
            var comparer = new HeaderComparer(headers);
            var formattedHeaders = new SortedDictionary<string, (string paddedHeader, int padding)>(comparer);

            if (useIndex)
            {
                var width = items.Select(t => $"{items.IndexOf(t) + 1}".Length).Max() + 1;
                formattedHeaders.Add("#", ("#".PadRight(width), width));
            }
            
            foreach (var header in headers)
            {
                var width = items.Select(t => headerPropertyMap[header](t).Length).Max();
                width = width > header.Length ? width : header.Length;

                formattedHeaders.Add(header, (header.PadRight(width), width));
            }
            
            return formattedHeaders;
        }
    }
    
    public class HeaderComparer : IComparer<string>
    {
        private readonly List<string> _headers;

        public HeaderComparer(List<string> headers)
        {
            _headers = headers;
        }
        public int Compare(string x, string y)
        {
            return _headers.IndexOf(x).CompareTo(_headers.IndexOf(y));
        }
    }
}