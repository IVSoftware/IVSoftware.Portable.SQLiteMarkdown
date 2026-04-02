using IVSoftware.Portable.SQLiteMarkdown.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{

    internal delegate string GetPathDlgt(object o);
    internal class ModeledFullPathInfo
    {
        public StdModelPath StdModelPath { get; set; }
        public GetPathDlgt? GetPath { get; set; }
    }
}
