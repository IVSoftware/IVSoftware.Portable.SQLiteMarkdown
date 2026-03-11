using IVSoftware.Portable.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    class SelfRemovingXBoundAttribute : XBoundAttribute, IDisposable
    {
        public SelfRemovingXBoundAttribute(Enum stdEnum, Enum tag)
            : base(
                  name: stdEnum.ToString(),
                  tag: tag,
                  text: $"[{tag}]")
        { }
        public SelfRemovingXBoundAttribute(Enum stdEnum, object tag, string? text = null)
            : base(
                  name: stdEnum.ToString(),
                  tag: tag,
                  text: text)
        { }

        public void Dispose()
        {
            if(Parent is not null)
            {
                Remove();
            }
            else
            {
                Debug.Fail($@"ADVISORY - Expecting parent.");
            }
        }
    }

    internal static partial class Extensions
    {
        public static SelfRemovingXBoundAttribute SetSelfRemovingXBoundAttribute(this XElement @this, Enum stdEnum, Enum tag)
        {
            var xba = new SelfRemovingXBoundAttribute(stdEnum, tag);
            @this.Add(xba);
            return xba;
        }
        public static SelfRemovingXBoundAttribute SetSelfRemovingXBoundAttribute(this XElement @this, Enum stdEnum, object tag)
        {
            var xba = new SelfRemovingXBoundAttribute(stdEnum, tag);
            @this.Add(xba);
            return xba;
        }
    }
}
