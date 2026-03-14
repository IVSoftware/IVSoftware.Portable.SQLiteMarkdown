using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    sealed class ReadOnlyFullPathAffinity : IFullPathAffinity
    {
        readonly object _unk;

        public static ReadOnlyFullPathAffinity Create(object? unk)
        {
            if (unk is null)
            {
                nameof(ReadOnlyFullPathAffinity).ThrowHard<ArgumentNullException>($"{nameof(unk)} cannot be null.");
                return null!; // We warned you.
            }
            else
            {
                return new ReadOnlyFullPathAffinity(unk);
            }
        }
        private ReadOnlyFullPathAffinity(object unk) => _unk = unk;

        /// <summary>
        /// This class states policy that Id <==> PrimaryKey, and attempts to reflect it.
        /// </summary>
        /// <remarks>
        /// - If not found, returns empty string. 
        /// - This is considered a benign condition that the client must check.
        /// </remarks>
        public string Id
        {
            get
            {
                if (_id is null)
                {
                    if (_unk is IFullPathAffinity known)
                    {
                        _id = known.Id ?? string.Empty;
                    }
                    else
                    {
                        _id = _unk.GetType().GetPK()?.GetValue(_unk) as string ?? string.Empty;
                    }
                }
                return _id;
            }
        }
        string _id = null!;

        public string ParentId
        {
            get
            {
                if (_parentId is null)
                {
                    if (_unk is IFullPathAffinity known)
                    {
                        _parentId = known.ParentId ?? string.Empty;
                    }
                    else
                    {
                        var prop = _unk.GetType().GetProperty(nameof(ParentId));
                        _parentId = prop?.GetValue(_unk)?.ToString() ?? string.Empty;
                    }
                }
                return _parentId;
            }
        }
        string? _parentId = null;

        public string ParentPath
        {
            get
            {
                if (_parentPath is null)
                {
                    if (_unk is IFullPathAffinity known)
                    {
                        _parentPath = known.ParentPath ?? string.Empty;
                    }
                    else
                    {
                        if (_unk.GetType().GetProperty(nameof(IFullPathAffinity.FullPath))?.GetValue(_unk) is string full)
                        {
                            var parts = full.Split('\\');
                            _parentPath = parts.Length > 1
                                ? string.Join("\\", parts.Take(parts.Length - 1))
                                : string.Empty;
                        }
                        else
                        {
                            _parentPath = string.Empty;
                        }
                    }
                }
                return _parentPath;
            }
        }
        string? _parentPath = null;

        public string FullPath => ParentPath.LintCombinedSegments(Id);
    }
}
