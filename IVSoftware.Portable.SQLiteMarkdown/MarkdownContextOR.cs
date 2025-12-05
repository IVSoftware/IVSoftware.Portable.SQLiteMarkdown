using System.Collections.Generic;
using System;
using System.IO.Pipes;
using SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using IVSoftware.Portable.Disposable;
using System.Diagnostics;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public class MarkdownContextOR
    {
        public MarkdownContextOR(string query, List<KeyValuePair<string, object>> args)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
            Args = args ?? new List<KeyValuePair<string, object>>();
        }
#if false && SAVE
        public MarkdownContextOR(string query, List<KeyValuePair<string, object>> args)
        {
            if (DHostAtomic.IsZero())
            {
                Query = query ?? throw new ArgumentNullException(nameof(query));
                Args = args ?? new List<KeyValuePair<string, object>>();
            }
            else
            {
                lock (_lock)
                {
                    Query = query ?? throw new ArgumentNullException(nameof(query));
                    Args =
                        args
                        ?? new List<KeyValuePair<string, object>>();

                    for (int i = 0; i < args.Count; i++)
                    {
                        var kvp = args[i];
                        if (kvp.Value is string encoded)
                        {
                            var expr = encoded;
                            restart:
                            foreach (var decoder in Atomics)
                            {
                                var exprB4 = expr;
                                expr = expr.Replace(decoder.Key, decoder.Value);
                                if (expr != exprB4)
                                {
                                    // Atomics is not ordered, so recheck all.
                                    goto restart;
                                }
                            }
                            args[i] = new KeyValuePair<string, object>(kvp.Key, expr);
                        }
                    }
                }
            }
        }
#endif

        public static Dictionary<string, string> Atomics = new Dictionary<string, string>();

        /// <summary>
        /// Clear values from the Atomics dictionary when we're done with them.
        /// </summary>
        static DisposableHost DHostAtomic
        {
            get
            {
                if (_dhostAtomic is null)
                {
                    _dhostAtomic = new DisposableHost();
                    _dhostAtomic.FinalDispose += (sender, e) => Atomics.Clear();
                }
                return _dhostAtomic;
            }
        }
        static DisposableHost _dhostAtomic = null;
        static object _lock = new object();

        public static implicit operator string(MarkdownContextOR context) => context.ToString();

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

        public static IDisposable GetToken() => DHostAtomic.GetToken();
    }
}
