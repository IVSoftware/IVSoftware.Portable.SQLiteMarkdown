using IVSoftware.Portable.SQLiteMarkdown.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{

    internal delegate string GetFullPathDlgt(object o);
    internal class ModelingCapabilityInfo
    {
        public ModelingCapability ModelingCapability { get; set; }
        public GetFullPathDlgt? GetFullPath { get; set; }
    }
}
