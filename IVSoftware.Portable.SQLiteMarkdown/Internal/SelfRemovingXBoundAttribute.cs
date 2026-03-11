using IVSoftware.Portable.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    class SelfRemovingXBoundAttribute : XBoundAttribute
    {
        public SelfRemovingXBoundAttribute(Enum stdEnum, Enum tag)
            : base(
                  name: stdEnum.ToString(),
                  tag: tag,
                  text: $"[{tag}]")
        {
        }
    }

    internal static partial class Extensions
    {
        public static void SetSelfRemovingXBoundAttribute(this XElement @this, Enum stdEnum, Enum tag)
            => @this.Add(new SelfRemovingXBoundAttribute(stdEnum, tag));
    }
}
