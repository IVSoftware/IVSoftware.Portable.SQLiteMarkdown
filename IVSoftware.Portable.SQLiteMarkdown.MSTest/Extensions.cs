using System.Diagnostics;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    static class SQLiteMarkdownTestExtensions
    {
        public static T DequeueSingle<T>(this Queue<T> queue)
            => queue.Count switch
            {
                0 => throw new InvalidOperationException("Queue is empty."),
                1 => queue.Dequeue(),
                _ => throw new InvalidOperationException("Multiple items in queue."),
            };
    }
}
