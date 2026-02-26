using SQLite;
using System;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    static class SQLiteTableMappingCache
    {
        public static TableMapping GetMapping(this Type type) => Cache.GetMapping(type);
        private static SQLiteConnection Cache
        {
            get
            {
                if (_cache is null)
                {
                    _cache = new SQLiteConnection(":memory:");
                }
                return _cache;
            }
        }
        private static SQLiteConnection? _cache = null;
    }
}
