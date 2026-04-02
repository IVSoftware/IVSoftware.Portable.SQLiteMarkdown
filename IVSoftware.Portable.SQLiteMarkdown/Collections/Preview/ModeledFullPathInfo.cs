using IVSoftware.Portable.SQLiteMarkdown.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{

    internal delegate string GetFullPathDlgt(object o);
    internal class ModeledFullPathInfo
    {
        public ModeledPathProperty ModelingCapability { get; set; }
        public GetFullPathDlgt? GetFullPath { get; set; }
    }
}
