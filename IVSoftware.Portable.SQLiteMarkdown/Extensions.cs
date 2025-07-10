using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public enum StdAstNode
    {
        expr,
        ast,
        memo,
        term,
        and,
        or,
        not,
        sub,
    }
    public enum StdAstAttr
    {
        key,
        value,
        ismatched,
        run,
        args,
        clauseE,
        clauseN,
    }
    public enum QueryFilterMode
    {
        Query,
        Filter,
    }
    public static partial class Extensions
    {

        #region V E R S I O N    1 . 0

        public static string ParseSqlMarkdown<T>(
            this string @this,
            byte minInputLength,
            QueryFilterMode qfMode = QueryFilterMode.Query)
        {
            var _ = ValidationState.Valid;
            return @this.ParseSqlMarkdown(
                ref _,
                typeof(T),
                QueryFilterMode.Query,
                (expr) => Regex.IsMatch(expr, $@"[a-zA-Z0-9]{{{minInputLength}}}"),
                out XElement _);
        }


        public static string ParseSqlMarkdown<T>(
            this string @this,
            QueryFilterMode qfMode = QueryFilterMode.Query)
        {
            var validationState = ValidationState.Empty;
            return @this.ParseSqlMarkdown(
                ref validationState,
                typeof(T),
                qfMode,
                null,
                out _);
        }

        public static string ParseSqlMarkdown<T>(
            this string @this, 
            QueryFilterMode qfMode,
            out XElement xast)
        {
            var validationState = ValidationState.Empty;
            return @this.ParseSqlMarkdown(
                ref validationState,
                typeof(T),
                qfMode,
                null,
                out xast);
        }

        private static string ParseSqlMarkdown(
            this string @this,
            ref ValidationState validationState,
            Type type,
            QueryFilterMode qfMode,
            Predicate<string> validationPredicate,
            out XElement xexpr) => 
                new MarkdownContext(type)
                {
                    ValidationPredicate = validationPredicate,
                }.ParseSqlMarkdown(@this, type, qfMode, out xexpr);
        public static string ParseSqlMarkdown(this string @this, Type type, QueryFilterMode qfMode, out XElement xast)
            => new MarkdownContext(type).ParseSqlMarkdown(@this, type, qfMode, out xast);
        public static string ParseSqlMarkdown(this MarkdownContext @this, string expr, Type type, QueryFilterMode qfMode, out XElement xast)
            => @this.ParseSqlMarkdown(expr, type, qfMode, out xast);

        /// <summary>
        /// Non-breaking compatible filter term attribute getter.
        /// </summary>
        public static MarkdownTermAttribute GetQueryTermAttribute(this PropertyInfo pi)
            =>
            pi.GetCustomAttribute<QueryLikeTermAttribute>() is QueryLikeTermAttribute qlt
            ? qlt
            : pi.GetCustomAttribute<SqlLikeTermAttribute>();

        /// <summary>
        /// Non-breaking compatible filter term attribute getter.
        /// </summary>
        public static MarkdownTermAttribute GetFilterTermAttribute(this PropertyInfo pi)
            => 
            pi.GetCustomAttribute<FilterLikeTermAttribute>() is FilterLikeTermAttribute flt
            ? flt
            : pi.GetCustomAttribute<FilterContainsTermAttribute>();


        /// <summary>
        /// Non-breaking compatible filter term attribute getter.
        /// </summary>
        public static bool IsQueryTermAttribute(this MarkdownTermAttribute mta)
            => typeof(QueryLikeTermAttribute).IsAssignableFrom(mta.GetType());

        /// <summary>
        /// Non-breaking compatible filter term attribute getter.
        /// </summary>
        public static bool IsFilterTermAttribute(this MarkdownTermAttribute mta)
            => typeof(FilterLikeTermAttribute).IsAssignableFrom(mta.GetType());
        #endregion V E R S I O N    1 . 0

        static string ApplyCasing(this string s, StringCasing casingOption)
        {
            switch (casingOption)
            {
                case StringCasing.Original:
                    return s;
                case StringCasing.Lower:
                    return s.ToLower();
                case StringCasing.Upper:
                    return s.ToUpper();
                default:
                    return s;
            }
        }
        static string EncloseInSquareBrackets(this string s) => $"[{s.Trim()}]";

        public static string MakeTags(
            this string @this,
            TermDelimiter termDelimiter = TermDelimiter.Comma,
            StringCasing stringCasing = StringCasing.Lower)
        {
            // Example input: Tags = "C# .NET MAUI,C# WPF, C# WinForms".MakeTags()
            // ExampleOutput: 
            char delimiter;
            switch (termDelimiter)
            {
                default:
                case TermDelimiter.Comma:
                    delimiter = ',';
                    break;
                case TermDelimiter.Semicolon:
                    delimiter = ';';
                    break;
                case TermDelimiter.Tilde:
                    delimiter = '~';
                    break;
            }
            var split =
                @this
                .Split(delimiter)
                .Where(_ => !string.IsNullOrWhiteSpace(_));
            return string.Join(string.Empty, split.Select(_ => _.EncloseInSquareBrackets().ApplyCasing(stringCasing)));
        }

        /// <summary>
        /// Converts a plural word to its singular form by applying common English pluralization rules.
        /// Handles cases such as 'ies' to 'y', 'ves' to 'f', and words ending in 'oes' or 'es'.
        /// Ensures words ending in 'ss' are not incorrectly modified.
        /// </summary>
        /// <param name="word">The word to be singularized.</param>
        /// <returns>The singular form of the input word, or the original word if no changes are made.</returns>
        public static string Singularize(this string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            if (word.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 3) + "y";

            if (word.EndsWith("ves", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 3) + "f";

            if (word.EndsWith("oes", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 2);

            if (word.EndsWith("es", StringComparison.OrdinalIgnoreCase) && !word.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 2);

            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase) && word.Length > 1)
                return word.Substring(0, word.Length - 1);

            return word;
        }


        /// <summary>
        /// Forms a fuzzy query expression by checking and replacing terms enclosed in '%' characters 
        /// within the provided SQL string with their singular forms. If no terms can be singularized, 
        /// the original list is returned unchanged.
        /// </summary>
        /// <typeparam name="T">The type of the proxy used for sql gen.</typeparam>
        /// <param name="smart">The list of model instances to return if no modifications occur.</param>
        /// <param name="sql">The SQL query string containing candidate terms for singularization.</param>
        /// <returns>A list of model instances, either modified based on singularization or unchanged.</returns>

        public static string ToFuzzyQuery(this string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return sql;

            var terms = new List<string>();
            var fuzzySql = sql;

            foreach (var term in sql.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = term.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                var lengthBefore = trimmed.Length;
                var core = trimmed.Trim('[', ']');
                var isBracketed = lengthBefore - core.Length == 2;

                var singular = core.Singularize();
                if (singular != core)
                {
                    terms.Add(singular);
                    var original = $"%{(isBracketed ? "[" + core + "]" : core)}%";
                    var replacement = $"%{(isBracketed ? "[" + singular + "]" : singular)}%";
                    fuzzySql = fuzzySql.Replace(original, replacement);
                }
            }

            return fuzzySql;
        }

        public static string ToStringFromEventType(this SenderEventPair @this)
            => @this.e is null
            ? "null"
            : @this.e is PropertyChangedEventArgs epc
                ? epc.PropertyName
                : @this.e is NotifyCollectionChangedEventArgs ecc
                ? ecc.Action.ToString()
                : @this.e.ToString();

        /// <summary>
        /// Returns true if the input starts with '[' and is a valid JSON array of strings.
        /// Returns false if input is null, doesn't start with '[', or fails validation.
        /// </summary>
        /// <remarks>
        /// Constructs like "[canine] [color]" and "brown&dog" are valid
        /// in SQLiteMarkdown but will choke the JSON parser, so prequalify.
        /// </remarks>
        public static bool CanParseAsJson(this string @this)
            => Regex.IsMatch(@this, @"^\s*\[\s*(""(?:[^""\\]|\\.)*""\s*,\s*)*(""[^""\\]*""\s*)\]\s*$");
    }
}
