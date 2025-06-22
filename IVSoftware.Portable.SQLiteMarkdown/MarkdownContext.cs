using System.Collections.Generic;
using System;
using System.IO.Pipes;
using SQLite;
using System.Linq;
using System.Text.RegularExpressions;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public class MarkdownContext
    {
        public MarkdownContext(string query, List<KeyValuePair<string, object>> args)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
            Args = args ?? new List<KeyValuePair<string, object>>();
        }

        public static implicit operator string(MarkdownContext context) => context.ToString();

        public string Query { get; }

        public string PositionalQuery =>
            Regex.Replace(Query, @"@\w+", "?");

        public object[] PositionalArgs =>
            Args.Select(kvp => kvp.Value).ToArray();

        public List<KeyValuePair<string, object>> Args { get; }

        public override string ToString()
        {
            string formatted = Query;
            foreach (var param in Args)
            {
                var value = param.Value is string str
                    ? $"'{str.Replace("'", "''")}'"
                    : param.Value?.ToString() ?? "NULL";

                formatted = formatted.Replace(param.Key, value);
            }
            return formatted;
        }
    }
}
