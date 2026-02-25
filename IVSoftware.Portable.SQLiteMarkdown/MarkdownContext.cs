using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Represents the full parsing and transformation context for converting a user
    /// expression  (in lightweight markdown syntax) into an executable SQL WHERE
    /// clause by accumulating  parsing state, managing AST nodes, tracking substitutions,
    /// and guiding final SQL generation via attribute-aware hydration.
    /// </summary>
    [DebuggerDisplay("ContractType={ContractType} ProxyType={ProxyType}")]
    public partial class MarkdownContext
        : WatchdogTimer
        , IMarkdownContext
        , INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a self-contained expression parsing environment, binding it
        /// to an XML-based AST using the IVSoftware.Portable.XBoundObject NuGet.
        /// </summary>
        /// <remarks>
        /// [Careful] 
        /// The initial type must be known and captured regardless of GF mode.
        /// A parameterless CTOR would not be appropriate so avoid that.
        /// </remarks>
        [Canonical]
        public MarkdownContext(Type type)
        {
            Interval = TimeSpan.FromSeconds(0.25);          // Default. Consumer can change.
            XAST = new
                XElement(nameof(StdAstNode.ast))
                .WithBoundAttributeValue(this);
            ContractType = type;
        }

        private Dictionary<string, object> _args { get; } = new Dictionary<string, object>();

        public string ParseSqlMarkdown<T>(string expr, QueryFilterMode qfMode = QueryFilterMode.Query)
            => ParseSqlMarkdown(
                expr, typeof(T),
                qfMode,
                out XElement _);

        /// <summary>
        /// Parse the current InputText expr and build it against ContractType.
        /// </summary>
        public string ParseSqlMarkdown()
            => ParseSqlMarkdown(
                InputText,
                ContractType!,  // ThrowHard if null
                IsFiltering ? QueryFilterMode.Filter : QueryFilterMode.Query,
                out XElement _);

        /// <summary>
        /// Run from internal values using a supplied ProxyType
        /// </summary>
        public string ParseSqlMarkdown<T>()
            => ParseSqlMarkdown(
                InputText,
                typeof(T),
                IsFiltering ? QueryFilterMode.Filter : QueryFilterMode.Query,
                out XElement _);

        /// <summary>
        /// Canonical entry point for parsing an expression against a specified proxy type and query/filter mode.
        /// </summary>
        /// <remarks>
        /// Resolves the effective table identity for the pass, builds the AST, and produces the final SQL expression.
        /// This overload is the semantic root used by all other ParseSqlMarkdown entry points.
        /// 
        /// The supplied proxyType must evaluate to the canonical contract table name. This is achieved by:
        /// 1. Omitting a [Table] attribute so inheritance resolution applies, or
        /// 2. Declaring an explicit [Table] attribute that agrees with the canonical base contract.
        /// 
        /// Conflicting table declarations are handled according to ContractErrorLevel.
        /// </remarks>
        [Canonical]
        public string ParseSqlMarkdown(string expr, Type proxyType, QueryFilterMode qfMode, out XElement xast)
        {
            if (proxyType is null)
            {
                this.ThrowHard<ArgumentNullException>(("Proxy type must be specified"));
                xast = null!; // We warned you.
                return string.Empty;
            }
#if DEBUG
            if (expr == "animal b")
            { }
#endif
            // Guard reentrancy by making sure to clear previous passes.
            XAST.RemoveNodes();
            _activeQFMode = qfMode;
            if (ValidationPredicate?.Invoke(expr) == false)
            {
                xast = XAST;
                return string.Empty;
            }

            // Properties are valid for the current parse flow
            // and are *not* bound to the ContractType.
            Raw = expr;
            Transform = Raw;
            TableName = ResolveTableNameForPass(proxyType);

            Preamble = $"SELECT * FROM {TableName} WHERE";

            xast = XAST;
            uint quoteId = 0xFEFE0000;
            uint tagId = 0xFDFD0000;
            _args.Clear();

            var indexingMode = (IndexingMode)(DHostSelfIndexing[nameof(IndexingMode)] ?? 0);
            if (indexingMode == 0)
            {
                localEscape();
            }
            localRunTagLinter();

            if (indexingMode == 0)
            {
                localRunQuoteLinter('"');
            }
            localRunQuoteLinter('\'');
            localLint();
            localRemoveTransientTrailingOperator();
            localBuildAST();
            return localBuildExpression();

            #region L o c a l F x
            // Preemptive explicit escapes (query syntax)
            void localEscape()
            {
                // Escape map — from raw escape sequence to literal character
                var escapes = new Dictionary<string, string>
                {
                    [@"\&"] = "&",
                    [@"\|"] = "|",
                    [@"\!"] = "!",
                    [@"\("] = "(",
                    [@"\)"] = ")",
                    [@"\["] = "[",
                    [@"\]"] = "]",
                    [@"\'"] = "'",
                    [@"\"""] = "\"",
                    [@"\\"] = "\\"
                };

                // Ensure these are defined in your class
                // int quoteId = 0; — must be field, not local
                // Dictionary<string, string> Atomics = new(); — must be field
                // string Transform = some raw input — must be field or property

                string hydrated = Transform;

                foreach (var kvp in escapes)
                {
                    string escSeq = kvp.Key;
                    string literalChar = kvp.Value;

                    int index = hydrated.IndexOf(escSeq, StringComparison.Ordinal);
                    while (index >= 0)
                    {
                        string key = $"${quoteId++:X8}$";
                        hydrated = hydrated.Substring(0, index) + key + hydrated.Substring(index + escSeq.Length);

                        Atomics[key] = literalChar;

                        // Continue looking for next instance
                        index = hydrated.IndexOf(escSeq, index + key.Length, StringComparison.Ordinal);
                    }
                }

                Transform = hydrated;

                lock (_lock)
                {
                    this.OnAwaited(new AwaitedEventArgs(caller: nameof(localEscape)));
                }
            }

            void localRunTagLinter()
            {
                string
                    exprLocal = Transform,
                    exprOR = exprLocal,
                    key;
                lock (_lock)
                {
                    StringBuilder
                        sbExpr = new StringBuilder(),
                        sbTag = new StringBuilder();
                    bool isTagOpen = false;

                    for (int i = 0; i < exprLocal.Length; i++)
                    {
                        var c = exprLocal[i];
                        switch (c)
                        {
                            case '[':
                                sbTag.Append(c);
                                isTagOpen = true; // And sure, it might already be open in which case '[' is a literal char inside the tag.
                                break;
                            case ']':
                                if (isTagOpen)
                                {
                                    sbTag.Append(c);
                                    isTagOpen = false;
                                    key = $"${tagId++:X8}$";
                                    sbExpr.Append(key);
                                    Atomics[key] = sbTag.ToString();
                                    sbTag.Clear();
                                }
                                else
                                {
                                    sbTag.Append(c);
                                }
                                break;
                            default:
                                if (isTagOpen)
                                {
                                    sbTag.Append(c);
                                }
                                else
                                {
                                    sbExpr.Append(c);
                                }
                                break;
                        }
                    }

                    if (isTagOpen)
                    {
                        sbExpr.Append(sbTag);
                        sbTag.Clear();
                    }
                    exprLocal = sbExpr.ToString();
                    Transform = exprLocal;
                }
            }

            void localRunQuoteLinter(char quoteChar)
            {
                string
                    exprLocal = Transform,
                    exprOR = exprLocal;
                lock (_lock)
                {
                    StringBuilder
                        sbExpr = new StringBuilder(),
                        sbAtomic = new StringBuilder();
                    string key;
                    bool isQuoteOpen = false;

                    for (int i = 0; i < exprLocal.Length; i++)
                    {
                        var c = exprLocal[i];
                        if (c == quoteChar)
                        {
                            var run = localGetQuoteRun();
                            // Toggle
                            if (isQuoteOpen)
                            {
                                if (sbAtomic.Length != 0)
                                {
                                    key = $"${quoteId++:X8}$";

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
                                        Atomics[key] = sbAtomic.ToString();
                                        sbAtomic.Clear();
                                        sbExpr.Append(key);     // Placeholder
                                    }
                                    else
                                    {
                                        Atomics[key] = sbAtomic.ToString();
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
                                while (lookAhead < exprLocal.Length && exprLocal[lookAhead] == quoteChar)
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
                    exprLocal = sbExpr.ToString();

#if DEBUG && false && SAVE
                    Debug.WriteLine($"250619.A");
                    Debug.WriteLine($"{exprOR,-30} | {exprLocal,-30}");
                    { }
#endif
                    Transform = exprLocal;
                }
            }

            // Reduce to no-redundant, non-conflicting operations.
            void localLint()
            {
#if DEBUG
                var b4 = Transform;
#endif
                if (string.IsNullOrWhiteSpace(Transform))
                {
                    return;
                }
                else
                {
                    if (indexingMode == 0)
                    {
                        localLintQuerySyntaxEnabled();
                    }
                    else
                    {
                        localLintQuerySyntaxDisabled();
                    }
                }

                void localLintQuerySyntaxEnabled()
                {
                    Transform = Transform.Trim();
                    // Collapse multiple spaces to a single space
                    Transform = Regex.Replace(Transform, @"\s+", " ");

                    // Normalize operators with proper spacing
                    Transform = Regex.Replace(Transform, @"\s*\|\s*", "|");
                    Transform = Regex.Replace(Transform, @"\s*&\s*", "&");
                    Transform = Regex.Replace(Transform, @"\s*!\s*", "!");

                    // Normalize repeated operators
                    Transform = Regex.Replace(Transform, @"[&]{2,}", "&");
                    Transform = Regex.Replace(Transform, @"[|]{2,}", "|");
                    Transform = Regex.Replace(Transform, @"[!]{2,}", "!");
                    // Insert explicit AND before '!' if it's a negated group and not already spaced
                    Transform = Regex.Replace(Transform, @"(?<!^)(?<![&|!(])!", "&!");

                    // Convert remaining space between words into implicit AND
                    Transform = Transform.Replace(' ', '&');


                    // Throw if adjacent operators conflict
                    if (Regex.IsMatch(Transform, @"(&\||\|&|!\||\|!|!&)"))
                    {
                        throw new InvalidOperationException("Conflicting adjacent logical operators are not allowed.");
                    }

                    lock (_lock)
                    {
                        this.OnAwaited(new AwaitedEventArgs(caller: nameof(localLint)));
                    }
#if false && DEBUG && SAVE
                        if (b4 != Transform)
                        {
                            Debug.WriteLine($"250705 {b4} -> {Transform}");
                        }
#endif
                }

                void localLintQuerySyntaxDisabled()
                {
                    Transform = Transform.Trim();

                    // Collapse all whitespace runs to a single space.
                    Transform = Regex.Replace(Transform, @"\s+", " ");

                    // In term mode, space means token boundary only.
                    // No operator normalization. No conflict detection.
                    // We are not parsing a language here.
                    Transform = Transform.Replace(' ', '&');

                    lock (_lock)
                    {
                        this.OnAwaited(new AwaitedEventArgs(caller: nameof(localLint)));
                    }
#if false && DEBUG && SAVE
    if (b4 != Transform)
    {
        Debug.WriteLine($"250705 TERM {b4} -> {Transform}");
    }
#endif
                }
            }

            // When query syntax is enabled, suppress trailing operators
            // with the assumption that they're a transient of IME runtime.
            void localRemoveTransientTrailingOperator()
            {
                if (indexingMode == 0 && !string.IsNullOrEmpty(Transform))
                {
                    var wk = Transform;

                    while (wk.Length > 0)
                    {
                        char last = wk[wk.Length - 1];

                        // Operators that require a right-hand operand.
                        if (last == '&' || last == '|' || last == '!' || last == '\\')
                        {
                            wk = wk.Substring(0, wk.Length - 1);
                            continue;
                        }
                        break;
                    }
                    Transform = wk;
                }
            }

            // Support nested expressions
            void localBuildAST()
            {
                char c;
                char cp; // Character preview
                int i, j;
                var sb = new StringBuilder();
                XElement
                    xcurrent = XAST,
                    xprev = xcurrent;
                XElement
                    xopen;

                if (indexingMode == 0)
                {
                    localBuildQuerySyntaxEnabledAST();
                }
                else
                {
                    localBuildQuerySyntaxDisabledAST();     // Fails 12 tests
                    // localBuildQuerySyntaxEnabledAST();   // Original: fails only one test
                }

                lock (_lock)
                {
                    this.OnAwaited(new AwaitedEventArgs(caller: nameof(localBuildAST)));
                }

                #region L o c a l F x

                // Return true if loop can continue (i.e. end not detected).
                bool localAppendChar(char newChar, char previewChar)
                {
                    bool isLastChar;
                    if (indexingMode == 0)
                    {
                        isLastChar =
                            previewChar == '\0'
                            ? true
                            : @"&|!()[]\".Contains(previewChar); // The '#' makes null not found.
                    }
                    else
                    {
                        isLastChar =
                            previewChar == '\0'
                            ? true
                            : previewChar == '&';
                    }


                    if (sb.Length == 0)
                    {
                        xprev = xcurrent;
                        xcurrent = new XElement(nameof(StdAstNode.term));
                    }
                    sb.Append(newChar);
                    if (isLastChar)
                    {
                        xcurrent.SetAttributeValue(nameof(StdAstAttr.value), sb);
                        if (DHostSelfIndexing[nameof(IndexingMode)] is IndexingMode selfIndexingMode)
                        {
                            Debug.Assert(!DHostSelfIndexing.IsZero(), "Expecting to 'not' have to check this!");
                            #region S E L F    I N D E X I N G    S U P P O R T
                            var rawTerm = sb.ToString();
                            bool isTag = rawTerm.StartsWith("$FDFD") && rawTerm.Length == 10;
                            bool indexingHandled = false;

                            // Rehydration is still necessary for atomic particles that are 'not' tags!.
                            string rehydrated = Rehydrate(rawTerm);
                            if (isTag)
                            {
                                if (!rehydrated.Contains(','))
                                {
                                    switch (selfIndexingMode)
                                    {
                                        case IndexingMode.QueryLikeTerm:
                                            _parsedIndexTerms[IndexingMode.QueryLikeTerm].Add(rehydrated);
                                            break;
                                        case IndexingMode.FilterLikeTerm:
                                            _parsedIndexTerms[IndexingMode.FilterLikeTerm].Add(rehydrated);
                                            break;
                                        case IndexingMode.TagMatchTerm:
                                            _parsedIndexTerms[IndexingMode.TagMatchTerm].Add(rehydrated.Trim("[]".ToCharArray()));
                                            break;
                                    }
                                    indexingHandled = true;
                                }
                            }
                            if (!indexingHandled)
                            {
                                if (rehydrated.CanParseAsJson())
                                {
                                    var terms = JsonConvert.DeserializeObject<List<string>>(rehydrated);

                                    foreach (var t in terms)
                                    {
                                        var atom = t.Trim();
                                        if (string.IsNullOrWhiteSpace(atom)) continue;

                                        switch (selfIndexingMode)
                                        {
                                            case IndexingMode.QueryLikeTerm:
                                                _parsedIndexTerms[IndexingMode.QueryLikeTerm].Add(atom);
                                                break;
                                            case IndexingMode.FilterLikeTerm:
                                                _parsedIndexTerms[IndexingMode.FilterLikeTerm].Add(atom);
                                                break;
                                            case IndexingMode.TagMatchTerm:
                                                _parsedIndexTerms[IndexingMode.TagMatchTerm].Add(atom.Trim("[]".ToCharArray()));
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    switch (selfIndexingMode)
                                    {
                                        case IndexingMode.QueryLikeTerm:
                                            _parsedIndexTerms[IndexingMode.QueryLikeTerm].Add(rehydrated.ToLower());
                                            break;
                                        case IndexingMode.FilterLikeTerm:
                                            _parsedIndexTerms[IndexingMode.FilterLikeTerm].Add(rehydrated.ToLower());
                                            break;
                                        case IndexingMode.TagMatchTerm:
                                            _parsedIndexTerms[IndexingMode.TagMatchTerm].Add(rehydrated.Trim("[]".ToCharArray()).ToLower());
                                            break;
                                    }
                                }
                            }
                        }
                        #endregion S E L F    I N D E X I N G    S U P P O R T
                        sb.Clear();
                        return false;
                    }
                    else
                    {
                        return true; // Keep going
                    }
                }

                void localBuildQuerySyntaxEnabledAST()
                {
                    for (i = 0, j = 1; i < Transform.Length; i++, j++)
                    {
                        c = Transform[i];
                        cp =
                            j == Transform.Length
                            ? '\0'
                            : Transform[j];

                        switch (c)
                        {
                            case '&':
                                xprev = xcurrent;
                                xcurrent = new XElement(nameof(StdAstNode.and));
                                break;
                            case '|':
                                xprev = xcurrent;
                                xcurrent = new XElement(nameof(StdAstNode.or));
                                break;
                            case '!':
                                xprev = xcurrent;
                                xcurrent = new XElement(nameof(StdAstNode.not));
                                break;
                            case '(':
                                xprev = xcurrent;
                                xcurrent = new XElement(nameof(StdAstNode.sub), new XAttribute(nameof(StdAstAttr.ismatched), bool.FalseString));
                                break;
                            case ')':
                                xopen = xcurrent.Ancestors().FirstOrDefault(_ => _.Name.LocalName == nameof(StdAstNode.sub));
                                if (xopen is null)
                                {
                                    // Literal - add to character stream.
                                    sb.Append(c);
                                }
                                else
                                {
                                    xopen.SetAttributeValue(nameof(StdAstAttr.ismatched), bool.TrueString);
                                }
                                continue;   // <- Different!
                            default:
                                if (localAppendChar(c, cp))
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                        }
                        xprev.Add(xcurrent);

                        // Detect unary 'not'
                        if (xcurrent.Name.LocalName != nameof(StdAstNode.sub) &&
                            xprev.Name.LocalName == nameof(StdAstNode.not) &&
                            xprev.Parent != null)
                        {
                            xcurrent = xprev.Parent;
                        }
                    }
                }
                void localBuildQuerySyntaxDisabledAST()
                {
                    for (i = 0, j = 1; i < Transform.Length; i++, j++)
                    {
                        c = Transform[i];
                        cp =
                            j == Transform.Length
                            ? '\0'
                            : Transform[j];

                        switch (c)
                        {
                            case '&':
                                xprev = xcurrent;
                                xcurrent = new XElement(nameof(StdAstNode.and));
                                break;
#if false
                            case '|':
                                xprev = xcurrent;
                                xcurrent = new XElement(nameof(StdAstNode.or));
                                break;
                            case '!':
                                xprev = xcurrent;
                                xcurrent = new XElement(nameof(StdAstNode.not));
                                break;
                            case '(':
                                xprev = xcurrent;
                                xcurrent = new XElement(nameof(StdAstNode.sub), new XAttribute(nameof(StdAstAttr.ismatched), bool.FalseString));
                                break;
                            case ')':
                                xopen = xcurrent.Ancestors().FirstOrDefault(_ => _.Name.LocalName == nameof(StdAstNode.sub));
                                if (xopen is null)
                                {
                                    // Literal - add to character stream.
                                    sb.Append(c);
                                }
                                else
                                {
                                    xopen.SetAttributeValue(nameof(StdAstAttr.ismatched), bool.TrueString);
                                }
                                continue;   // <- Different!
#endif
                            default:
                                if (localAppendChar(c, cp))
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                        }
                        xprev.Add(xcurrent);

#if false
                        // Detect unary 'not'
                        if (xcurrent.Name.LocalName != nameof(StdAstNode.sub) &&
                            xprev.Name.LocalName == nameof(StdAstNode.not) &&
                            xprev.Parent != null)
                        {
                            xcurrent = xprev.Parent;
                        }
#endif
                    }
                }


                #endregion L o c a l F x
            }

            // Interpret QueryLikeTerms or FilterLikeTerms
            string localBuildExpression()
            {
                string childClauseE, childClauseN;
                var nArgs =
                    XAST
                    .DescendantsAndSelf()
                    .Count(n => n.Name.LocalName == "term") - 1;
                foreach (var node in
                    XAST
                    .DescendantsAndSelf()
                    .Reverse())
                {
                    // In the order added:
                    var builderE = node.Elements().Select(_ =>
                        _
                        .Attribute(nameof(StdAstAttr.clauseE))?.Value)
                        .OfType<string>()
                        .ToList();
                    childClauseE = string.Join(" ", builderE);

                    var builderN = node.Elements().Select(_ =>
                        _
                        .Attribute(nameof(StdAstAttr.clauseN))?.Value)
                        .OfType<string>()
                        .ToList();
                    childClauseN = string.Join(" ", builderN);

                    if (Enum.TryParse(node.Name.LocalName, out StdAstNode nodetype))
                    {
                        switch (nodetype)
                        {
                            case StdAstNode.expr:
                                break;
                            case StdAstNode.ast:
                                if (string.IsNullOrWhiteSpace(childClauseE))
                                {
                                    throw new InvalidOperationException("Expecting non-empty term.");
                                }
                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseE),
                                    childClauseE);
                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseN),
                                    childClauseN);
                                break;
                            case StdAstNode.term:
                                var likeTerms = proxyType
                                    .GetProperties()
                                    .Where(p =>
                                        (qfMode == QueryFilterMode.Query && p.GetCustomAttribute<QueryLikeTermAttribute>() != null) ||
                                        (qfMode == QueryFilterMode.Filter && p.GetCustomAttribute<FilterLikeTermAttribute>() != null))
                                    .ToArray();
                                var tagTerms = proxyType
                                    .GetProperties()
                                    .Where(p =>
                                        (p.GetCustomAttribute<TagMatchTermAttribute>() != null))
                                    .ToArray();
                                var value = node.Attribute(nameof(StdAstAttr.value))?.Value ?? string.Empty;
                                var substr = value.Substring(0, Math.Min(5, value.Length));
                                bool isTag = substr == "$FDFD" && value.Length == 10;
                                value = Rehydrate(value);
                                var argId = $"@param{nArgs--:X4}";
                                _args[argId] = value;
                                value = value.Replace("'", "''"); // It's time to escape the squotes for SQL.

                                if (likeTerms.Length == 0 && tagTerms.Length == 0)
                                    throw new InvalidOperationException($"Parser requires at least one term for {qfMode}.");
                                // Different - 
                                // Enumerate fields and memoize them.
                                List<string>
                                    exprsN = new List<string>(),
                                    exprsE = new List<string>();
                                var visited = new HashSet<PropertyInfo>();
                                foreach (var likeTerm in likeTerms)
                                {
                                    if (visited.Add(likeTerm))
                                    {
                                        exprsE.Add($"{likeTerm.Name} LIKE '%{value}%'");
                                        exprsN.Add($"{likeTerm.Name} LIKE {argId}");
                                    }
                                }
                                if (isTag)
                                {
                                    foreach (var tagTerm in tagTerms)
                                    {
                                        if (visited.Add(tagTerm))
                                        {
                                            exprsE.Add($"{tagTerm.Name} LIKE '%{value}%'");
                                            exprsN.Add($"{tagTerm.Name} LIKE {argId}");
                                        }
                                    }
                                }
                                else
                                {
                                    switch (_activeQFMode)
                                    {
                                        case QueryFilterMode.Query:
                                            break;
                                        case QueryFilterMode.Filter:
                                            var openBracketCount = 0;
                                            foreach (var c in value)
                                            {
                                                switch (c)
                                                {
                                                    case '[': openBracketCount++; break;
                                                    case ']': openBracketCount--; break;
                                                }
                                            }

                                            // Fuzzy tag filter.
                                            if (openBracketCount != 0) // Could be pos or neg
                                            {
                                                foreach (var tagTerm in tagTerms)
                                                {
                                                    if (visited.Add(tagTerm))
                                                    {
                                                        exprsE.Add($"{tagTerm.Name} LIKE '%{value}%'");
                                                        exprsN.Add($"{tagTerm.Name} LIKE {argId}");
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                                var unionsE = string.Join(" OR ", exprsE);
                                var unionsN = string.Join(" OR ", exprsN);

                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseE),
                                    string.IsNullOrWhiteSpace(childClauseE)
                                    ? $"({unionsE})"
                                    : $"({unionsE}) {childClauseE}");

                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseN),
                                    string.IsNullOrWhiteSpace(childClauseN)
                                    ? $"({unionsN})"
                                    : $"({unionsN}) {childClauseN}");

#if false
                                node.SetBoundAttributeValue(
                                    tag: tmpls,
                                    name: nameof(StdAstAttr.pos),
                                    text: $"[KVPs]");
#endif
                                break;
                            case StdAstNode.not:
                                if (string.IsNullOrWhiteSpace(childClauseE))
                                {
                                    this.ThrowHard<InvalidOperationException>("Expecting non-empty term.");
                                    break;
                                }
                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseE),
                                    $"(NOT {childClauseE})");
                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseN),
                                    $"(NOT {childClauseN})");
                                break;
                            case StdAstNode.and:
                                if (string.IsNullOrWhiteSpace(childClauseE))
                                {
                                    // KNOWN EDGE CASE
                                    // e.g. sho|bla) [not animal]
                                    var msg = "ADVISORY : KNOWN EDGE CASE - Expecting non-empty term.";
                                    Debug.WriteLine(msg);
                                    this.Advisory(msg);
                                }
                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseE),
                                    $"AND {childClauseE}");
                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseN),
                                    $"AND {childClauseN}");
                                break;
                            case StdAstNode.or:
                                if (string.IsNullOrWhiteSpace(childClauseE))
                                {
                                    this.ThrowHard<InvalidOperationException>("Expecting non-empty term.");
                                    break;
                                }
                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseE),
                                    $"OR {childClauseE}");
                                node.SetAttributeValue(
                                    nameof(StdAstAttr.clauseN),
                                    $"OR {childClauseN}");
                                break;
                            case StdAstNode.sub:
                                if (string.IsNullOrWhiteSpace(childClauseE))
                                {
                                    this.ThrowHard<InvalidOperationException>("Expecting non-empty term.");
                                    break;
                                }
                                if (node.Attribute(nameof(StdAstAttr.ismatched))?.Value is string ismatched)
                                {
                                    if (bool.Parse(ismatched))
                                    {
                                        node.SetAttributeValue(
                                           nameof(StdAstAttr.clauseE),
                                           $"({childClauseE})");
                                        node.SetAttributeValue(
                                           nameof(StdAstAttr.clauseN),
                                           $"({childClauseN})");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("ADVISORY: ToDo - render this as a literal.");
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException("Sub node is required to have an 'ismatched' attribute.");
                                }
                                break;
                            case StdAstNode.memo:
                            default:
                                throw new NotImplementedException($"Bad case: {node.Name.LocalName}");
                        }
                        switch (node.Name.LocalName)
                        {
                            default:
                                break;
                        }
                    }
                }
                WherePredicate = XAST.Attribute(nameof(StdAstAttr.clauseE))?.Value ?? $"1";

#if false && DEBUG && SAVE
                Debug.WriteLine(string.Empty);
                Debug.WriteLine("250702 - String-First");
                Debug.WriteLine(Query);
                Debug.WriteLine(string.Empty);
                Debug.WriteLine("250702 - Named");
                Debug.WriteLine(ToString());

                string
                    stringFirst = Query.Replace(Environment.NewLine, " ").Replace("  ", " "),
                    toString = ToString().Replace(Environment.NewLine, " ").Replace("  ", " ");
                Debug.Assert(stringFirst == toString);
#endif

                lock (_lock)
                {
                    this.OnAwaited(new AwaitedEventArgs(caller: nameof(localBuildExpression)));
                }
                return Query;
            }
            #endregion L o c a l F x   
        }

        /// <summary>
        /// The raw unprocessed input string from the user.
        /// </summary>
        public string Raw { get; private set; } = string.Empty;

        /// <summary>
        /// Caches values from [SelfIndexed] properties grouped by IndexingMode.
        /// Populated on ParseSqlMarkdown(...), used to generate QueryTerm, FilterTerm, and TagMatchTerm.
        /// </summary>
        private Dictionary<IndexingMode, HashSet<string>> _parsedIndexTerms
            = new Dictionary<IndexingMode, HashSet<string>>
            {
                [IndexingMode.QueryLikeTerm] = new HashSet<string>(),
                [IndexingMode.FilterLikeTerm] = new HashSet<string>(),
                [IndexingMode.TagMatchTerm] = new HashSet<string>()
            };

        /// <summary>
        /// A mutable working buffer that begins as <see cref="Raw"/> and evolves across parser passes.
        /// 
        /// This property captures the expression's transformation state at each stage of preprocessing,
        /// including quote escaping, tag substitution, lint normalization, and spacing cleanup.
        ///
        /// It directly drives downstream processes like AST construction and transliteration,
        /// and is observable during unit tests for step-by-step verification.
        /// </summary>
        public string Transform { get; internal set; } = string.Empty;

#if false
        /// <summary>
        /// The intermediate SQL clause template generated from <see cref="Transform"/>. 
        /// 
        /// This string encodes logical structure using placeholders like <c>$FEFF_property$</c> 
        /// to defer actual field targeting until the final hydration phase. It reflects the
        /// parsed intent of the expression, before being expanded across annotated model fields.
        ///
        /// Use this as the basis for per-field LIKE clause generation or substitution.
        /// </summary>
        public string TermLogic { get; internal set; } = string.Empty;
#endif

        /// <summary>
        /// The main AST node into which logical structures are injected.
        /// </summary>
        public XElement XAST { get; }

        /// <summary>
        /// A dictionary of atomically substituted tokens, used for safely round-tripping quotes and tags.
        /// </summary>
        public Dictionary<string, string> Atomics { get; } = new Dictionary<string, string>();

        public string Rehydrate(string expr)
        {
            restart:
            foreach (var kvp in Atomics)
            {
                var exprB4 = expr;
                expr = expr.Replace(kvp.Key, kvp.Value);
                if (expr != exprB4)
                {
                    goto restart;
                }
            }
            return expr;
        }

        /// <summary>
        /// The assembled SQL expression.
        /// </summary>
        public string Query
        {
            get
            {
                var builder = new List<string> { Preamble };
                if (!string.IsNullOrWhiteSpace(WherePredicate))
                {
                    builder.Add(WherePredicate);
                }
                if (CanExecutePrimaryKeyClause())
                {
                    builder.Add(PrimaryKeyClause());
                }
                if (Limit > 0)
                {
                    builder.Add($"LIMIT {Limit}");
                }
                var preview = string.Join(Environment.NewLine, builder);
                return preview;
            }
        }

        /// <summary>
        /// The SQL WHERE clause preamble, e.g. "SELECT * FROM tablename WHERE".
        /// </summary>
        public string Preamble { get; protected set; } = string.Empty;

        public string? WherePredicate { get; protected set; }
        public string? PrimaryKeyPredicate { get; protected set; } = string.Empty;

        public bool CanExecutePrimaryKeyClause()
        {
            int total = AllowedPrimaryKeys.Count + DisallowedPrimaryKeys.Count;

            return
                total != 0
                && total <= MaxPrimaryKeyTermCount
                && FilteringState != FilteringState.Ineligible
                && !string.IsNullOrWhiteSpace(PrimaryKeyColumnName);
        }

        /// <summary>
        /// Builds a primary key predicate using IN and/or NOT IN clauses
        /// based on AllowedPrimaryKeys and DisallowedPrimaryKeys.
        /// Does not include leading AND; caller is responsible for composition.
        /// Returns an empty string if no predicate can be constructed.
        /// </summary>
        public string PrimaryKeyClause()
        {
            if (!CanExecutePrimaryKeyClause())
                return string.Empty;

            var clauses = new List<string>();

            if (AllowedPrimaryKeys.Count > 0)
            {
                string allowedList = string.Join(
                    ", ",
                    AllowedPrimaryKeys.Select(pk => localFormatPrimaryKeyLiteral(pk)));

                clauses.Add($"{PrimaryKeyColumnName} IN ({allowedList})");
            }

            if (DisallowedPrimaryKeys.Count > 0)
            {
                string disallowedList = string.Join(
                    ", ",
                    DisallowedPrimaryKeys.Select(pk => localFormatPrimaryKeyLiteral(pk)));

                clauses.Add($"{PrimaryKeyColumnName} NOT IN ({disallowedList})");
            }

            if (clauses.Count == 0)
                return string.Empty;

            return string.Join(" AND ", clauses);

            static string localFormatPrimaryKeyLiteral(object pk)
            {
                return pk switch
                {
                    uint or int or long => pk.ToString()!,
                    string s => $"'{s.Replace("'", "''")}'",
                    _ => throw new InvalidOperationException("Unsupported primary key type.")
                };
            }
        }

        public string? PrimaryKeyColumnName { get; protected set; }


        #region L I M I T S 
        /// <summary>
        /// The maximum count of included and excluded predicates.
        /// </summary>
        public uint MaxPrimaryKeyTermCount { get; set; } = 1024;

        /// <summary>
        /// To exclude this term from query, set to 0.
        /// </summary>
        public uint DefaultLimit { get; set; } = uint.MinValue;

        public uint Limit =>
            _dhostLimit.Tokens.LastOrDefault()?.Sender is uint limit
            ? limit
            : DefaultLimit;

        private readonly DisposableHost _dhostLimit = new();

        public IDisposable BeginLimit(uint limit) => _dhostLimit.GetToken(sender: limit);
        private string LimitTerm => Limit == uint.MaxValue ? string.Empty : $"LIMIT {Limit}";
        #endregion L I M I T S

        #region N A M E D    S U P P O R T
        /// <summary>
        /// The query string using named parameters like '@param0001'.
        /// </summary>
        public string NamedQuery =>
            $"{Preamble} {XAST.Attribute(nameof(StdAstAttr.clauseN))?.Value ?? "1"}";

        /// <summary>
        /// Ordered named parameters mapped to their wrapped values (e.g., '%value%').
        /// Preserves insertion order for compatibility with cnx.CreateCommand(...).
        /// </summary>
        public Dictionary<string, object> NamedArgs =>
            _args
                .Select(_ => new KeyValuePair<string, object>(
                    _.Key,
                    $"%{_.Value}%"))
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(_ => _.Key, kvp => kvp.Value);

        #endregion N A M E D    S U P P O R T

        #region P O S I T I O N A L    S U P P O R T
        /// <summary>
        /// The query string with positional placeholders ('?') instead of named parameters.
        /// </summary>
        public string PositionalQuery =>
            Regex.Replace(NamedQuery, @"@param[0-9A-Fa-f]+", "?");

        /// <summary>
        /// Argument values in the exact order they appear in the positional query.
        /// Suitable for use with cnx.CreateCommand(...).
        /// </summary>
        public object[] PositionalArgs =>
            Regex
                .Matches(NamedQuery, @"@param[0-9A-Fa-f]+")
                .Cast<Match>()
                .Select(_ => $"%{_args[_.Value]}%")
                .ToArray();

        /// <summary>
        /// Gets or sets a pre-parse eligibility filter that determines whether
        /// an input expression should proceed through the parsing pipeline.
        /// </summary>
        /// <remarks>
        /// This predicate is evaluated at the start of <c>ParseSqlMarkdown</c>.
        /// If it returns <c>false</c>, parsing is aborted and an empty result is returned.
        ///
        /// This is not a syntactic validator. Structural correctness is handled
        /// later by the linter and AST builder. Instead, this property acts as a
        /// lightweight gate to prevent unnecessary parsing and SQL generation.
        ///
        /// By default:
        /// - In <see cref="QueryFilterMode.Query"/> mode, the expression must contain
        ///   at least three contiguous alphanumeric characters.
        /// - In <see cref="QueryFilterMode.Filter"/> mode, all input is considered eligible.
        ///
        /// Consumers may override this predicate to enforce domain-specific
        /// eligibility rules such as minimum length, throttling heuristics,
        /// or restricted character policies.
        /// </remarks>
        public Predicate<string> ValidationPredicate
        {
            get
            {
                return _validationPredicate ?? (expr =>
                {
                    // Default true, or (in Query mode) 3 minimum contiguous chars.
                    switch (_activeQFMode)
                    {
                        case QueryFilterMode.Query:
                            return Regex.IsMatch(expr, @"[a-zA-Z0-9]{3}");
                        case QueryFilterMode.Filter:
                        default:
                            return true;
                    }
                });
            }
            set => _validationPredicate = value;
        }

        Predicate<string>? _validationPredicate = null;

        private QueryFilterMode _activeQFMode = QueryFilterMode.Query;

        #endregion P O S I T I O N A L    S U P P O R T

        public object _lock = new object();
        public override string ToString()
        {
            string formatted = NamedQuery;

            foreach (var param in _args)
            {
                string value;
                if (param.Value is string s)
                {
                    value = $"'%{s.Replace("'", "''")}%'";
                }
                else if (param.Value == null)
                {
                    value = "NULL";
                }
                else
                {
                    value = param.Value.ToString();
                }
                formatted = formatted.Replace(param.Key, value);
            }
            return formatted;
        }

        #region C O N F I G
        public QueryFilterConfig QueryFilterConfig
        {
            get => _queryFilterConfig;
            set
            {
                if (!Equals(_queryFilterConfig, value))
                {
                    _queryFilterConfig = value;
                    // Seems odd, right? But this actually does something.
                    // It forces a review of of its current state to make
                    // sure that it's still legal with the new QFC.
                    FilteringState = FilteringState;
                    OnPropertyChanged();
                }
            }
        }
        QueryFilterConfig _queryFilterConfig = QueryFilterConfig.QueryAndFilter;

        public DisposableHost DHostSelfIndexing { get; } = new();

        /// <summary>
        /// Support for inheritance model for Collection.
        /// </summary>
        /// <remarks>
        /// - "The ephemeral backing store for this collection’s contract."
        /// - Like any other SQLite database this can be configured with N tables.
        ///   However, the semantic constraints on contract parsing (where ContractType 
        ///   is assumed to be the item type of the collection that subclasses it) will
        ///   provide an advisory stream should this be called upon to service more
        ///   that the implicit single table for the collection.
        /// </remarks>
        protected SQLiteConnection FilterQueryDatabase
        {
            get
            {
                if (_filterQueryDatabase is null)
                {
                    _filterQueryDatabase = new SQLiteConnection(":memory:");
                    _filterQueryDatabase.CreateTable(ContractType);
                }
                return _filterQueryDatabase;
            }
            set
            {
                if (!Equals(_filterQueryDatabase, value))
                {
                    _filterQueryDatabase = value;
                    OnPropertyChanged();
                    this.OnAwaited();
                }
            }
        }
        SQLiteConnection? _filterQueryDatabase = default;

        /// <summary>
        /// Support for inheritance model for Collection.
        /// </summary>
        public SQLiteConnection? MemoryDatabase
        {
            get => _memoryDatabase;
            set
            {
                if (!Equals(_memoryDatabase, value))
                {
                    if (_memoryDatabase != null)
                    {
                        _memoryDatabase.Dispose();
                    }
                    _memoryDatabase = value;
                    OnPropertyChanged();
                    this.OnAwaited();
                }
            }
        }
        SQLiteConnection? _memoryDatabase = default;

        #endregion C O N F I G

        #region N A V    S E A R C H    S T A T E    M A C H I N E

        protected FilteringState FilteringStatePrev { get; set; }
        public FilteringState FilteringState
        {
            get => _filteringState;
            // {461B2298-3E0A-49F8-AE52-EB43F70699AD}
            // CONTROLLED ACCESS
            // To obtain set access to this property, make a subclass.
            // It is not designed to be modified directly, e.g. by the consumer
            // of a client collection that exposes MarkdownContext as a property.
            // EXAMPLE - promote to internal
            // public class InternalMarkdownContext<T> : MarkdownContext<T>
            // {
            //    public new FilteringState FilteringState
            //    {
            //        get => base.FilteringState;
            //        internal set => base.FilteringState = value;
            //    }
            // }
            // RATIONALE
            // Promoting to internal is something that can be safely done
            // in the assembly that holds the client collection, but would
            // not work if done in the library itself.
            protected set
            {
                switch (QueryFilterConfig)
                {
                    case QueryFilterConfig.Query:
                        // Query-only is always ineligible for filtering.
                        value = FilteringState.Ineligible;
                        break;
                    case QueryFilterConfig.Filter:
                        // Filter-only is always either armed or active.
                        if (value == FilteringState.Ineligible)
                        {
                            value = FilteringState.Armed;
                        }
                        break;
                    case QueryFilterConfig.QueryAndFilter:
                        break;
                    default:
                        break;
                }
                if (!Equals(_filteringState, value))
                {
                    FilteringStatePrev = _filteringState;
                    _filteringState = value;
                    OnFilteringStateChanged();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RouteToFullRecordset));
                    OnPropertyChanged(nameof(IsFiltering));
                }
            }
        }
        FilteringState _filteringState = FilteringState.Ineligible;

        /// <summary>
        /// Exposes <see cref="FilteringState"/> for test scenarios while preventing direct production mutation.
        /// </summary>
        /// <remarks>
        /// This property exists solely to support legacy and test workflows that require
        /// setting <see cref="FilteringState"/> externally. The setter intentionally invokes
        /// <c>ThrowHard&lt;NotSupportedException&gt;</c> to discourage direct use.
        /// 
        /// Consumers who require write access should derive from the declaring type and
        /// use the protected setter instead. Alternatively, the throw may be handled to 
        /// enable controlled legacy behavior, but such usage should be limited to tests.
        /// </remarks>
        [Obsolete("Use a subclass to gain access to the protected setter for FilteringState.")]
        public FilteringState FilteringStateForTest
        {
            get => FilteringState;
            set
            {
                // To enable legacy behavior, handle this Throw.
                if (this.ThrowHard<NotSupportedException>(
                   "Use a subclass to gain access to the protected setter for FilteringState.").Handled)
                {
                    FilteringState = value;
                }
            }
        }

        /// <summary>
        /// Indicates whether the collection is operating in latched filtering mode.
        /// </summary>
        /// <remarks>
        /// Filtering mode exhibits hysteresis behavior to preserve user intent.
        /// 
        /// After a successful query yielding sufficient results, filtering becomes
        /// eligible and transitions to Active upon the next IME input.
        /// 
        /// Clearing the IME does not immediately revert to query mode. If filtering
        /// was previously Active, the state remains latched until an explicit second
        /// clear action signals intent to exit filtering.
        /// 
        /// This prevents oscillation between Query and Filter modes caused by
        /// transient empty input and preserves refinement context.
        /// </remarks>
        public bool IsFiltering
            => FilteringState == FilteringState.Ineligible
                ? false
                : FilteringState == FilteringState.Active
                    ? true
                    : FilteringStatePrev == FilteringState.Active;

        // Canonical {5932CB31-B914-4DE8-9457-7A668CDB7D08}
        public FilteringState Clear(bool all = false)
        {
            if (all)
            {
                InputText = string.Empty;
                FilteringState = FilteringState.Ineligible;
                SearchEntryState = SearchEntryState.Cleared;
            }
            else
            {
                // UpgradeToFilter : FilteringState == FilteringState.Ineligible;
                // DowngradeToQuery: FilteringState != FilteringState.Ineligible;
                // DowngradeToClear: DowngradeToQuery + SearchEntryState is not showing a query result.
                if (InputText.Length > 0)
                {
                    // Input text is non-empty.
                    InputText = string.Empty;
                    switch (FilteringState)
                    {
                        case FilteringState.Ineligible:
                            break;
                        case FilteringState.Armed:
                            // Basically, if there is entry text but the filtering
                            // is still only armed not active, that indicates that
                            // what we're seeing in the list is the result of a full
                            // db query that just occurred. So now, when we CLEAR that
                            // text, it's assumed to be in the interest of filtering
                            // that query result, so filtering stays Armed in this case.
                            break;
                        case FilteringState.Active:
                            break;
                        default:
                            throw new NotImplementedException($"Bad case: {FilteringState}");
                    }
                }
                else
                {
                    // Input text is empty.
                    switch (FilteringState)
                    {
                        case FilteringState.Ineligible:
                            // RELEASE 1.0.2 bug fixed by adding this clause.
                            switch (SearchEntryState)
                            {
                                case SearchEntryState.QueryCompleteNoResults:
                                case SearchEntryState.QueryCompleteWithResults:
                                    SearchEntryState = SearchEntryState.Cleared;
                                    break;
                            }
                            break;
                        case FilteringState.Armed:
                        case FilteringState.Active:
                            // Text is already empty and clear is invoked (clicked) again - this is a hard reset.
                            FilteringState = FilteringState.Ineligible;
                            break;
                        default:
                            throw new NotImplementedException($"Bad case: {FilteringState}");
                    }
                }
            }
            // Fluent return;
            return FilteringState;
        }

        protected virtual void OnFilteringStateChanged() { }

        public string InputText
        {
            get => _inputText;
            set
            {
                value ??= string.Empty;
                if (!Equals(_inputText, value))
                {
                    _inputText = value;
                    OnInputTextChanged();
                    OnPropertyChanged();
                    RestartIfSemanticInputChanged();
                }
            }
        }
        string _inputText = string.Empty;

        /// <summary>
        /// Normalizes trailing transient operators and restarts the debounce cycle
        /// only if the resulting semantic input differs from the previous value.
        /// </summary>
        protected virtual void RestartIfSemanticInputChanged()
        {
            var nonTransientInputText = InputText.TrimEnd();
            bool hasTrailingEscapedOperator = false;
            if (nonTransientInputText.Length >= 2)
            {
                int len = nonTransientInputText.Length;
                char c1 = nonTransientInputText[len - 2];
                char c2 = nonTransientInputText[len - 1];

                if (c1 == '\\' && (c2 == '\\' || c2 == '!' || c2 == '|' || c2 == '&'))
                {
                    hasTrailingEscapedOperator = true;
                }
            }
            if (!hasTrailingEscapedOperator)
            {
                nonTransientInputText = nonTransientInputText.TrimEnd(@"!|&".ToCharArray());
            }
            if (_nonTransientInputText != nonTransientInputText)
            {
                _nonTransientInputText = nonTransientInputText;
                StartOrRestart();
            }
        }
        string _nonTransientInputText = string.Empty;


        [Careful("Trimming or modifying the raw InputText is not allowed.")]
        protected virtual void OnInputTextChanged()
        {
            var filteringStateB4 = FilteringState;
            switch (FilteringState)
            {
                case FilteringState.Ineligible:
                    if (InputText.Length == 0)
                    {
                        SearchEntryState = SearchEntryState.QueryEmpty;
                    }
                    else if (InputText.Length < 3)
                    {
                        SearchEntryState = SearchEntryState.QueryENB;
                    }
                    else
                    {
                        SearchEntryState = SearchEntryState.QueryEN;
                    }
                    break;
                case FilteringState.Armed:
                    if (InputText.Length != 0)
                    {
                        FilteringState = FilteringState.Active;
                    }
                    break;
                case FilteringState.Active:
                    if (InputText.Length == 0)
                    {
                        // Downgrade but stay armed.
                        FilteringState = FilteringState.Armed;
                    }
                    break;
                default:
                    throw new NotImplementedException($"Bad case: {FilteringState}");
            }
        }

        /// <summary>
        /// True when InputText is empty regardless of IsFiltering.
        /// </summary>
        /// <remarks>
        /// - Distinct from IsFiltering which captures the FilterQueryDatabase on
        ///   its positive edge, this is a lightweight signal to the canonical items.
        /// - This is distinct from the SyncAuthority which governs DDX direction.
        /// </remarks>
        public virtual bool RouteToFullRecordset => true;


        public SearchEntryState SearchEntryState
        {
            get => _searchEntryState;
            // See also: 
            // {461B2298-3E0A-49F8-AE52-EB43F70699AD}
            protected set
            {
                if (!Equals(_searchEntryState, value))
                {
                    _searchEntryState = value;
                    OnSearchEntryStateChanged();
                    OnPropertyChanged();
                }
            }
        }
        SearchEntryState _searchEntryState = default;

        protected virtual void OnSearchEntryStateChanged() { }

        protected override async Task OnEpochFinalizingAsync(EpochFinalizingAsyncEventArgs e)
        {
            await base.OnEpochFinalizingAsync(e);
            if (!e.Cancel)
            {
                OnInputTextSettled(new CancelEventArgs());
            }
        }
#if false
        protected WatchdogTimer WDTInputTextSettled
        {
            get
            {
                if (_wdtInputTextSettled is null)
                {
                    _wdtInputTextSettled =
                        new WatchdogTimer(
                            defaultInitialAction: () =>
                            {
                                // Do this.
                                // Otherwise fail Test_TrackProgressiveInputState() {...}
                                // See also: {24048258-8BE4-40C4-BF85-8863E98BED51}
                                Debug.WriteLine($"210206.Z defaultInitialAction");
                                _ready.Wait(0);
                                _wdtEpochToken ??= DHostEpochReferenceCount.GetToken();
                            },
                            defaultCompleteAction: () =>
                            {
                                OnInputTextSettled(new CancelEventArgs());
                                localSafeRelease();
                            })
                        {
                            Interval = InputTextSettleInterval
                        };
                    _wdtInputTextSettled.Cancelled += (sender, e) =>
                    {
                        localSafeRelease();
                    };
                    void localSafeRelease()
                    {
                        _ready.Wait(0);
                        _ready.Release();
                        _wdtEpochToken?.Dispose();
                        _wdtEpochToken = null;
                    }
                }
                return _wdtInputTextSettled;
            }
        }

        WatchdogTimer? _wdtInputTextSettled = null;
        IDisposable? _wdtEpochToken = null;
#endif

        /// <summary>
        /// Reference counter for Busy property.
        /// </summary>
        /// <remarks>
        /// This does not hold the awaiter and should be used
        /// for visual activity indicator only.
        /// </remarks>
        public DisposableHost DHostBusy
        {
            get
            {
                if (_dhostBusy is null)
                {
                    _dhostBusy = new DisposableHost();
                    _dhostBusy.BeginUsing += (sender, e) =>
                    {
                        Busy = true;
                        OnPropertyChanged(nameof(Busy));
                    };
                    _dhostBusy.FinalDispose += (sender, e) =>
                    {
                        Busy = false;
                        OnPropertyChanged(nameof(Busy));
                    };
                }
                return _dhostBusy;
            }
        }
        DisposableHost? _dhostBusy = null;

        public DisposableHost DHostEpochReferenceCount
        {
            get
            {
                if (_dhostEpochReferenceCount is null)
                {
                    _dhostEpochReferenceCount = new DisposableHost();
                    _dhostEpochReferenceCount.BeginUsing += (sender, e) =>
                    {
                        Debug.Assert(DateTime.Now.Date == new DateTime(2026, 2, 7).Date, "Don't forget disabled");
                        _ready.Wait(0);
                    };
                    _dhostEpochReferenceCount.FinalDispose += (sender, e) =>
                    {
                        _ready.Wait(0);
                        _ready.Release();
                    };
                }
                return _dhostEpochReferenceCount;
            }
        }
        DisposableHost? _dhostEpochReferenceCount = null;

#if DEBUG
        protected SemaphoreSlimWithTrace _ready { get; } = new SemaphoreSlimWithTrace(1, 1);
#else
        protected SemaphoreSlim _ready { get; } = new SemaphoreSlim(1, 1);
#endif
        public bool Busy { get; private set; }

        protected virtual void OnInputTextSettled(CancelEventArgs e)
        {
            InputTextSettled?.Invoke(this, e);
        }

        public event EventHandler? InputTextSettled;

        #endregion N A V    S E A R C H    S T A T E    M A C H I N E

        #region S E L F    I N D E X E D
        public string QueryTerm => string.Join("~", _parsedIndexTerms[IndexingMode.QueryLikeTerm]);
        public string FilterTerm => string.Join("~", _parsedIndexTerms[IndexingMode.FilterLikeTerm]);
        public string TagMatchTerm => string.Join(" ", _parsedIndexTerms[IndexingMode.TagMatchTerm].Select(_ => $"[{_}]"));

        #endregion S E L F    I N D E X E D

        /// <summary>
        /// Produce a IN clause with positive polarity that applies the specified PKs.
        /// </summary>
        public HashSet<object> AllowedPrimaryKeys { get; } = new();

        /// <summary>
        /// Produce a IN clause with negative polarity that applies the specified PKs.
        /// </summary>
        public HashSet<object> DisallowedPrimaryKeys { get; } = new();
    }
    public class MarkdownContext<T> : MarkdownContext
    {
        public MarkdownContext() : base(typeof(T)) { }
    }
}
