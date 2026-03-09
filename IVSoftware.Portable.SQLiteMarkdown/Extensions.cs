using IVSoftware.Portable;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq;
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
            return string.Join(string.Empty, split.Select(_ => _.EncloseInSquareBrackets().ApplyCasing(stringCasing)));
        }

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
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // 1. Explicit bracket grammar wins
            if (TryParseBracketGrammar(value, out var bracketTokens))
            {
                return string.Concat(bracketTokens.Select(t => $"[{t}]"));
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

            return string.Concat(tokens.Select(t => $"[{t}]"));
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

        internal static bool TryGetTableNameFromBaseClass(this Type @this, out string tableName)
            => @this.TryGetTableNameFromBaseClass(out tableName, out _);

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
        public static TAttribute? GetCustomAttribute<TAttribute>(
            this Enum value)
            where TAttribute : Attribute
            => GetDeclaredEnumField(value)?
                .GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>()
                .FirstOrDefault();

        /// <summary>
        /// Gets all custom attributes of the specified type applied to an enum value.
        /// </summary>
        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(
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

            var query = includeSelf
                ? @this.DescendantsAndSelf().Attributes(name)
                : @this.Descendants().Attributes(name);

            foreach (var attr in query.ToList())
            {
                attr.Remove();
            }
            return @this;
        }

        public static string[] GetTableNames(this SQLiteConnection @this)
        {
            var query = "SELECT name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%';";
            var preview = @this.QueryScalars<string>(query).ToArray();
            return preview;
        }

        #region I N T E R N A L
#if ABSTRACT
        KEEP THESE INTERNAL.
        The use of @throw as ThrowOrAdvise is new, is awesome, and is something 
        that needs to be OFFICIALLY intergrated into the broader IVS Nuget. It
        is CRITICAL to avoid having it leak out of this package. Really.
#endif

        /// <summary>
        /// Returns the <see cref="XBoundAttribute"/> for the specified <see cref="StdMarkdownAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Resolves the attribute using the enum name. If the attribute is missing or not bound
        /// as an <see cref="XBoundAttribute"/>, behavior is governed by <paramref name="throw"/>.
        /// Advisory or unspecified returns <c>null</c>.
        /// </remarks>
        internal static XBoundAttribute? XBoundAttribute(
            this XElement @this,
            StdMarkdownAttribute stdEnum,   // This type *only* by design. Do not generalize to Enum.
            ThrowOrAdvise? @throw = null)
        {
            string msg;
            if (@this.Attribute(stdEnum.ToString()) is not XAttribute attr)
            {
                msg = $"The {stdEnum.ToFullKey()} attribute was not found.";
            }
            else if (attr is not XBoundAttribute xba)
            {
                msg = $"The {stdEnum.ToFullKey()} attribute exists but is not an {nameof(XBoundAttribute)}.";
            }
            else
            {
                return xba;
            }
            switch (@throw)
            {
                case ThrowOrAdvise.ThrowHard:
                    @this.ThrowHard<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.ThrowSoft:
                    @this.ThrowSoft<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.ThrowFramework:
                    @this.ThrowFramework<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.Advisory:
                    @this.Advisory(msg);
                    break;
                case null:
                default: break;
            }
            return null;
        }

        /// <summary>
        /// Returns the typed <c>Tag</c> value of the <see cref="XBoundAttribute"/> identified by the specified <see cref="StdMarkdownAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Resolves the attribute via <see cref="XBoundAttribute(XElement, StdMarkdownAttribute, ThrowOrAdvise?)"/>.
        /// If the attribute is missing, not bound, or its <c>Tag</c> is not assignable to
        /// <typeparamref name="T"/>, the result is <c>default</c>.
        /// </remarks>
        internal static T? XBoundAttributeValue<T>(
            this XElement @this,
            StdMarkdownAttribute stdEnum,   // This type *only* by design. Do not generalize to Enum.
            ThrowOrAdvise? @throw = null)
        {
            if(@this.XBoundAttribute(stdEnum, @throw) is { } xba && xba.Tag is T valueT)
            {
                return valueT;
            }
            return default;
        }

        /// <summary>
        /// Returns the <see cref="XAttribute"/> identified by the specified <see cref="Enum"/> key.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Enum member → canonical attribute name."
        /// - Resolves the attribute using the enum name as the attribute key.
        /// - When the attribute is not present, behavior is governed by <paramref name="throw"/>. 
        /// - Advisory or unspecified returns <c>null</c>.
        /// </remarks>
        internal static XAttribute? Attribute(
            this XElement @this, 
            Enum stdEnum, ThrowOrAdvise? @throw = null)
        {
            string msg;
            if (@this.Attribute(stdEnum.ToString()) is not XAttribute attr)
            {
                msg = $"The {stdEnum.ToFullKey()} attribute was not found.";
            }
            else
            {
                return attr;
            }
            switch (@throw)
            {
                case ThrowOrAdvise.ThrowHard:
                    @this.ThrowHard<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.ThrowSoft:
                    @this.ThrowSoft<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.ThrowFramework:
                    @this.ThrowFramework<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.Advisory:
                    @this.Advisory(msg);
                    break;
                case null:
                default: break;
            }
            return null;
        }

        /// <summary>
        /// Returns the attribute value converted to <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// When the attribute is missing or unusable, recovery proceeds as follows:
        /// 1. SAFE: If T is nullable, returns default(T).
        /// 2. SAFE: If @default is T, @default is returned as T.
        /// 3. ADVISED SAFE: If [DefaultValue] is supplied, attempt conversion.
        /// 4. ADVISED UNSAFE: Throw policy is invoked; if the throw is handled,
        ///    default(T) is returned after the notification.
        /// </remarks>
        internal static T? GetAttributeValue<T>(
            this XElement @this,
            Enum stdEnum,
            object? @default = null,
            ThrowOrAdvise? @throw = null)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // [Careful] Attribute method already has error handling!
            if (@this.Attribute(stdEnum, @throw) is { } attr)
            {
                try
                {
                    return localConvertValue(attr.Value, targetType);
                }
                catch(Exception ex)
                {
                    @this.RethrowFramework(ex);
                }
            }

            // If explicit default not supplied, consult [DefaultValue].
            // NOTE: This line is a minor duplication that keeps both reflections out of the hot path unless needed.
            @default ??= stdEnum.GetCustomAttribute<DefaultValueAttribute>()?.Value;

            // Direct default match (explicit @default or [DefaultValue])
            if (@default is T defaultT)
                return defaultT;

            // Attempt conversion of default
            if (@default is not null)
            {
                try
                {
                    return localConvertValue(@default, targetType);
                }
                catch
                {
                    // Ignore conversion failure and fall through to policy enforcement.
                }
            }

            // No usable value resolved; enforce non-nullable contract
            bool isNullable = targetType != typeof(T) || !typeof(T).IsValueType;

            if (!isNullable)
            {
                localThrowHelper($"Non-nullable type({typeof(T).Name}) requires {nameof(@default)}");
            }

            return default;

            #region L o c a l F x
            T localConvertValue(object? value, Type targetType)
            {
                var s = value?.ToString() ?? string.Empty;

                if (targetType == typeof(string))
                    return (T)(object)s;

                // Fast reject obvious non-numeric input when numeric expected
                if (value is string sVal && localIsNumericType(targetType))
                {
                    if (!double.TryParse(sVal, out _))
                    {
                        // If explicit default not supplied, consult [DefaultValue].
                        // NOTE: This line is a minor duplication that keeps both reflections out of the hot path unless needed.
                        @default ??= stdEnum.GetCustomAttribute<DefaultValueAttribute>()?.Value;
                        if(@default is T defaultT)
                        {
                            return defaultT;
                        }
                        else
                        {
                            // If no explicit @throw level then throw hard.
                            @throw ??= ThrowOrAdvise.ThrowHard;
                            localThrowHelper($"The string provided '{sVal}' is not numeric.");
                            return default!; // We warned you.
                        }
                    }
                }

                if (targetType.IsEnum)
                    return (T)Enum.Parse(targetType, s, ignoreCase: true);

                return (T)Convert.ChangeType(value, targetType);
            }

            static bool localIsNumericType(Type t)
            {
                return t == typeof(byte) ||
                       t == typeof(sbyte) ||
                       t == typeof(short) ||
                       t == typeof(ushort) ||
                       t == typeof(int) ||
                       t == typeof(uint) ||
                       t == typeof(long) ||
                       t == typeof(ulong) ||
                       t == typeof(float) ||
                       t == typeof(double) ||
                       t == typeof(decimal);
            }

            void localThrowHelper(string msg)
            {
                switch (@throw ?? ThrowOrAdvise.ThrowHard)
                {
                    case ThrowOrAdvise.ThrowHard:
                        @this.ThrowHard<InvalidOperationException>(msg);
                        break;
                    case ThrowOrAdvise.ThrowSoft:
                        @this.ThrowSoft<InvalidOperationException>(msg);
                        break;
                    case ThrowOrAdvise.ThrowFramework:
                        @this.ThrowFramework<InvalidOperationException>(msg);
                        break;
                    case ThrowOrAdvise.Advisory:
                        @this.Advisory(msg);
                        break;
                }
            }
            #endregion
        }

        /// <summary>
        /// Assigns the value of the attribute identified by the specified <see cref="StdMarkdownAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Write canonical attribute text with controlled length semantics."
        /// The value is converted to string and written using the enum name as the attribute key.       
        /// - When the resulting string exceeds <paramref name="maxLength"/>, the value is truncated
        ///   and the configured throw policy is invoked.
        /// - When <paramref name="padToMaxLength"/> is <c>true</c>, the value is right-padded to the specified length.
        /// </remarks>
        internal static void SetAttributeValue(
            this XElement @this,
            StdMarkdownAttribute stdEnum,
            object? value,
            byte maxLength = byte.MaxValue,
            bool padToMaxLength = false,
            ThrowOrAdvise? @throw = null)
        {
            if (value is null)
            {
                @this.SetAttributeValue(stdEnum.ToString(), null);
                return;
            }

            string @string = value.ToString() ?? string.Empty;
            if (@string.Length > maxLength)
            {
                @string = @string.Substring(0, maxLength);

                var msg = $"Value for {stdEnum.ToFullKey()} exceeded {maxLength} characters and has been truncated.";
                switch (@throw)
                {
                    case ThrowOrAdvise.ThrowHard:
                        @this.ThrowHard<InvalidOperationException>(msg);
                        break;
                    case ThrowOrAdvise.ThrowSoft:
                        @this.ThrowSoft<InvalidOperationException>(msg);
                        break;
                    case ThrowOrAdvise.ThrowFramework:
                        @this.ThrowFramework<InvalidOperationException>(msg);
                        break;
                    case ThrowOrAdvise.Advisory:
                        @this.Advisory(msg);
                        break;
                    case null:
                    default:
                        break;
                }
            }
            else if(padToMaxLength)
            {
                @string = @string.PadRight(maxLength);
            }
            @this.SetAttributeValue(stdEnum.ToString(), @string);
        }

        // Fluent event attacher. Internal only; goes in Collections, with Preview semantics.
        internal static T WithCollectionChangeHandler<T>(this T @this, NotifyCollectionChangedEventHandler handler)
            where T: INotifyCollectionChanged
        {
            ((INotifyCollectionChanged)@this).CollectionChanged += handler;
            return @this;
        }
        #endregion I N T E R N A L
    }

    public static class SQLiteConnectionMapper
    {
        public static TableMapping GetSQLiteMapping(
            this Type type,
            CreateFlags createFlags = CreateFlags.None,
            Type? contractType = null)
            => type.GetSQLiteMapping(out _, out _, createFlags, contractType);
        public static TableMapping GetSQLiteMapping(
            this Type type,
            out string? pkName,
            out string? pkPropertyName,
            CreateFlags createFlags = CreateFlags.None,
            Type? contractType = null)
        {
            TableMapping? mapping = null;

            if (contractType is null)
            {
                foreach (var @base in type.BaseTypes(includeSelf: true))
                {
                    if (@base.GetCustomAttribute<SQLite.TableAttribute>(inherit: false) is not null)
                    {
                        mapping = Mapper.GetMapping(@base, createFlags);
                        break;
                    }
                }
            }
            else
            {
                mapping = Mapper.GetMapping(contractType);
                if (!contractType.IsAssignableFrom(type))
                {
                    TableMapping aspirant;
                    foreach (var @base in type.BaseTypes(includeSelf: true).Reverse())
                    {
                        if (@base.GetCustomAttribute<SQLite.TableAttribute>(inherit: false) is not null)
                        {
                            aspirant = Mapper.GetMapping(@base, createFlags);
                            if(aspirant.TableName == mapping.TableName)
                            {

                            }
                            else
                            {

                            }
                            break;
                        }
                    }
                }
            }
            if(mapping is null)
            {
                // Did not find any explicit [Table] tags.
                mapping = Mapper.GetMapping(type);
                Debug.Assert(
                    type.Name == mapping.TableName, 
                    "Expecting tha name of the Type will be used as the table name.");
            }
            pkName = mapping.PK?.Name;
            pkPropertyName = mapping.PK?.PropertyName;
            return mapping;

        }
        static SQLiteConnection Mapper
        {
            get
            {
                if (_mapper is null)
                {
                    _mapper = new SQLiteConnection(":memory:");
                }
                return _mapper;
            }
        }
        static SQLiteConnection? _mapper = null;
    }
}
