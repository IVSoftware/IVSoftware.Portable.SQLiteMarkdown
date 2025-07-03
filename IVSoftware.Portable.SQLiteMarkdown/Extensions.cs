﻿using IVSoftware.Portable.Disposable;
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
        #region V E R S I O N    O R
        private static readonly Dictionary<Type, MarkdownContextOR> _contextCache = new Dictionary<Type, MarkdownContextOR>();

        /// <summary>
        /// Parses the given markdown expression into a complete SQL query string
        /// using default validation behavior. Suitable for one-shot query scenarios
        /// where no filter state needs to be preserved.
        /// </summary>
        /// <typeparam name="T">The target model type, which must be attributed for markdown indexing.</typeparam>
        /// <returns>A SQL query string with interpolated LIKE/TagMatch conditions.</returns>
        public static string ParseSqlMarkdown<T>(
            this string expr,
            byte minInputLength,
            IndexingMode indexingMode)
        {
            var _ = ValidationState.Valid;
            return expr.ParseSqlMarkdownOR<T>(ref _, null, minInputLength, indexingMode);
        }

#if false

        public static string ParseSqlMarkdown(this string @this, Type type, QueryFilterMode qfMode, out XElement xast)
            => new MarkdownContext(@this, type).ParseSqlMarkdown(@this, type, qfMode, out xast);
        public static string ParseSqlMarkdown(this MarkdownContext @this, string expr, Type type, QueryFilterMode qfMode, out XElement xast) 
            => @this.ParseSqlMarkdown(expr, type, qfMode, out xast);

#endif

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

        public static MarkdownContextOR ParseSqlMarkdown<T>(
            this string expr,
            IndexingMode indexingMode,
            ref ValidationState validationState)
            => ParseSqlMarkdownOR<T>(expr, ref validationState, null, 3, indexingMode);

        public static MarkdownContextOR ParseSqlMarkdown<T>(
            this string expr,
            ref ValidationState validationState,
            Predicate<string> validationPredicate = null)
            => ParseSqlMarkdownOR<T>(expr, ref validationState, validationPredicate, 3, IndexingMode.All);

        /// <summary>
        /// An adapter or maybe a prototype for how things are supposed to be!
        /// </summary>
        public static MarkdownContextOR ParseSqlMarkdown<T>(
            this string expr,
            ref SearchEntryState searchEntryState)
        {
            using (MarkdownContextOR.GetToken())
            {
                ValidationState validationState;
                switch (searchEntryState)
                {
                    case SearchEntryState.Cleared:
                        validationState = ValidationState.Empty;
                        break;
                    case SearchEntryState.QueryEmpty:
                        validationState = ValidationState.Empty;
                        break;
                    case SearchEntryState.QueryENB:
                        validationState = ValidationState.Invalid;
                        break;
                    case SearchEntryState.QueryEN:
                        validationState = ValidationState.Valid;
                        break;
                    case SearchEntryState.QueryCompleteNoResults:
                        validationState = ValidationState.Valid;
                        break;
                    case SearchEntryState.QueryCompleteWithResults:
                        validationState = ValidationState.Valid & ValidationState.DisableMinLength;
                        break;
                    default:
                        throw new NotImplementedException("ToDo");
                }
                return expr.ParseSqlMarkdownOR<T>(ref validationState, null, 0, IndexingMode.All);
            }
        }

        /// <summary>
        /// Parses the given markdown expression into a <see cref="MarkdownContextOR"/> containing 
        /// the SQL command text and query parameters. Designed for use in interactive workflows 
        /// where validation state and filter behavior must be tracked over time.
        /// </summary>
        /// <typeparam name="T">The target model type, which must be attributed for markdown indexing.</typeparam>
        /// <param name="expr">The expression string to parse.</param>
        /// <param name="validationState">
        /// An output value indicating whether the expression is valid, invalid, or empty.
        /// Use this to determine behavior transitions (e.g., from query mode to filter mode).
        /// </param>
        /// <param name="validationPredicate">
        /// An optional predicate to apply custom term validation rules (e.g., input length).
        /// If null, a default validation predicate is applied.
        /// </param>
        /// <returns>A <see cref="MarkdownContextOR"/> with SQL text and its corresponding parameters.</returns>
        private static MarkdownContextOR ParseSqlMarkdownOR<T>(
            this string expr,
            ref ValidationState validationState,
            Predicate<string> validationPredicate,
            byte minInputLength,
            IndexingMode indexingMode) =>
                ParseSqlMarkdownOR(
                    expr,
                    typeof(T),
                    ref validationState,
                    validationPredicate,
                    minInputLength,
                    indexingMode);

        private static MarkdownContextOR ParseSqlMarkdownOR(
            this string expr,
            Type type,
            ref ValidationState validationState,
            Predicate<string> validationPredicate,
            byte minInputLength,
            IndexingMode indexingMode)
        {
            using (MarkdownContextOR.GetToken())
            {

                if (validationState.HasFlag(ValidationState.DisableMinLength))
                {
                    minInputLength = 0;
                }

                #region I N P R O G
                bool hasSelfIndexed =
                    type
                    .GetProperties()
                    .Any(_ => Attribute.IsDefined(_, typeof(SelfIndexedAttribute)));
                #endregion I N P R O G

#if false
            if (!_contextCache.TryGetValue(type, out var context))
            {
                context = new MarkdownContextOR(type);
                _contextCache[type] = context;
            }
            throw new NotImplementedException();
#else
                if (validationPredicate is null)
                {
                    validationPredicate = Static.DefaultValidationPredicate;
                }
                string sql = null;
                List<KeyValuePair<string, object>> orderedArgs = new List<KeyValuePair<string, object>>();
                string lint = expr.Lint();

                if (string.IsNullOrWhiteSpace(lint))
                {
                    validationState = ValidationState.Empty;
                    return new MarkdownContextOR(string.Empty, orderedArgs); // Empty CommandText for empty input
                }
                else
                {
                    if (lint.TryTokenize(out var astNodes, minInputLength))
                    {
                        validationState = ValidationState.Valid;
                        sql = localMakeSqlCommand(astNodes);
                    }
                    else
                    {
                        validationState = ValidationState.Invalid;
                        return new MarkdownContextOR(string.Empty, orderedArgs); // Empty CommandText for invalid input
                    }
                }
                return new MarkdownContextOR(sql, orderedArgs);

                #region Local Methods
                string localMakeSqlCommand(ASTNode[] astNodes)
                {
                    int paramIndex = 1;

                    string table = type.GetCustomAttribute<TableAttribute>()?.Name ?? type.Name;

                    var indexProperties = new HashSet<PropertyInfo>();
                    var termProperties = new List<PropertyInfo>();
                    var tagProperties = new List<PropertyInfo>();

                    foreach (var pi in type.GetProperties())
                    {
                        foreach (var attr in
                                  pi
                                  .GetCustomAttributes<MarkdownTermAttribute>(inherit: true))
                        {
                            if (attr is SqlLikeTermAttribute)
                            {
                                indexProperties.Add(pi);
                                termProperties.Add(pi);
                            }
                            else if (attr is FilterContainsTermAttribute)
                            {
                                indexProperties.Add(pi);
                                termProperties.Add(pi);
                            }
                            else if (attr is TagMatchTermAttribute)
                            {
                                indexProperties.Add(pi);
                                tagProperties.Add(pi);
                            }
                        }
                    }


                    var builder = new List<string>();

                    if (indexProperties.Count == 0)
                    {
                        builder.Add($"SELECT * FROM {table}");
                    }
                    else
                    {
                        builder.Add($"SELECT * FROM {table} WHERE");

                        var astExpr = localBuildWhere();
                        if (!string.IsNullOrWhiteSpace(astExpr))
                        {
                            builder.Add(astExpr);
                        }

                        string localBuildWhere()
                        {
                            var conditions = new List<string>();
                            var currentOperator = " AND ";
                            bool lastWasNot = false;

                            foreach (var node in astNodes)
                            {
                                switch (node.ASTType)
                                {
                                    case NodeType.Term:
                                        var conditionBuilder = new List<string>();

                                        foreach (var pi in termProperties)
                                        {
                                            if (localExcludeByFlag(pi))
                                            {
                                                continue;
                                            }
                                            string paramName = $"@param{paramIndex++}";
                                            string paramValue = pi.GetCustomAttribute<TagMatchTermAttribute>() is null
                                                ? $"%{node.Value}%"
                                                : $"%[{node.Value}]%";

                                            orderedArgs.Add(new KeyValuePair<string, object>(paramName, paramValue));
                                            conditionBuilder.Add($"{pi.Name} LIKE {paramName}");
                                        }

                                        var termCondition = $"({string.Join(" OR ", conditionBuilder)})";

                                        if (lastWasNot)
                                        {
                                            conditions.Add($"NOT ({termCondition})");
                                            lastWasNot = false;
                                        }
                                        else
                                        {
                                            conditions.Add(termCondition);
                                        }
                                        break;
                                    case NodeType.Tag:
                                        var tagConditionBuilder = new List<string>();

                                        foreach (var pi in tagProperties)
                                        {
                                            if (localExcludeByFlag(pi))
                                            {
                                                continue;
                                            }
                                            if (!indexingMode.HasFlag(IndexingMode.TagMatchTerm)) continue;

                                            string paramName = $"@param{paramIndex++}";
                                            string paramValue = $"%[{node.Value}]%";

                                            orderedArgs.Add(new KeyValuePair<string, object>(paramName, paramValue));
                                            tagConditionBuilder.Add($"{pi.Name} LIKE {paramName}");
                                        }

                                        var tagCondition = $"({string.Join(" OR ", tagConditionBuilder)})";

                                        if (lastWasNot)
                                        {
                                            conditions.Add($"NOT ({tagCondition})");
                                            lastWasNot = false;
                                        }
                                        else
                                        {
                                            conditions.Add(tagCondition);
                                        }
                                        break;
                                    case NodeType.And:
                                        currentOperator = " AND ";
                                        break;
                                    case NodeType.Or:
                                        currentOperator = " OR ";
                                        break;
                                    case NodeType.Not:
                                        lastWasNot = true;
                                        break;
                                    case NodeType.Parenthesis:
                                        break;
                                    default:
                                        Debug.Fail("ADVISORY - Bad Case.");
                                        break;
                                }
                            }
                            return string.Join(currentOperator, conditions);
                            #region L o c a l F x

                            bool localExcludeByFlag(PropertyInfo pi)
                            {
                                switch (pi.Name)
                                {
                                    case nameof(ISelfIndexedMarkdown.QueryTerm):
                                        return (!indexingMode.HasFlag(IndexingMode.LikeTerm));
                                    case nameof(ISelfIndexedMarkdown.FilterTerm):
                                        return (!indexingMode.HasFlag(IndexingMode.ContainsTerm));
                                    case nameof(ISelfIndexedMarkdown.TagMatchTerm):
                                        return (!indexingMode.HasFlag(IndexingMode.TagMatchTerm));
                                    default: return false;
                                }
                            }
                            #endregion L o c a l F x

                        }
                    }

                    if (builder.Count == 3)
                    {
                        builder[1] = $"({builder[1]})";
                        builder[2] = $"({builder[2]})";
                    }

                    return string.Join(Environment.NewLine, builder);
                }
                #endregion Local Methods
#endif
            }
        }

        public static string Lint(this string expr, bool trim = false)
        {
            if (string.IsNullOrWhiteSpace(expr))
            {
                return string.Empty;
            }
            using (MarkdownContextOR.GetToken())
            {
#if DEBUG
                var exprOR = expr; // Reporting
#endif
                expr = expr.Trim();
                localRunQuoteLinter('"');
                localRunQuoteLinter('\'');


                // Collapse multiple spaces to a single space
                expr = Regex.Replace(expr, @"\s+", " ");

                // Normalize operators with proper spacing
                expr = Regex.Replace(expr, @"\s*\|\s*", "|");
                expr = Regex.Replace(expr, @"\s*&\s*", "&");
                expr = Regex.Replace(expr, @"\s*!\s*", "!");

                // Normalize repeated operators
                expr = Regex.Replace(expr, @"[&]{2,}", "&");
                expr = Regex.Replace(expr, @"[|]{2,}", "|");
                expr = Regex.Replace(expr, @"[!]{2,}", "!");

                // Convert remaining space between words into implicit AND
                expr = expr.Replace(' ', '&');

                // Restore atomic quote content.
                localRestoreAtomicQuoteContent();


                // Throw only on conflicting adjacent operators
                if (Regex.IsMatch(expr, @"(&\||\|&|!\||\|!|&!|!&)"))
                {
                    throw new InvalidOperationException("Conflicting adjacent logical operators are not allowed.");
                }


                return trim ? expr.Trim() : expr;

                #region L o c a l F x
                void localRunQuoteLinter(char quoteChar)
                {
                    lock (_lock)
                    {
                        StringBuilder
                            sbExpr = new StringBuilder(),
                            sbAtomic = new StringBuilder();
                        string key;
                        bool isQuoteOpen = false;

                        for (int i = 0; i < expr.Length; i++)
                        {
                            var c = expr[i];
                            if (c == quoteChar)
                            {
                                var run = localGetQuoteRun();
                                // Toggle
                                if (isQuoteOpen)
                                {
                                    if (sbAtomic.Length != 0)
                                    {
                                        key = "$" + Guid.NewGuid().ToString().ToLower() + "$";

                                        if (run > 0)
                                        {
                                            if (run % 2 == 0)
                                            {
                                                for (int j = 1; j < run; j++)
                                                {
                                                    sbAtomic.Append(quoteChar);
                                                    i++;
                                                }
                                            }
                                            else
                                            {
                                                for (int j = 1; j < run; j++)
                                                {
                                                    sbAtomic.Append(quoteChar);
                                                    i++;
                                                }
                                            }
                                            MarkdownContextOR.Atomics[key] = sbAtomic.ToString();
                                            sbAtomic.Clear();
                                            sbExpr.Append(key);     // Placeholder

                                            sbExpr.Append(quoteChar); // END by appending quote to expr at-large.
                                        }
                                        else
                                        {
                                            MarkdownContextOR.Atomics[key] = sbAtomic.ToString();
                                            sbAtomic.Clear();
                                            sbExpr.Append(key);
                                        }
                                    }
                                    isQuoteOpen = false;
                                }
                                else
                                {
                                    if (run > 0)
                                    {
                                        sbExpr.Append(quoteChar);
                                        if (run % 2 == 0)
                                        {
                                            for (int j = 1; j < run; j++)
                                            {
                                                sbExpr.Append(quoteChar);
                                                i++;
                                            }
                                            isQuoteOpen = false;
                                        }
                                        else
                                        {
                                            for (int j = 1; j < run; j++)
                                            {
                                                sbAtomic.Append(quoteChar);
                                                i++;
                                            }
                                            isQuoteOpen = true;
                                        }
                                    }
                                }

                                int localGetQuoteRun()
                                {
                                    int count = 0;
                                    int lookAhead = i;
                                    while (lookAhead < expr.Length && expr[lookAhead] == quoteChar)
                                    {
                                        count++;
                                        lookAhead++;
                                    }
                                    return count;
                                }
                            }
                            else
                            {
                                if (isQuoteOpen)
                                {
                                    sbAtomic.Append(c);
                                }
                                else
                                {
                                    sbExpr.Append(c);
                                }
                            }
                        }

                        if (isQuoteOpen)
                        {
                            sbExpr.Append(quoteChar);   // We swallowed this, thinking it was the first of a pair.
                            sbExpr.Append(sbAtomic);
                            sbAtomic.Clear();
                        }
                        expr = sbExpr.ToString();

#if DEBUG
                        Debug.WriteLine($"250619.A");
                        Debug.WriteLine($"{exprOR,-30} | {expr,-30}");
                        { }
#endif
                    }
                }
            }

            void localRestoreAtomicQuoteContent()
            {
                string exprB4;
                lock (_lock)
                {
                    restart:
                    foreach (var kvp in MarkdownContextOR.Atomics.ToArray())
                    {
                        var value = kvp.Value.Replace(@"""", "$dquote$");
                        exprB4 = expr;
                        if (!Equals(expr = expr.Replace(kvp.Key, value), exprB4))
                        {
                            goto restart;
                        }
                    }
                }
            }
            #endregion L o c a l F x
        }
        static object _lock = new object();

        public static bool PromptEachStep { get; set; }
        public static event EventHandler StackPrompt;
        class AtomicStack : Stack<KeyValuePair<string, string>>
        {
            public void Push(string key, string value)
            {
                var kvp = new KeyValuePair<string, string>(key, value);
                if (PromptEachStep)
                {
                    StackPrompt?.Invoke(kvp, EventArgs.Empty);
                }
                base.Push(kvp);
            }
            public new KeyValuePair<string, string> Pop()
            {
                var kvp = base.Pop();
                if (PromptEachStep)
                {
                    StackPrompt?.Invoke(kvp, EventArgs.Empty);
                }
                return kvp;
            }
        }
        public static string LintProbationary(this string expr, bool trim = false)
        {
            if (string.IsNullOrWhiteSpace(expr))
            {
                return string.Empty;
            }
            AtomicStack atomicStack = new AtomicStack();
#if DEBUG
            var exprOR = expr; // Reporting
#endif
            localAtomizeBrackets();
            localRunQuoteLinter('"');
            localRunQuoteLinter('\'');


            // Collapse multiple spaces to a single space
            expr = Regex.Replace(expr, @"\s+", " ");

            // Normalize operators with proper spacing
            expr = Regex.Replace(expr, @"\s*\|\s*", "|");
            expr = Regex.Replace(expr, @"\s*&\s*", "&");
            expr = Regex.Replace(expr, @"\s*!\s*", "!");

            // Convert remaining space between words into implicit AND
            expr = Regex.Replace(expr, @"(?<=\w) (?=\w)", "&");

            // Remove redundant operators
            expr = RemoveRedundantOperators(expr);

            // Restore atomic quote content.
            localRestoreAtomicQuoteContent();


            return trim ? expr.Trim() : expr;

            #region L o c a l F x
            void localAtomizeBrackets()
            {

            }

            void localRunQuoteLinter(char quoteChar)
            {
                StringBuilder
                    sbExpr = new StringBuilder(),
                    sbAtomic = new StringBuilder();
                string key;
                bool isQuoteOpen = false;

                for (int i = 0; i < expr.Length; i++)
                {
                    var c = expr[i];
                    if (c == quoteChar)
                    {
                        var run = localGetQuoteRun();
                        // Toggle
                        if (isQuoteOpen)
                        {
                            if (sbAtomic.Length != 0)
                            {
                                key = "$" + Guid.NewGuid().ToString().ToLower() + "$";

                                if (run > 0)
                                {
                                    if (run % 2 == 0)
                                    {
                                        for (int j = 0; j < run; j++)
                                        {
                                            sbAtomic.Append(quoteChar);
                                            i++;
                                        }
                                    }
                                    else
                                    {
                                        for (int j = 1; j < run; j++)
                                        {
                                            sbAtomic.Append(quoteChar);
                                            i++;
                                        }
                                    }
                                    atomicStack.Push(key, sbAtomic.ToString());
                                    sbAtomic.Clear();
                                    sbExpr.Append(key);     // Placeholder

                                    sbExpr.Append(quoteChar); // END by appending quote to expr at-large.
                                }
                                else
                                {
                                    atomicStack.Push(key, sbAtomic.ToString());
                                    sbAtomic.Clear();
                                    sbExpr.Append(key);
                                }
                            }
                            isQuoteOpen = false;
                        }
                        else
                        {
                            if (run > 0)
                            {
                                sbExpr.Append(quoteChar);
                                if (run % 2 == 0)
                                {
                                    for (int j = 1; j < run; j++)
                                    {
                                        sbExpr.Append(quoteChar);
                                        i++;
                                    }
                                    isQuoteOpen = false;
                                }
                                else
                                {
                                    for (int j = 1; j < run; j++)
                                    {
                                        sbAtomic.Append(quoteChar);
                                        i++;
                                    }
                                    isQuoteOpen = true;
                                }
                            }
                        }

                        int localGetQuoteRun()
                        {
                            int count = 0;
                            int lookAhead = i;
                            while (lookAhead < expr.Length && expr[lookAhead] == quoteChar)
                            {
                                count++;
                                lookAhead++;
                            }
                            return count;
                        }
                    }
                    else
                    {
                        if (isQuoteOpen)
                        {
                            sbAtomic.Append(c);
                        }
                        else
                        {
                            sbExpr.Append(c);
                        }
                    }
                }

                if (isQuoteOpen)
                {
                    sbExpr.Append(sbAtomic);
                    sbAtomic.Clear();
                }

#if DEBUG
                expr = sbExpr.ToString();
                Debug.WriteLine($"250619.A");
                Debug.WriteLine($"{exprOR,-30} | {expr,-30}");
                { }
#endif
            }

            void localRestoreAtomicQuoteContent()
            {
                while (atomicStack.Count > 0)
                {
                    var kvp = atomicStack.Pop();
                    expr = expr.Replace(kvp.Key, kvp.Value);
                }
            }
            #endregion L o c a l F x
        }

        private static string LintOperators(string expr, string pattern, char operatorChar)
        {
            // Replace patterns with the operator
            return Regex.Replace(expr, pattern, operatorChar.ToString());
        }
        private static string RemoveRedundantOperators(string expr)
        {
            // Remove redundant '&' or '|' at the beginning or end of the expression
            expr = expr.Trim();
            expr = Regex.Replace(expr, @"^[&|]+|[&|]+$", string.Empty);
            return expr;
        }
        private static string DelintLiteral(this string atomic)
        {
            if (atomic.Contains("&"))
            {
                Debug.Fail("ADVISORY - First Time.");
            }
            return atomic.Replace("&", " ");
        }
        public static bool TryTokenize(this string expr, out ASTNode[] result, int minInputLength = 3)
        {
#if DEBUG
            switch (expr)
            {
                case "b":
                    break;
                default:
                    break;
            }
#endif
            var tokens = new List<ASTNode>();
            bool benignUnmatched = false;
            if (string.IsNullOrWhiteSpace(expr))
            {
                result = Array.Empty<ASTNode>();
                return false;
            }
            else
            {
                expr = expr.Lint(trim: false);
                if (expr.CanParseAsJson())
                {
                    try
                    {
                        // ADVISORY
                        // - If you're breaking on throw here CHECK YOUR EXCEPTION SETTINGS.
                        // - This is an intentional throw.
                        var list = JsonConvert.DeserializeObject<List<string>>(expr);
                        if (list?.Count > 0)
                        {
                            result = list
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .Select(s => new ASTNode(NodeType.Term, s.Trim()))
                                .ToArray();
                            return result.Length > 0;
                        }
                    }
                    catch
                    {
                        // Fall through to normal path if not valid JSON
                        // for example when a tag value presents something like "[canine] [color]"
                    }
                }
                var stack = new Stack<ASTNode>();
                var currentToken = string.Empty;
                int length = expr.Length;

                for (int i = 0; i < length; i++)
                {
                    char c = expr[i];

                    if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }

                    switch (c)
                    {
                        case '&':
                            if (!string.IsNullOrEmpty(currentToken))
                            {
                                tokens.Add(new ASTNode(NodeType.Term, currentToken));
                                currentToken = string.Empty;
                            }
                            tokens.Add(new ASTNode(NodeType.And, "&"));
                            break;

                        case '|':
                            if (!string.IsNullOrEmpty(currentToken))
                            {
                                tokens.Add(new ASTNode(NodeType.Term, currentToken));
                                currentToken = string.Empty;
                            }
                            tokens.Add(new ASTNode(NodeType.Or, "|"));
                            break;

                        case '!':
                            if (!string.IsNullOrEmpty(currentToken))
                            {
                                tokens.Add(new ASTNode(NodeType.Term, currentToken));
                                currentToken = string.Empty;
                            }
                            tokens.Add(new ASTNode(NodeType.Not, "!"));
                            break;

                        case '(':
                            if (!string.IsNullOrEmpty(currentToken))
                            {
                                tokens.Add(new ASTNode(NodeType.Term, currentToken));
                                currentToken = string.Empty;
                            }
                            stack.Push(new ASTNode(NodeType.Parenthesis, "("));
                            break;

                        case ')':
                            if (!string.IsNullOrEmpty(currentToken))
                            {
                                tokens.Add(new ASTNode(NodeType.Term, currentToken));
                                currentToken = string.Empty;
                            }
                            while (stack.Count > 0)
                            {
                                var node = stack.Pop();
                                if (node.ASTType == NodeType.Parenthesis)
                                {
                                    break;
                                }
                                tokens.Add(node);
                            }
                            break;

                        case '[': // Start of a tag
                            if (!string.IsNullOrEmpty(currentToken))
                            {
                                tokens.Add(new ASTNode(NodeType.Term, currentToken));
                                currentToken = string.Empty;
                            }

                            // Collect the tag
                            int startIndex = i + 1; // Start just after '['
                            while (startIndex < length && expr[startIndex] != ']')
                            {
                                startIndex++;
                            }

                            // Ensure the tag was properly closed
                            if (startIndex < length)
                            {
                                var tagValue = expr.Substring(i + 1, startIndex - i - 1); // Extract tag value
                                tokens.Add(new ASTNode(NodeType.Tag, tagValue));
                                i = startIndex; // Move the index to the position of ']'
                            }
                            else
                            {
                                benignUnmatched = true;
                            }
                            break;


                        #region L I T E R A L S
                        // Atomic quote begin or end.
                        case '\'':  // Single
                        case '"':   // Double
                            char quoteChar = c;
                            int start = i + 1;
                            if (start < length && expr[start] == quoteChar)
                            {
                                // SHORT CIRCUIT THE LITERAL
                                // It's not an atomic quoted term, just a literal quote char
                                currentToken += quoteChar;
                                i++; // skip over the escape portion of the literal
                            }
                            else
                            {
                                int closingIndex = -1;

                                for (int j = start; j < length; j++)
                                {
                                    if (expr[j] == quoteChar)
                                    {
                                        closingIndex = j;
                                        break;
                                    }
                                }

                                if (closingIndex > start) // Found matching quote
                                {
                                    if (!string.IsNullOrEmpty(currentToken))
                                    {
                                        tokens.Add(new ASTNode(NodeType.Term, currentToken));
                                        currentToken = string.Empty;
                                    }

                                    string quoted = expr.Substring(start, closingIndex - start);
                                    tokens.Add(new ASTNode(NodeType.Term, quoted));
                                    i = closingIndex; // advance i to closing quote
                                }
                                else
                                {
                                    // No closing match – treat as literal
                                    currentToken += quoteChar;
                                }
                            }
                            break;
                        #endregion  L I T E R A L S

                        default:
                            currentToken += c;
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(currentToken))
                {
                    tokens.Add(new ASTNode(NodeType.Term, currentToken));
                }

                foreach (var token in tokens)
                {
                    switch (token.ASTType)
                    {
                        case NodeType.Term:
                        case NodeType.Tag:
                            // Now that is is an encapsulated term, we can 
                            // restore any literals inside an atomic quote.
                            token.Value =
                                token.Value
                                .Replace("$dquote$", @"""")
                                .Replace("$squote$", @"'");
                            break;
                        default:
                            break;
                    }
                }
            }

            result = tokens.ToArray();
            return
                result.Length > 0 &&
                (result.Any(_ => _.ASTType == NodeType.Term && _.Value.Length >= minInputLength) ||
                 result.Any(_ => _.ASTType == NodeType.Tag)) && !benignUnmatched;
        }
        public static string GenerateTerm(
            this ASTNode[] astNodes,
            NodeTypeFlags nodeTypes,
            StringCasing casing = StringCasing.Lower,
            TermDelimiter termStyle = TermDelimiter.Comma
        )
        {
            var distinctTerms = new HashSet<string>();

            switch (nodeTypes)
            {
                case NodeTypeFlags.Term:
                    distinctTerms.UnionWith(
                        astNodes
                        .Where(node => node.ASTType == NodeType.Term)
                         .Select(node => node.Value));
                    break;

                case NodeTypeFlags.Tag:
                    distinctTerms.UnionWith(
                        astNodes
                        .Where(node => node.ASTType == NodeType.Tag)
                        .Select(node => $"[{node.Value}]"));
                    break;

                case NodeTypeFlags.All:
                    distinctTerms.UnionWith(
                        astNodes
                        .Where(node => node.ASTType == NodeType.Term || node.ASTType == NodeType.Tag)
                        .Select(node => node.Value));
                    break;
            }
            string tokenized;
            switch (termStyle)
            {
                default:
                case TermDelimiter.Comma:
                    tokenized = string.Join("~", distinctTerms);
                    break;
                case TermDelimiter.Semicolon:
                    tokenized = string.Join("~", distinctTerms.Select(_ => localWrap(_)));
                    break;
            }
            switch (casing)
            {
                case StringCasing.Original:
                    return tokenized;
                default:
                case StringCasing.Lower:
                    return tokenized.ToLower();
                case StringCasing.Upper:
                    return tokenized.ToUpper();
            }

            #region L o c a l M e t h o d s
            string localWrap(string s) => $"[{s.Trim()}]";
            #endregion L o c a l M e t h o d s;
        }

#if false && SAVE
        public static MarkdownContextOR GenerateInterpolatedTerm(
            this ASTNode[] astNodes,
            NodeTypeFlags nodeTypes,
            StringCasing stringCasing = StringCasing.Lower
)
        {
            var distinctTerms = new HashSet<string>();
            var parameters = new List<KeyValuePair<string, object>>();
            var parameterIndex = 1;

            switch (nodeTypes)
            {
                case NodeTypeFlags.Term:
                    foreach (var node in astNodes)
                        if (node.ASTType == NodeType.Term)
                            distinctTerms.Add(node.Value);
                    break;

                case NodeTypeFlags.Tag:
                    foreach (var node in astNodes)
                        if (node.ASTType == NodeType.Tag)
                            distinctTerms.Add(node.Value);
                    break;

                case NodeTypeFlags.All:
                    foreach (var node in astNodes)
                        if (node.ASTType == NodeType.Term || node.ASTType == NodeType.Tag)
                            distinctTerms.Add(node.Value);
                    break;
            }

            var tokenized = string.Join("~", distinctTerms.Select(term =>
            {
                string paramName = $"@param{parameterIndex++}".ApplyCasing(stringCasing);
                parameters.Add(new KeyValuePair<string, object>(paramName, term));
                return paramName;
            }));

            tokenized = tokenized
                .Replace("$dquote$", @"""")
                .Replace("$squote$", @"'");
            return new MarkdownContextOR(tokenized, parameters);
        }
#endif

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
        /// <typeparam name="T">The type of the model implementing IOnePageModel.</typeparam>
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

        public static (string Query, object[] Args) ToPositional(this MarkdownContextOR context)
            => (context.PositionalQuery, context.PositionalArgs);

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
        #endregion V E R S I O N    O R

        #region V E R S I O N    1 . 0

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
                new MarkdownContext()
                {
                    ValidationPredicate = validationPredicate,
                }.ParseSqlMarkdown(@this, type, qfMode, out xexpr);
        public static string ParseSqlMarkdown(this string @this, Type type, QueryFilterMode qfMode, out XElement xast)
            => new MarkdownContext().ParseSqlMarkdown(@this, type, qfMode, out xast);
        public static string ParseSqlMarkdown(this MarkdownContext @this, string expr, Type type, QueryFilterMode qfMode, out XElement xast)
            => @this.ParseSqlMarkdown(expr, type, qfMode, out xast);
    }
    #endregion V E R S I O N    1 . 0
}
