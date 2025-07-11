using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest.OP
{
    /// <summary>
    /// Identifies a collection item that has been clicked, 
    /// or null if the click occurred in a blank area.
    /// </summary>
    public class ItemMouseEventArgs
    {
        public ItemMouseEventArgs(object? item) => Item = item;

        public object? Item { get; }
    }
}
