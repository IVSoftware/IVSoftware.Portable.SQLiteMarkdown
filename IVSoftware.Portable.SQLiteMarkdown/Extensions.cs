using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Generates SQL from a markdown expression using an ad hoc MarkdownContext (MDC).
        /// </summary>
        /// <remarks>
        /// Performs SQL generation only and does not participate in filtering or context reuse.
        ///
        /// CONFIG
        /// - QueryFilterConfig is locked to Query to prevent creation of the internal
        ///   FilterQueryDatabase and to ensure the operation remains parse-only.
        ///
        /// MODE
        /// - QueryFilterMode selects whether SQL is generated using [QueryLikeTerm] or
        ///   [FilterLikeTerm]. This routing affects SQL generation only and is distinct
        ///   from QueryFilterConfig.
        ///
        /// TYPE RESOLUTION
        /// - When invoked as a string extension there is no proxy indirection, so the
        ///   provided type serves as both ContractType and ProxyType.
        /// - This behavior is distinct from a persistent MDC, where parsing occurs 
        ///   against a ProxyType that may differ from the ContractType (but which still must
        ///   resolve to the same SQLite table as the ContractType).
        /// </remarks>
        [Canonical("@this.ParseSqlMarkdown - All string extensions come here and operate on an ad hoc MDC instance.")]
        public static string ParseSqlMarkdown(this string expr, Type type, QueryFilterMode qfMode, out XElement xast)
            => new MarkdownContext(type)
            {
                QueryFilterConfig = QueryFilterConfig.Query,    // For ad hoc parsing, be sure to lock out filtering. 
            }.ParseSqlMarkdown(
                expr, 
                proxyType: type,    // These are always the same thing for a string extension.
                qfMode,             // Not to be confused with QueryFilterConfig. The MDC can parse as-though filtering regardless.
                out xast);


        /// <summary>
        /// Performs SQL parsing only using a custom validation predicate.
        /// </summary>
        /// <remarks>
        /// CONFIG
        /// - The QueryFilterConfig is locked to Query to prevent instantiation of 
        ///   the built-in FilterQueryDatabase factory, or filtering against it.
        /// MODE
        /// - This is distinct from the QueryFilterMode argument, which is a router between
        ///   the [QueryLikeTerm] and the [FilterLikeTerm] which impacts the SQL generation.
        /// </remarks>
        [Canonical("Standalone target for string extensions to ParseSqlMarkdown with validation flow.")]
        private static string ParseSqlMarkdown(
            this string @this,
            ref ValidationState validationState,
            Type type,
            QueryFilterMode qfMode,
            Predicate<string>? validationPredicate,
            out XElement xexpr) =>
                new MarkdownContext(
                type)
                {
                    QueryFilterConfig = QueryFilterConfig.Query,    // For ad hoc parsing, be sure to lock out filtering. 
                    ValidationPredicate = validationPredicate!,     // Setting to null sets the VP to its default (which isn't null).
                }.ParseSqlMarkdown(@this, type, qfMode, out xexpr);

        /// <summary>
        /// Non-breaking compatible filter term attribute getter.
        /// </summary>
        public static MarkdownTermAttribute GetQueryTermAttribute(this PropertyInfo pi)
            =>
            pi.GetCustomAttribute<QueryLikeTermAttribute>() is QueryLikeTermAttribute qlt
            ? qlt
            : pi.GetCustomAttribute<SqlLikeTermAttribute>(); // Non-breaking compatible filter term

        /// <summary>
        /// Non-breaking compatible filter term attribute getter.
        /// </summary>
        public static MarkdownTermAttribute GetFilterTermAttribute(this PropertyInfo pi)
            =>
            pi.GetCustomAttribute<FilterLikeTermAttribute>() is FilterLikeTermAttribute flt
            ? flt
            : pi.GetCustomAttribute<FilterContainsTermAttribute>(); // Non-breaking compatible filter term


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

        /// <summary>
        /// Formats a delimited list of tag values into canonical bracketed form.
        /// </summary>
        /// <remarks>
        /// This method is a strict formatter and assumes delimiter-based input.
        /// It does not support or interpret bracket syntax. If the input contains
        /// square brackets, an exception is thrown to prevent ambiguous or mixed
        /// grammars.
        ///
        /// Behavior:
        /// - Splits the input string using a single specified delimiter.
        /// - Trims and filters empty tokens.
        /// - Wraps each token in square brackets.
        /// - Applies optional casing.
        /// - Concatenates all bracketed tokens without separators.
        ///
        /// Use <c>NormalizeTags</c> when grammar detection is required.
        /// </remarks>
        /// <param name="this">The input string containing delimited tag values.</param>
        /// <param name="termDelimiter">The delimiter used to split the input.</param>
        /// <param name="stringCasing">The casing applied to each tag value.</param>
        /// <returns>A canonical bracketed tag string.</returns>
        public static string MakeTags(
            this string @this,
            TermDelimiter termDelimiter = TermDelimiter.Comma,
            StringCasing stringCasing = StringCasing.Lower)
        {
            if (@this.Any(_ => _ == '[' || _ == ']'))
            {
                throw new ArgumentException("Square brackets must be removed before calling this method.");
            }
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

            var preview = 
                InsertSpaceBetweenTags
                ? string.Join(" ", split.Select(_ => _.EncloseInSquareBrackets().ApplyCasing(stringCasing)))
                : string.Join(string.Empty, split.Select(_ => _.EncloseInSquareBrackets().ApplyCasing(stringCasing)));
            return preview;
        }

        internal static bool InsertSpaceBetweenTags { get; set; } = true;
        /// <summary>
        /// Normalizes a tag string into canonical bracketed form by detecting
        /// the applicable tag grammar.
        /// </summary>
        /// <remarks>
        /// Grammar precedence:
        /// 1. Explicit bracket syntax ("[tag]") takes priority.
        /// 2. Known delimiters (comma, semicolon, tilde).
        /// 3. Whitespace as a fallback.
        ///
        /// The result is a concatenation of individually bracketed tokens
        /// with no separators. Delimiter-based parsing is delegated to
        /// <c>MakeTags</c>.
        /// </remarks>
        /// <param name="value">The raw tag input string.</param>
        /// <returns>A canonical bracketed tag string.</returns>
        public static string NormalizeTags(this string value)
        {
            string preview;
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // 1. Explicit bracket grammar wins
            if (TryParseBracketGrammar(value, out var bracketTokens))
            {
                preview =
                    InsertSpaceBetweenTags
                    ? string.Concat(bracketTokens.Select(t => $" [{t}]")).TrimStart()
                    : string.Concat(bracketTokens.Select(t => $"[{t}]"));
                return preview;
            }

            // 2. Known delimiter grammar
            foreach (TermDelimiter delimiter in Enum.GetValues(typeof(TermDelimiter)))
            {
                char c;
                switch (delimiter)
                {
                    case TermDelimiter.Comma:
                        c = ',';
                        break;
                    case TermDelimiter.Semicolon:
                        c = ';';
                        break;
                    case TermDelimiter.Tilde:
                        c = '~';
                        break;
                    default:
                        throw new NotImplementedException($"Bad case: {delimiter}");
                }
                if (value.Contains(c))
                {
                    return value.MakeTags(delimiter, StringCasing.Original);
                }
            }

            // 3. Whitespace grammar
            var tokens =
                value
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            preview = 
                InsertSpaceBetweenTags
                ? string.Concat(tokens.Select(t => $" [{t}]")).TrimStart()
                : string.Concat(tokens.Select(t => $"[{t}]"));
            return preview;
        }

        private static bool TryParseBracketGrammar(string input, out IEnumerable<string> tokens)
        {
            var matches = Regex.Matches(input, @"\[(.*?)\]");
            if (matches.Count == 0)
            {
                tokens = null;
                return false;
            }

            tokens =
                matches
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value);

            return true;
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

        /// <summary>
        /// Enumerates base classes of the specified subclass.
        /// </summary>
        public static IEnumerable<Type> BaseTypes(
            this Type type,
            bool includeSelf = false)
        {
            for (var current = includeSelf ? type : type.BaseType;
                 current is not null;
                 current = current.BaseType)
            {
                yield return current;
            }
        }

        internal static bool TryGetTableNameFromBaseClass(this Type @this, out string tableName, out Type baseClass)
        {
            foreach (var @base in @this.BaseTypes().Reverse())
            {
                tableName = @base.GetCustomAttribute<TableAttribute>(inherit: false)?.Name!;
                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    baseClass = @base;
                    return true;
                }
            }
            baseClass = @this;
            tableName = @this.GetCustomAttribute<TableAttribute>(
                inherit: false // CRITICAL! Does not produce an accurate result otherwise.
            )?.Name ?? @this.Name;
            return false;
        }

        internal static bool TryGetTableNameFromTableAttribute(this Type @this, out string tableName)
            => !string.IsNullOrWhiteSpace(tableName = @this.GetCustomAttribute<TableAttribute>(
                inherit: false // CRITICAL! Does not produce an accurate result otherwise.
        )?.Name!);

        /// <summary>
        /// Return string formatted as: json_extract(Properties, '$.Description');
        /// </summary>
        public static string JsonExtract(this string columnName, string key)
        {
            if (key.Contains('\''))
                columnName.ThrowHard<ArgumentException>("Key must not contain single quotes.", nameof(key));

            return $"json_extract({columnName}, '$.{key}')";
        }

        /// <summary>
        /// Nullable overload.
        /// </summary>
        public static DateTimeOffset? FloorToSecond(this DateTimeOffset? @this)
            => @this.HasValue
                ? @this.Value.FloorToSecond()
                : @this;

        /// <summary>
        /// Truncates sub-second precision (milliseconds and below).
        /// </summary>
        public static DateTimeOffset FloorToSecond(this DateTimeOffset @this)
            => new DateTimeOffset(
                @this.Ticks - (@this.Ticks % TimeSpan.TicksPerSecond),
                @this.Offset);

        /// <summary>
        /// Gets the first custom attribute of the specified type applied to an enum value.
        /// </summary>
        internal static TAttribute? GetCustomAttribute<TAttribute>(
            this Enum value)
            where TAttribute : Attribute
            => GetDeclaredEnumField(value)?
                .GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>()
                .FirstOrDefault();

        /// <summary>
        /// Gets all custom attributes of the specified type applied to an enum value.
        /// </summary>
        internal static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(
            this Enum value)
            where TAttribute : Attribute
            => GetDeclaredEnumField(value)?
               .GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>()
                ?? Enumerable.Empty<TAttribute>();

        /// <summary>
        /// Returns the declared FieldInfo corresponding to a defined enum value.
        /// </summary>
        /// <remarks>
        /// Ensures that the supplied enum value maps to a named constant declared on its type.
        /// If the value represents an undefined numeric backing (e.g., casted integral or invalid flag),
        /// an InvalidOperationException is thrown via ThrowHard.
        /// This method does not support composite flag values unless explicitly declared.
        /// </remarks>
        private static FieldInfo GetDeclaredEnumField(Enum value)
        {
            var type = value.GetType();

            if (Enum.IsDefined(type, value))
            {
                return type.GetField(value.ToString())!;
            }
            else
            {
                var numeric = Convert.ToInt64(value);
                value.ThrowHard<InvalidOperationException>(
                    $"Enum value '{value}' (underlying {numeric}) does not correspond to a declared field on '{type.FullName}'.");
                return null!; // We warned you.
            }
        }

        /// <summary>
        /// Produces the normalized semantic form of the input by trimming trailing
        /// whitespace and transient logical operators (!, |, &) unless they are
        /// explicitly escaped.
        /// </summary>
        public static string TrimEndTransients(this string? input)
        {
            input ??= string.Empty;
            var text = input.TrimEnd();
            bool hasTrailingEscapedOperator = false;

            if (text.Length >= 2)
            {
                int len = text.Length;
                char c1 = text[len - 2];
                char c2 = text[len - 1];

                if (c1 == '\\' && (c2 == '\\' || c2 == '!' || c2 == '|' || c2 == '&'))
                {
                    hasTrailingEscapedOperator = true;
                }
            }

            if (!hasTrailingEscapedOperator)
            {
                text = text.TrimEnd('!', '|', '&');
            }
            return text;
        }

        /// <summary>
        /// Returns true when the normalized semantic input contains no effective terms.
        /// </summary>
        public static bool IsSemanticallyEmpty(this string? input)
            => input.TrimEndTransients().Length == 0;

        /// <summary>
        /// Normalizes and concatenates materialized path segments using '\' as the canonical separator.
        /// </summary>
        /// <remarks>
        /// - Accepts null or mixed '/' and '\' input.
        /// - Trims whitespace.
        /// - Removes empty segments.
        /// - Always emits a canonical '\' separated path.
        /// - If root is null or empty, only segments are considered.
        /// </remarks>
        public static string LintCombinedSegments(this string? root, params string[] segments)
        {
            var concat = new List<string>();

            void Append(string? value)
            {
                var linted = localLintSinglePath(value);
                if (!string.IsNullOrEmpty(linted))
                {
                    concat.AddRange(linted.Split('\\'));
                }
            }

            // Root may be null. In that case it contributes nothing.
            if (!string.IsNullOrWhiteSpace(root))
            {
                Append(root);
            }

            if (segments != null)
            {
                foreach (var segment in segments)
                {
                    Append(segment);
                }
            }

            return string.Join("\\", concat);

            #region L o c a l F x
            string localLintSinglePath(string? path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return string.Empty;

                var parts = path
                    .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0);

                return string.Join("\\", parts);
            }
            #endregion
        }

        /// <summary>
        /// Produces a compact diagnostic string describing a collection change event.
        /// </summary>
        /// <remarks>
        /// Formats the source (projection vs other), action, and item counts for
        /// quick inspection during debugging and tracing.
        ///
        /// Reports <see cref="NotifyCollectionChangedEventArgs.NewItems"/> and
        /// <see cref="NotifyCollectionChangedEventArgs.OldItems"/> only when the
        /// respective IList is non-null. Empty collections are reported with a
        /// count of zero; null lists are omitted entirely.
        /// </remarks>
        public static string ToString(
            this NotifyCollectionChangedEventArgs e,
            bool isProjection)
        {
            var sb = new System.Text.StringBuilder();

            sb.Append($"{(isProjection ? "NetProjection" : "Other")}.{e.Action.ToString().PadRight(7)} ");

            if (e.NewItems is { } newItems)
            {
                sb.Append($"NewItems={newItems.Count.ToString().PadLeft(2)} ");
            }

            if (e.OldItems is { } oldItems)
            {
                sb.Append($"OldItems={oldItems.Count.ToString().PadLeft(2)} ");
            }

            if (e.NewStartingIndex != -1)
            {
                sb.Append($"NewIndex={e.NewStartingIndex.ToString().PadLeft(2)} ");
            }

            if (e.OldStartingIndex != -1)
            {
                sb.Append($"OldIndex={e.OldStartingIndex.ToString().PadLeft(2)} ");
            }

            sb.Append(e.GetType().Name.PadRight(43));

            return sb.ToString();
        }

        /// <summary>
        /// Formats an InternalsVisibleTo attribute for the assembly that defines this instance.
        /// </summary>
        /// <remarks>
        /// Emits the full strong-name public key (not the token).
        /// Throws if the assembly is not strong-named.
        /// Intended for generating friend assembly declarations.
        /// </remarks>
        public static string InternalsVisibleToFormatted(this object @this)
        {
            var asm = @this.GetType().Assembly;
            var name = asm.GetName();
            var publicKey = name.GetPublicKey();

            if (publicKey is null || publicKey.Length == 0)
                throw new InvalidOperationException("Assembly is not strong-named.");

            var hex = BitConverter
                .ToString(publicKey)
                .Replace("-", string.Empty)
                .ToLowerInvariant();

            return $@"[assembly: InternalsVisibleTo(""{name.Name}, PublicKey={hex}"")]";
        }

        /// <summary>
        /// Removes the specified attribute from all descendant elements,
        /// optionally including the current element.
        /// </summary>
        public static XElement RemoveDescendantAttributes(this XElement @this, Enum stdEnum, bool includeSelf = false)
        {
            var name = stdEnum.ToString();

            var query = (includeSelf
                ? @this.DescendantsAndSelf().Attributes(name)
                : @this.Descendants().Attributes(name))
                .ToArray();

            foreach (var attr in query.ToList())
            {
                if (attr.Parent is null)
                {   /* G T K */
                    // 260313 we're beginning to do this auto on the XElement.Changed events.
                }
                else
                {
                    attr.Remove();
                }
            }
            return @this;
        }

        /// <summary>
        /// Removes the specified attribute from all descendant elements,
        /// optionally including the current element.
        /// </summary>
        public static XElement RemoveDescendantAttributes(this XElement @this, Enum[] stdEnums, bool includeSelf = false)
        {
            var names = stdEnums.Select(e => e.ToString()).ToArray();

            var query = (includeSelf
                ? @this.DescendantsAndSelf()
                : @this.Descendants())
                .SelectMany(x => names.SelectMany(name => x.Attributes(name)))
                .ToArray();

            foreach (var attr in query)
            {
                if (attr.Parent is null)
                {   /* G T K */
                }
                else
                {
                    attr.Remove();
                }
            }
            return @this;
        }

        public static string[] GetTableNames(this SQLiteConnection @this)
        {
            var query = "SELECT name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%';";
            var preview = @this.QueryScalars<string>(query).ToArray();
            return preview;
        }

        public static bool TryGetWhereAttribute(this Enum @this, out string binding, out string expr, bool @throw = false)
        {
            if (@this.GetCustomAttribute<WhereAttribute>() is { } attr)
            {
                binding = attr.Binding;
                expr = attr.Expr;
                return true;
            }
            else
            {
                if (@throw)
                {
                    @this.ThrowSoft<InvalidOperationException>(
                        $"Advisory failed {nameof(TryGetWhereAttribute)}", @throw: @throw);
                }
                binding = expr = string.Empty;
                return false;
            }
        }
    }
}
