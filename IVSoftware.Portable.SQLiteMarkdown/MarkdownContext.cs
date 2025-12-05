
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Represents the full parsing and transformation context for converting a user
    /// expression  (in lightweight markdown syntax) into an executable SQL WHERE
    /// clause by accumulating  parsing state, managing AST nodes, tracking substitutions,
    /// and guiding final SQL generation via attribute-aware hydration.
    /// </summary>
    [DebuggerDisplay("ContractType={ContractType}")]
    public class MarkdownContext 
        : INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a self-contained expression parsing environment, binding it
        /// to an XML-based AST using the IVSoftware.Portable.XBoundObject NuGet.
        /// </summary>
        /// <remarks>
        /// [Careful] 
        /// The initial type must be known and captured regardless of GF mode.
        /// A paremeterless CTOR would not be appropriate so avaid that.
        /// </remarks>
        public MarkdownContext(Type type) 
        {
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
        /// Run from internal values using ContractType
        /// </summary>
        public string ParseSqlMarkdown()
            => ParseSqlMarkdown(
                InputText,
                ContractType,
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

        public string ParseSqlMarkdown(string expr, Type proxyType, QueryFilterMode qfMode, out XElement xast)
        {
            if (proxyType is null) throw new InvalidOperationException("Proxy type must be specified");
            ProxyType = proxyType;

            Raw = expr;
            Transform = Raw;

            Preamble = $"SELECT * FROM {localGetTableName()} WHERE";

            string localGetTableName()
            {
                var tableAttr = ContractType
                    .GetCustomAttributes(inherit: true)
                    .FirstOrDefault(attr => attr.GetType().Name == "TableAttribute");

                if (tableAttr != null)
                {
                    var nameProp = tableAttr.GetType().GetProperty("Name");
                    if (nameProp != null && nameProp.GetValue(tableAttr) is string name)
                        return name;
                }
                return proxyType.Name;
            }
#if DEBUG
            if (expr == "animal b")
            { }
#endif
            // Guard reentrancy by making sure to clear previous passes.
            XAST.RemoveNodes();
            _activeQFMode = qfMode;
            if(ValidationPredicate?.Invoke(expr) == false)
            {
                xast = XAST;
                return string.Empty;
            }
            xast = XAST;
            uint quoteId = 0xFEFE0000;
            uint tagId = 0xFDFD0000;
            _args.Clear();
            localEscape();
            localRunTagLinter();
            localRunQuoteLinter('"');
            localRunQuoteLinter('\'');
            localLint();
            localBuildAST();
            return localBuildExpression();

            #region L o c a l F x
            // Preemptive explicit escapes.
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

                lock (_lock)
                {
                    this.OnAwaited(new AwaitedEventArgs(caller: nameof(localBuildAST)));
                }

                #region L o c a l F x

                // Return true if loop can continue (i.e. end not detected).
                bool localAppendChar(char newChar, char previewChar)
                {
                    bool isLastChar =
                        previewChar == '\0'
                        ? true
                        : @"&|!()[]\".Contains(previewChar); // The '#' makes null not found.
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
                                    switch(selfIndexingMode)
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
                #endregion L o c a l F x
            }

            // Interpret QueryLikeTerms or FilterLikeTerms
            string localBuildExpression()
            {
                string childClauseE, childClauseN;
                string
                    table = ProxyType.GetCustomAttribute<TableAttribute>()?.Name ?? ProxyType.Name;
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
                                var likeTerms = ProxyType
                                    .GetProperties()
                                    .Where(p =>
                                        (qfMode == QueryFilterMode.Query && p.GetCustomAttribute<QueryLikeTermAttribute>() != null) ||
                                        (qfMode == QueryFilterMode.Filter && p.GetCustomAttribute<FilterLikeTermAttribute>() != null))
                                    .ToArray();
                                var tagTerms = ProxyType
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
                                    throw new InvalidOperationException("Expecting non-empty term.");
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
                                    Debug.WriteLine("ADVISORY : KNOWN EDGE CASE - Expecting non-empty term.");
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
                                    throw new InvalidOperationException("Expecting non-empty term.");
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
                                    throw new InvalidOperationException("Expecting non-empty term.");
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
                Query = $@"
{Preamble} 
{XAST.Attribute(nameof(StdAstAttr.clauseE))?.Value ?? $"1"}"
.Trim();

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
                return
                    Query;
            }
            #endregion L o c a l F x   
        }


        /// <summary>
        /// The raw unprocessed input string from the user.
        /// </summary>
        public string Raw { get; private set; }

        /// <summary>
        /// The attributed CLR type used to resolve which properties participate in term matching.
        /// </summary>
        public Type ContractType
        {
            get => _contractType;
            set
            {
                if (value is null)
                {
                    Debug.Assert(DateTime.Now.Date == new DateTime(2025, 7, 4).Date, "Don't forget disabled");
                    throw new InvalidOperationException("Need that type...!");
                }
                else
                {
                    switch (_activeQFMode)
                    {
                        case QueryFilterMode.Query:
                            // Allow unconditional
                            if (!Equals(_contractType, value))
                            {
                                _contractType = value;
                                OnContractTypeChanged();
                                OnPropertyChanged();
                            }
                            break;
                        case QueryFilterMode.Filter:

                            // Allow only if Type not set by previous query.
                            Debug.Assert(
                                QueryFilterConfig == QueryFilterConfig.Filter,
                                "Expecting Query before Filter in any other mode!"
                            );

                            if (_contractType is null)
                            {
                                _contractType = value;
                                OnContractTypeChanged();
                                OnPropertyChanged();
                            }
                            break;
                    }
                }
            }
        }

        Type _contractType = default;

        protected virtual void OnContractTypeChanged()
        {
            if (ContractType is null)
            {
                throw new InvalidOperationException("Contract type cannot be null.");
            }
            else
            {
                if (FilterQueryDatabase != null)
                {
                    FilterQueryDatabase.Dispose();
                }
                FilterQueryDatabase  = new SQLiteConnection(":memory:");
                FilterQueryDatabase.CreateTable(ContractType);
            }
        }

        public Type ProxyType { get; private set; }

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
        /// The SQL WHERE clause preamble, e.g. "SELECT * FROM tablename WHERE".
        /// </summary>
        public string Preamble { get; internal set; } = string.Empty;

        /// <summary>
        /// A mutable working buffer that begins as <see cref="Raw"/> and evolves across parser passes.
        /// 
        /// This property captures the expression's transformation state at each stage of preprocessing,
        /// including quote escaping, tag substitution, lint normalization, and spacing cleanup.
        ///
        /// It directly drives downstream processes like AST construction and transliteration,
        /// and is observable during unit tests for step-by-step verification.
        /// </summary>
        public string Transform 
        { 
            get;
            internal set;
        }

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
        public string Query { get; private set; }

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

        Predicate<string> _validationPredicate = null;
        
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
        public event PropertyChangedEventHandler PropertyChanged;

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

        public DisposableHost DHostSelfIndexing
        {
            get
            {
                if (_dhostSelfIndexing is null)
                {
                    _dhostSelfIndexing = new DisposableHost();
                }
                return _dhostSelfIndexing;
            }
        }
        DisposableHost _dhostSelfIndexing = null;

        protected virtual SQLiteConnection FilterQueryDatabase { get; set; }

        
        public SQLiteConnection MemoryDatabase
        {
            get => _memoryDatabase;
            set
            {
                if (!Equals(_memoryDatabase, value))
                {
                    if(_memoryDatabase != null)
                    {
                        _memoryDatabase.Dispose();
                    }
                    _memoryDatabase = value;
                    OnPropertyChanged();
                }
            }
        }
        // Nullable property, but we're not in
        // a target framework that supports it.
        SQLiteConnection _memoryDatabase = default;

        #endregion C O N F I G

        #region N A V    S E A R C H    S T A T E    M A C H I N E
        public virtual bool RouteToFullRecordset => true;
        protected FilteringState FilteringStatePrev { get; set;  }
        public FilteringState FilteringState
        {
            get =>_filteringState;
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
                        if(value == FilteringState.Ineligible)
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

        public FilteringState FilteringStateForTest
        {
            get => FilteringState;
            set => FilteringState = value;
        }

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
                bool isDowngradeToQuery = FilteringState != FilteringState.Ineligible;
                bool isUpgradeToFilter = FilteringState == FilteringState.Ineligible;
                if (InputText.Length > 0)
                {
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
                    switch (FilteringState)
                    {
                        case FilteringState.Ineligible:
                            break;
                        case FilteringState.Armed:
                        case FilteringState.Active:
                            // If the text is already empty and
                            // you click again, it's a hard reset!
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
                if (!Equals(_inputText, value))
                {
                    _inputText = value;
                    OnPropertyChanged();
                    OnInputTextChanged();
                }
            }
        }
        string _inputText = string.Empty;
        protected virtual void OnInputTextChanged()
        {
            // Trim without eventing.
            _inputText = _inputText.Trim();

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
                    OnInputTextSettled(new CancelEventArgs(cancel: true));
                    return;
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
            WDTInputTextSettled.StartOrRestart();
        }

        public SearchEntryState SearchEntryState
        {
            get => _searchEntryState;
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
                                // Do this or fail here:
                                // - Test_TrackProgressiveInputState() 
                                // See also: {24048258-8BE4-40C4-BF85-8863E98BED51}
                                _ready.Wait(0);
                            },
                            defaultCompleteAction: () =>
                            {
                                OnInputTextSettled(new CancelEventArgs());
                            })
                        {
                            Interval = InputTextSettleInterval
                        };
                }
                return _wdtInputTextSettled;
            }
        }
        IDisposable _busyToken = null;

        public DisposableHost DHostBusy
        {
            get
            {
                if (_dhostBusy is null)
                {
                    _dhostBusy = new DisposableHost();
                    _dhostBusy.BeginUsing += (sender, e) =>
                    {
                        _ready.Wait(0);
                        Busy = true;
                        OnPropertyChanged(nameof(Busy));
                    };
                    _dhostBusy.FinalDispose += (sender, e) =>
                    {
                        Busy = false;
                        OnPropertyChanged(nameof(Busy));
                        _ready.Wait(0);
                        _ready.Release();
                    };
                }
                return _dhostBusy;
            }
        }
        DisposableHost _dhostBusy = null;
        protected SemaphoreSlim _ready { get; } = new SemaphoreSlim(1, 1);
        public TaskAwaiter GetAwaiter() =>
            _ready
            .WaitAsync(new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token)
            .GetAwaiter();
        public bool Busy { get; private set; }

        protected virtual void OnInputTextSettled(CancelEventArgs e)
        {
            InputTextSettled?.Invoke(this, e);
        }

        WatchdogTimer _wdtInputTextSettled = null;


        public event EventHandler InputTextSettled;

        public TimeSpan InputTextSettleInterval
        {
            get => _inputTextSettleInterval;
            set
            {
                if (!Equals(_inputTextSettleInterval, value))
                {
                    if (_wdtInputTextSettled is WatchdogTimer wdt)
                    {
                        wdt.Interval = value;
                    }
                    _inputTextSettleInterval = value;
                    OnPropertyChanged();
                }
            }
        }


        TimeSpan _inputTextSettleInterval = TimeSpan.FromSeconds(0.25);
        #endregion N A V    S E A R C H    S T A T E    M A C H I N E

        #region S E L F    I N D E X E D
        public string QueryTerm => string.Join("~", _parsedIndexTerms[IndexingMode.QueryLikeTerm]);
        public string FilterTerm => string.Join("~", _parsedIndexTerms[IndexingMode.FilterLikeTerm]);
        public string TagMatchTerm => string.Join(" ", _parsedIndexTerms[IndexingMode.TagMatchTerm].Select(_ => $"[{_}]"));

        #endregion S E L F    I N D E X E D
    }
    public class MarkdownContext<T> : MarkdownContext
    {
        public MarkdownContext() : base(typeof(T)) { }
    }
}
