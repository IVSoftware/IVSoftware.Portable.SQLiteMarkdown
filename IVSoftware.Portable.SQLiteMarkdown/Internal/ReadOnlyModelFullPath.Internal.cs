using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.Collections;
using IVSoftware.Portable.Xml.Linq.Collections.Internal;
using System;
using System.Linq;

namespace IVSoftware.Portable.Common.Collections
{
    sealed class ReadOnlyModelFullPath : IModelFullPath
    {
        readonly object _unk;

        public static ReadOnlyModelFullPath Create(object? unk)
        {
            if (unk is null)
            {
                nameof(ReadOnlyModelFullPath).ThrowHard<ArgumentNullException>($"{nameof(unk)} cannot be null.");
                return null!; // We warned you.
            }
            else
            {
                return new ReadOnlyModelFullPath(unk);
            }
        }
        private ReadOnlyModelFullPath(object unk) => _unk = unk;

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
                    if (_unk is IModelFullPath known)
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
                    if (_unk is IModelFullPath known)
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
                    if (_unk is IModelFullPath known)
                    {
                        _parentPath = known.ParentPath ?? string.Empty;
                    }
                    else
                    {
                        if (_unk.GetType().GetProperty(nameof(IModelFullPath.FullPath))?.GetValue(_unk) is string full)
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
